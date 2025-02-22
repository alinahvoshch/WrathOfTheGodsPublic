using Luminance.Common.DataStructures;
using Luminance.Common.Easings;
using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Luminance.Core.Sounds;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Content.Particles;
using NoxusBoss.Content.Particles.Metaballs;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    /// <summary>
    /// The arm used for waving during the redirecting rubble attack.
    /// </summary>
    public ref float RubbleGravitySlam_ArmToWaveWith => ref NPC.ai[2];

    /// <summary>
    /// Whether the player has hit ground yet as a result of strong gravity during the redirecting rubble attack.
    /// </summary>
    public bool RubbleGravitySlam_TargetHasHitGround
    {
        get => NPC.ai[3] == 1f;
        set => NPC.ai[3] = value.ToInt();
    }

    /// <summary>
    /// The looped sound used during the redirecting rubble attack as the player falls.
    /// </summary>
    public LoopedSoundInstance? ExtremeGravityFallLoop
    {
        get;
        set;
    }

    /// <summary>
    /// How long it takes for the Avatar to impose strong gravity during the redirecting rubble attack.
    /// </summary>
    public static int RubbleGravitySlam_GravityDelay => GetAIInt("RubbleGravitySlam_GravityDelay");

    /// <summary>
    /// How long the Avatar imposes strong gravity during the redirecting rubble attack.
    /// </summary>
    public static int RubbleGravitySlam_StrongGravityTime => GetAIInt("RubbleGravitySlam_StrongGravityTime");

    /// <summary>
    /// The amount of time the Avatar waits after slamming the player down before they hand wave the rubble during the redirecting rubble attack.
    /// </summary>
    public static int RubbleGravitySlam_HandWaveDelay => GetAIInt("RubbleGravitySlam_HandWaveDelay");

    /// <summary>
    /// The quantity of rubble created during the redirecting rubble attack.
    /// </summary>
    public static int RubbleGravitySlam_RubbleCount => GetAIInt("RubbleGravitySlam_RubbleCount");

    /// <summary>
    /// The amount of time the Avatar waits after rubble is fired during the redirecting rubble attack.
    /// </summary>
    public static int RubbleGravitySlam_RubbleAccelerateWaitTime => GetAIInt("RubbleGravitySlam_RubbleAccelerateWaitTime");

    /// <summary>
    /// The amount of damage redirecting rubble from the Avatar does.
    /// </summary>
    public static int RedirectingRubbleDamage => GetAIInt("RedirectingRubbleDamage");

    [AutomatedMethodInvoke]
    public void LoadState_RubbleGravitySlam()
    {
        StateMachine.RegisterTransition(AvatarAIType.RubbleGravitySlam_ApplyExtremeGravity, AvatarAIType.RubbleGravitySlam_MoveRubble, false, () =>
        {
            return AITimer >= RubbleGravitySlam_GravityDelay + RubbleGravitySlam_StrongGravityTime;
        });
        StateMachine.RegisterTransition(AvatarAIType.RubbleGravitySlam_MoveRubble, null, false, () =>
        {
            return AITimer >= RubbleGravitySlam_RubbleAccelerateWaitTime;
        });

        StatesToNotStartTeleportDuring.Add(AvatarAIType.RubbleGravitySlam_ApplyExtremeGravity);
        StatesToNotStartTeleportDuring.Add(AvatarAIType.RubbleGravitySlam_MoveRubble);

        // Load the AI state behaviors.
        StateMachine.RegisterStateBehavior(AvatarAIType.RubbleGravitySlam_ApplyExtremeGravity, DoBehavior_RubbleGravitySlam_ApplyExtremeGravity);
        StateMachine.RegisterStateBehavior(AvatarAIType.RubbleGravitySlam_MoveRubble, DoBehavior_RubbleGravitySlam_MoveRubble);
    }

    public void DoBehavior_RubbleGravitySlam_ApplyExtremeGravity()
    {
        // Calculate body part variables.
        Vector2 leftArmHoverOffset = Vector2.Zero;
        Vector2 rightArmHoverOffset = Vector2.Zero;

        if (AITimer == 1)
        {
            RubbleGravitySlam_ArmToWaveWith = Main.rand.NextFromList(-1, 1);
            RubbleGravitySlam_TargetHasHitGround = false;
            NPC.netUpdate = true;
        }

        // Do a downward pose and cause gravity to become far, far stronger for players.
        if (AITimer >= RubbleGravitySlam_GravityDelay)
        {
            leftArmHoverOffset.X += Main.rand.NextFloatDirection() * 10f + 85f;
            rightArmHoverOffset.X -= Main.rand.NextFloatDirection() * 10f + 85f;

            if (RubbleGravitySlam_ArmToWaveWith == 1)
            {
                leftArmHoverOffset.Y -= 150f;
                rightArmHoverOffset.Y += 580f;
            }
            else
            {
                leftArmHoverOffset.Y += 580f;
                rightArmHoverOffset.Y -= 150f;
                HandGraspAngle = InverseLerp(0f, 10f, AITimer - RubbleGravitySlam_GravityDelay) * -0.23f;
            }
        }

        // Update the looping sound.
        ExtremeGravityFallLoop?.Update(Main.LocalPlayer.Center, sound =>
        {
            float fallSpeedInterpolant = InverseLerp(25f, 130f, Main.LocalPlayer.velocity.Y);
            sound.Pitch = fallSpeedInterpolant * 0.74f;
            sound.Volume = Lerp(1f, 2.9f, fallSpeedInterpolant);
        });

        // Hover near the player.
        ZPosition = Lerp(ZPosition, 3f, 0.2f);

        // Reset all distortion.
        IdealDistortionIntensity = 0f;

        // Impose gravity.
        if (AITimer == RubbleGravitySlam_GravityDelay)
        {
            ExtremeGravityFallLoop?.Stop();
            ExtremeGravityFallLoop = LoopedSoundManager.CreateNew(GennedAssets.Sounds.Avatar.GravitySlamFallLoop, () => !NPC.active || CurrentState != AvatarAIType.RubbleGravitySlam_ApplyExtremeGravity);

            IProjOwnedByBoss<AvatarOfEmptiness>.KillAll();

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead)
                    continue;

                Vector2 startingPortalPosition = FindGroundVertical(p.Top.ToTileCoordinates()).ToWorldCoordinates();
                Vector2 endingTeleportPosition = FindGroundVertical(new((int)(p.Center.X / 16f), (int)Main.worldSurface - 270)).ToWorldCoordinates();
                if (startingPortalPosition.WithinRange(endingTeleportPosition, 732f))
                    continue;

                NewProjectileBetter(NPC.GetSource_FromAI(), startingPortalPosition, -Vector2.UnitY, ModContent.ProjectileType<DarkPortal>(), 0, 0f, i, 1.1f, RubbleGravitySlam_StrongGravityTime, (int)DarkPortal.PortalAttackAction.TeleportPlayerThroughPortal);
                NewProjectileBetter(NPC.GetSource_FromAI(), endingTeleportPosition, -Vector2.UnitY, ModContent.ProjectileType<DarkPortal>(), 0, 0f, i, 1.1f, RubbleGravitySlam_StrongGravityTime, (int)DarkPortal.PortalAttackAction.TeleportPlayerOutOfPortal);
            }
        }

        // Apply extremely strong gravity.
        if (AITimer >= RubbleGravitySlam_GravityDelay && AITimer < RubbleGravitySlam_GravityDelay + RubbleGravitySlam_StrongGravityTime)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead)
                    continue;

                // Create impact effects and terminate this state immediately if the player has hit ground due to the gravity.
                bool hitWater = Collision.WetCollision(p.BottomLeft, p.width, 30);
                bool hitGround = Collision.SolidCollision(p.BottomLeft, p.width, 30, true);
                Tile groundTile = Framing.GetTileSafely((int)(p.Center.X / 16f), (int)(p.Bottom.Y / 16f) + 1);
                if (TileID.Sets.Platforms[groundTile.TileType] && groundTile.HasUnactuatedTile)
                    hitGround = true;

                bool endGravity = hitWater || hitGround;
                if (endGravity && !RubbleGravitySlam_TargetHasHitGround)
                {
                    AITimer = RubbleGravitySlam_GravityDelay + RubbleGravitySlam_StrongGravityTime - 1;
                    RubbleGravitySlam_TargetHasHitGround = true;
                    ExtremeGravityFallLoop?.Stop();
                    ExtremeGravityFallLoop = null;

                    if (!hitWater)
                        DoBehavior_RubbleGravitySlam_CreateImpactEffects(p);
                    NPC.netUpdate = true;
                }

                p.mount?.Dismount(p);
                p.velocity.Y += 3.2f;
                p.GetModPlayer<PlayerDataManager>().MaxFallSpeedBoost.Value = 130f;
            }
            AvatarOfEmptinessSky.WindSpeedFactor = Lerp(AvatarOfEmptinessSky.WindSpeedFactor, 5f, 0.15f);
            AvatarOfEmptinessSky.WindVerticalStretchFactor = Lerp(AvatarOfEmptinessSky.WindVerticalStretchFactor, 5.41f, 0.034f);
        }
        else
        {
            NPC.Center = Vector2.Lerp(NPC.Center, Target.Center - Vector2.UnitY * 200f, 0.5f);
            NPC.velocity *= 0.85f;
        }

        // Update limbs.
        PerformBasicHeadUpdates(1.9f);
        PerformBasicFrontArmUpdates(1.9f, leftArmHoverOffset, rightArmHoverOffset);
    }

    public void DoBehavior_RubbleGravitySlam_MoveRubble()
    {
        HandGraspAngle = InverseLerp(27f, 0f, AITimer - RubbleGravitySlam_HandWaveDelay) * -0.23f;

        if (AITimer == 1)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < RubbleGravitySlam_RubbleCount; i++)
                {
                    float groundOffset = Main.rand.NextFloat(250f, 1550f) * Main.rand.NextFromList(-1f, 1f);
                    Point groundSearchPoint = (Target.Center + Vector2.UnitX * groundOffset).ToTileCoordinates();
                    Vector2 groundPosition = FindGroundVerticalPlatforms(groundSearchPoint).ToWorldCoordinates();

                    float angularVarianceFactor = InverseLerp(300f, 500f, Abs(groundOffset));
                    Vector2 rubbleVelocity = -Vector2.UnitY.RotatedByRandom(angularVarianceFactor * 0.23f) * 32f;

                    NewProjectileBetter(NPC.GetSource_FromAI(), groundPosition, rubbleVelocity, ModContent.ProjectileType<RedirectingRubble>(), RedirectingRubbleDamage, 0f);
                }
            }

            ScreenShakeSystem.StartShake(18f, TwoPi, null, 0.56f);
            RubbleGravitySlam_TargetHasHitGround = false;
            NPC.netUpdate = true;
        }

        float handWaveInterpolant = EasingCurves.Cubic.Evaluate(EasingType.InOut, InverseLerp(0f, 60f, AITimer - RubbleGravitySlam_HandWaveDelay));
        Vector2 leftArmOffset = Vector2.UnitX * 100f;
        Vector2 rightArmOffset = Vector2.UnitX * -100f;
        if (RubbleGravitySlam_ArmToWaveWith == -1f)
        {
            leftArmOffset.X -= handWaveInterpolant * 560f;
            leftArmOffset.Y -= handWaveInterpolant * 300f;
            rightArmOffset.X = 0f;
        }
        else
        {
            rightArmOffset.X += handWaveInterpolant * 560f;
            rightArmOffset.Y -= handWaveInterpolant * 300f;
            leftArmOffset.X = 0f;
        }

        if (AITimer == RubbleGravitySlam_HandWaveDelay)
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.RocksRedirect with { Volume = 1.5f }, Target.Center);

        if (handWaveInterpolant > 0f && handWaveInterpolant < 1f)
        {
            int rubbleID = ModContent.ProjectileType<RedirectingRubble>();
            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (projectile.type == rubbleID)
                {
                    Vector2 idealVelocity = projectile.SafeDirectionTo(Target.Center) * handWaveInterpolant * 23f;
                    projectile.velocity = Vector2.Lerp(projectile.velocity, idealVelocity, handWaveInterpolant * 0.14f);
                    projectile.As<RedirectingRubble>().CurrentState = RedirectingRubble.RubbleAIState.AccelerateForward;
                }
            }
        }

        // Update limbs.
        PerformBasicFrontArmUpdates(3f, leftArmOffset, rightArmOffset);
        PerformBasicHeadUpdates(6f);
    }

    /// <summary>
    /// Shifts a point until it reaches level ground. Unlike the Luminance version of this method, this includes platforms.
    /// </summary>
    /// <param name="p">The original point.</param>
    public static Point FindGroundVerticalPlatforms(Point p)
    {
        // The tile is solid. Check up to verify that this tile is not inside of solid ground.
        if (Collision.SolidCollision(p.ToWorldCoordinates(), 1, 1))
        {
            bool solidGround = true;
            while (solidGround && p.Y >= 1)
            {
                p.Y--;
                Vector2 worldPosition = p.ToVector2() * 16f;
                solidGround = Collision.SolidCollision(worldPosition - Vector2.UnitY * 16f, 16, 16, true);

                Tile checkTile = Framing.GetTileSafely(p);
                if (checkTile.HasUnactuatedTile && TileID.Sets.Platforms[checkTile.TileType])
                    solidGround = true;
            }
        }

        // The tile is not solid. Check down to verify that this tile is not above ground in the middle of the air.
        else
        {
            bool solidGround = false;
            while (!solidGround && p.Y < Main.maxTilesY)
            {
                p.Y++;
                Vector2 worldPosition = p.ToVector2() * 16f;
                solidGround = Collision.SolidCollision(worldPosition - Vector2.UnitY * 16f, 16, 16, true);

                Tile checkTile = Framing.GetTileSafely(p);
                if (checkTile.HasUnactuatedTile && TileID.Sets.Platforms[checkTile.TileType])
                    solidGround = true;
            }
        }

        return p;
    }

    public static Color DoBehavior_RubbleGravitySlam_CalculateImpactColor(Tile tile)
    {
        // Switch expression syntax looks disgusting when applied here.
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (tile.TileType)
        {
            case TileID.SnowBlock:
            case TileID.IceBlock:
                return Color.Lerp(Color.White, new(172, 255, 255), Main.rand.NextFloat(0.6f));
            case TileID.CorruptGrass:
            case TileID.Ebonstone:
            case TileID.Ebonsand:
                return Color.Lerp(new(87, 57, 114), new(82, 50, 145), Main.rand.NextFloat(0.5f));
            case TileID.CrimsonGrass:
            case TileID.Crimstone:
            case TileID.Crimsand:
                return Color.Lerp(new(91, 23, 23), new(128, 26, 28), Main.rand.NextFloat(0.5f));
        }
#pragma warning restore IDE0066 // Convert switch statement to expression

        return Color.Lerp(Color.SaddleBrown, Color.DarkGray, Main.rand.NextFloat(0.5f));
    }

    /// <summary>
    /// Creates impact particles and sounds for the redirecting rubble attack.
    /// </summary>
    /// <param name="player">The player to create effects relative to.</param>
    public static void DoBehavior_RubbleGravitySlam_CreateImpactEffects(Player player)
    {
        ModContent.GetInstance<TileDistortionMetaball>().CreateParticle(player.Bottom, Vector2.Zero, 120f);
        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.GravitySlamImpact, player.Center);

        for (float dx = -800f; dx < 800f; dx += Main.rand.NextFloat(14f, 31f))
        {
            float dustSizeInterpolant = Main.rand.NextFloat();
            float dustScale = Lerp(1f, 2f, dustSizeInterpolant);
            Vector2 dustVelocity = (-Vector2.UnitY * Main.rand.NextFloat(30f, 44f) + Main.rand.NextVector2Circular(10f, 20f)) / dustScale;

            Point groundSearchPoint = (player.Center + Vector2.UnitX * dx).ToTileCoordinates();
            Point groundTilePosition = FindGroundVerticalPlatforms(groundSearchPoint);
            groundTilePosition.Y++;

            Tile tile = Framing.GetTileSafely(groundTilePosition);
            Color dustColor = DoBehavior_RubbleGravitySlam_CalculateImpactColor(tile);

            Vector2 groundPosition = groundTilePosition.ToWorldCoordinates();
            SmallSmokeParticle dust = new SmallSmokeParticle(groundPosition, dustVelocity, dustColor, Color.Transparent, dustScale, 200f);
            dust.Spawn();

            for (int j = 0; j < 3; j++)
                WorldGen.KillTile_MakeTileDust(groundTilePosition.X, groundTilePosition.Y, tile);
        }

        for (int i = 0; i < 32; i++)
        {
            Vector2 dustSpawnPosition = player.Center + Vector2.UnitY * 20f + Main.rand.NextVector2Circular(10f, 4f);
            Tile groundTilePosition = Framing.GetTileSafely((dustSpawnPosition + Vector2.UnitY * 24f).ToTileCoordinates());
            Color dustColor = DoBehavior_RubbleGravitySlam_CalculateImpactColor(groundTilePosition);

            Vector2 dustVelocity = Vector2.UnitX.RotatedByRandom(0.2f) * Main.rand.NextFloat(5f, 67f) * Main.rand.NextFromList(-1f, 1f);
            float dustScale = dustVelocity.Length() / 32f;
            SmallSmokeParticle impactDust = new SmallSmokeParticle(dustSpawnPosition, dustVelocity, dustColor, Color.Transparent, dustScale, 200f);
            impactDust.Spawn();
        }
    }
}
