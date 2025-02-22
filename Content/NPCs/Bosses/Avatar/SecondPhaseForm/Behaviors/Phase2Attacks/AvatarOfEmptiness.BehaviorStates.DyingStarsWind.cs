using Luminance.Common.DataStructures;
using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    /// <summary>
    /// The offset factor for shadow hands during the Avatar's Dying Star's Wind attack.
    /// </summary>
    public ref float DyingStarsWind_HandOffsetFactor => ref NPC.ai[2];

    /// <summary>
    /// How long it takes, in frames, for the star to appear and begin animating.
    /// </summary>
    public static int DyingStarsWind_GrowDelay => GetAIInt("DyingStarsWind_GrowDelay");

    /// <summary>
    /// How long it takes, in frames, for the star to grow to its full size.
    /// </summary>
    public static int DyingStarsWind_GrowToFullSizeTime => GetAIInt("DyingStarsWind_GrowToFullSizeTime");

    /// <summary>
    /// How long it takes, in frames, for the star to go from a burning star to a cold iron exterior.
    /// </summary>
    public static int DyingStarsWind_DeathDelay => GetAIInt("DyingStarsWind_DeathDelay");

    /// <summary>
    /// How long it takes, in frames, for the star to cool down to its iron shell.
    /// </summary>
    public static int DyingStarsWind_DeathCoolTime => GetAIInt("DyingStarsWind_DeathCoolTime");

    /// <summary>
    /// How long it takes, in frames, for the iron star to begin to crack after dying.
    /// </summary>
    public static int DyingStarsWind_CrackDelay => GetAIInt("DyingStarsWind_CrackDelay");

    /// <summary>
    /// How long it takes, in frames, for shoot behaviors to begin happening after dying.
    /// </summary>
    public static int DyingStarsWind_ShootDelay => GetAIInt("DyingStarsWind_ShootDelay");

    /// <summary>
    /// How long, in frames, telegraphs should last.
    /// </summary>
    public static int DyingStarsWind_TelegraphTime => GetAIInt("DyingStarsWind_TelegraphTime");

    /// <summary>
    /// How long it takes for the star to collapse.
    /// </summary>
    public static int DyingStarsWind_CollapseDelay => GetAIInt("DyingStarsWind_CollapseDelay");

    /// <summary>
    /// How long it takes for the star to shatter.
    /// </summary>
    public static int DyingStarsWind_ShatterDelay => GetAIInt("DyingStarsWind_ShatterDelay");

    /// <summary>
    /// How long it takes for the Avatar to transition to the next attack after the star is dead.
    /// </summary>
    public static int DyingStarsWind_AttackTransitionDelay => GetAIInt("DyingStarsWind_AttackTransitionDelay");

    /// <summary>
    /// The speed factor for the Avatar when following his target during the Dying Star's Wind attack
    /// </summary>
    public static float DyingStarsWind_VerticalSpeedTrackingFactor => GetAIFloat("DyingStarsWind_VerticalSpeedTrackingFactor");

    /// <summary>
    /// The speed factor for the Avatar when following his target during the Dying Star's Wind attack
    /// </summary>
    public static float DyingStarsWind_IronChunkBurstAngle => GetAIFloat("DyingStarsWind_IronChunkBurstAngle");

    /// <summary>
    /// The amount of damage decayed iron from the dead star the Avatar creates does.
    /// </summary>
    public static int IronDamage => GetAIInt("IronDamage");

    /// <summary>
    /// The amount of damage energy bursts from the dead star the Avatar creates do.
    /// </summary>
    public static int DyingStarBurstDamage => GetAIInt("DyingStarBurstDamage");

    [AutomatedMethodInvoke]
    public void LoadState_DyingStarsWind()
    {
        StateMachine.RegisterTransition(AvatarAIType.DyingStarsWind, null, false, () =>
        {
            return AITimer >= DyingStarsWind_AttackTransitionDelay;
        }, () => ShadowArms?.Clear());

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(AvatarAIType.DyingStarsWind, DoBehavior_DyingStarsWind);
    }

    public void DoBehavior_DyingStarsWind()
    {
        LookAt(Target.Center);

        // Teleport below the player on the first frame.
        if (AITimer == 1)
            StartTeleportAnimation(() => Target.Center + Vector2.UnitY * 1050f);

        ZPosition = Lerp(ZPosition, 0f, 0.24f);

        // Create the dying star on the third frame.
        if (AITimer == 3)
        {
            // Kill leftover projectiles.
            IProjOwnedByBoss<AvatarOfEmptiness>.KillAll();

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DeadStar>(), DyingStarBurstDamage, 0f);

                for (int i = 0; i < 2; i++)
                    ShadowArms.Add(new(NPC.Center, SpiderLilyPosition - NPC.Center - Vector2.UnitY * 270f));

                NPC.netUpdate = true;
            }
        }

        if (ShadowArms.Count >= 2)
        {
            float scale = 1f;
            float handScale = InverseLerp(DyingStarsWind_AttackTransitionDelay, DyingStarsWind_AttackTransitionDelay - 27f, AITimer);
            if (AnyProjectiles(ModContent.ProjectileType<DeadStar>()))
            {
                Projectile star = AllProjectilesByID(ModContent.ProjectileType<DeadStar>()).First();
                if (star.As<DeadStar>().Time >= DyingStarsWind_GrowDelay + DyingStarsWind_GrowToFullSizeTime)
                    scale = (star.scale * 0.25f).Squared();
            }
            DyingStarsWind_HandOffsetFactor = Lerp(DyingStarsWind_HandOffsetFactor, scale, 0.386f);

            float noiseInterpolant = AperiodicSin(FightTimer / 60f);
            Vector2 crescentOffset = Vector2.UnitX.RotatedBy(noiseInterpolant * PiOver4) * new Vector2(300f, 100f);
            Vector2 recoilOffset = Vector2.UnitX * InverseLerpBump(4f, 11f, 19f, 67f, AITimer) * 300f;

            AvatarShadowArm left = ShadowArms[0];
            left.Center = NPC.Center + (new Vector2(-130f, -820f) + crescentOffset * new Vector2(-1f, 1f)) * new Vector2(DyingStarsWind_HandOffsetFactor, 1f) + recoilOffset * new Vector2(-1f, 1f);
            left.Center = Vector2.Lerp(left.Center, NPC.Center, 1f - handScale);
            left.Scale = handScale * 1.25f;
            left.FlipHandDirection = true;
            left.HandRotationAngularOffset = Pi + 0.3f;
            left.RandomID = 4;

            noiseInterpolant = AperiodicSin(FightTimer / 60f + 451f);
            crescentOffset = Vector2.UnitX.RotatedBy(noiseInterpolant * PiOver4) * new Vector2(300f, 100f);
            AvatarShadowArm right = ShadowArms[1];
            right.Center = NPC.Center + (new Vector2(230f, -820f) + crescentOffset) * new Vector2(DyingStarsWind_HandOffsetFactor, 1f) + recoilOffset;
            right.Center = Vector2.Lerp(right.Center, NPC.Center, 1f - handScale);
            right.Scale = handScale * 1.25f;
            right.VerticalFlip = true;
            right.FlipHandDirection = true;
            right.HandRotationAngularOffset = PiOver2 + 0.4f;
            right.RandomID = 2;
        }

        // Decide the distortion intensity.
        IdealDistortionIntensity = 0.41f;

        // Lock the attack timer in place until the star is gone.
        if (AITimer > 4 && AnyProjectiles(ModContent.ProjectileType<DeadStar>()))
            AITimer = 4;

        if (AITimer >= 75)
            ShadowArms.Clear();

        // Make the lily glow.
        float lilyGlowBoost = InverseLerpBump(0f, 10f, 90f, 180f, AITimer);
        LilyGlowIntensityBoost = MathF.Max(LilyGlowIntensityBoost, lilyGlowBoost + 0.32f);

        // Attempt to hover below the target.
        Vector2 hoverDestination = Target.Center + Vector2.UnitY * NPC.scale * 550f;
        Vector2 idealVelocity = (hoverDestination - NPC.Center) * new Vector2(1f, DyingStarsWind_VerticalSpeedTrackingFactor) * 0.035f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, 0.027f);

        // Update the standard limbs.
        PerformStandardLimbUpdates();
    }
}
