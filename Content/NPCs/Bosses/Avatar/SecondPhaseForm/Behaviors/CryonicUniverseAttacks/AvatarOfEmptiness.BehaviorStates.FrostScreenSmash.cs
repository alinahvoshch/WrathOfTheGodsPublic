using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.Graphics.ScreenShatter;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using NoxusBoss.Core.World.GameScenes.RiftEclipse;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    public record TwinkleSmashTelegraphSet(List<Vector2> FrostOffsets, Vector2 ScreenSmashTeleportOffset, int DangerousTwinkleIndex)
    {
        /// <summary>
        /// Writes this telegraph set to a given <see cref="BinaryWriter"/>, for use with syncing.
        /// </summary>
        /// <param name="writer">The writer to supply telegraph data to.</param>
        public void Write(BinaryWriter writer)
        {
            writer.Write(FrostOffsets.Count);
            for (int i = 0; i < FrostOffsets.Count; i++)
                writer.WriteVector2(FrostOffsets[i]);

            writer.WriteVector2(ScreenSmashTeleportOffset);
            writer.Write(DangerousTwinkleIndex);
        }

        /// <summary>
        /// Reads a telegraph set from a given <see cref="BinaryReader"/>, for use with syncing.
        /// </summary>
        /// <param name="reader">The reader to acquire telegraph data from.</param>
        public static TwinkleSmashTelegraphSet Read(BinaryReader reader)
        {
            int offsetCount = reader.ReadInt32();
            List<Vector2> frostOffsets = new List<Vector2>(offsetCount);
            for (int i = 0; i < offsetCount; i++)
                frostOffsets.Add(reader.ReadVector2());

            Vector2 teleportOffset = reader.ReadVector2();
            int dangerousTwinkleIndex = reader.ReadInt32();

            return new(frostOffsets, teleportOffset, dangerousTwinkleIndex);
        }
    }

    /// <summary>
    /// The slope of the hitbox orientation line used during the Avatar's frost screen smash attack.
    /// </summary>
    public float FrostScreenSmashLineSlope
    {
        get;
        set;
    }

    /// <summary>
    /// The vectoral direction equivalent of the <see cref="FrostScreenSmashLineSlope"/> value.
    /// </summary>
    public Vector2 FrostScreenSmashLineDirection
    {
        get
        {
            float hurtZoneAngle = Atan(FrostScreenSmashLineSlope);
            Vector2 hurtZoneDirection = hurtZoneAngle.ToRotationVector2();
            return hurtZoneDirection;
        }
    }

    /// <summary>
    /// The center point of the hitbox orientation line used during the Avatar's frost screen smash attack.
    /// </summary>
    public Vector2 FrostScreenSmashLineCenter
    {
        get;
        set;
    }

    /// <summary>
    /// The details of all twinkle smashes the Avatar will perform.
    /// </summary>
    public List<TwinkleSmashTelegraphSet> TwinkleSmashDetails = [];

    /// <summary>
    /// The amount of time the Avatar spends creating fog during the frost screen smash attack.
    /// </summary>
    public static int FrostScreenSmash_CreateFrostTime => GetAIInt("FrostScreenSmash_CreateFrostTime");

    /// <summary>
    /// The amount of time the Avatar spends entering the background during the frost screen smash attack.
    /// </summary>
    public static int FrostScreenSmash_EnterBackgroundTime => GetAIInt("FrostScreenSmash_EnterBackgroundTime");

    /// <summary>
    /// How long telegraph twinkles linger during the frost screen smash attack.
    /// </summary>
    public static int FrostScreenSmash_TelegraphWaitTime => GetAIInt("FrostScreenSmash_TelegraphWaitTime");

    /// <summary>
    /// How long the Avatar waits before casting telegraphs during the frost screen smash attack.
    /// </summary>
    public static int FrostScreenSmash_TelegraphCastDelay => GetAIInt("FrostScreenSmash_TelegraphCastDelay");

    /// <summary>
    /// The amount of twinkle telegraphs the Avatar creates during the frost screen smash attack.
    /// </summary>
    public static int FrostScreenSmash_TwinkleCount => GetAIInt("FrostScreenSmash_TwinkleCount");

    /// <summary>
    /// The amount of smashes that the Avatar performs during the frost screen smash attack.
    /// </summary>
    public static int FrostScreenSmash_SmashCount => GetAIInt("FrostScreenSmash_SmashCount");

    /// <summary>
    /// The amount of time the Avatar spends dashing during the frost screen smash attack.
    /// </summary>
    public static int FrostScreenSmash_ForegroundDashDuration => GetAIInt("FrostScreenSmash_ForegroundDashDuration");

    /// <summary>
    /// The amount of time the Avatar spends waiting between dashes during the frost screen smash attack.
    /// </summary>
    public static int FrostScreenSmash_SmashForegroundAgainDelay => GetAIInt("FrostScreenSmash_SmashForegroundAgainDelay");

    /// <summary>
    /// The color of cyan twinkles.
    /// </summary>
    public static Color FrostScreenSmash_CyanSparkleColor => new(0.09f, 0.9f, 1f);

    /// <summary>
    /// The color of red twinkles.
    /// </summary>
    public static Color FrostScreenSmash_RedSparkleColor => new(0.91f, 0.02f, 0.22f);

    /// <summary>
    /// The maximum frost intensity during the frost screen smash attack.
    /// </summary>
    public static float FrostScreenSmash_MaxFrostIntensity => 0.66f;

    [AutomatedMethodInvoke]
    public void LoadState_FrostScreenSmash()
    {
        StateMachine.RegisterTransition(AvatarAIType.FrostScreenSmash_CreateFrost, null, false, () =>
        {
            return Main.netMode != NetmodeID.SinglePlayer;
        });

        StateMachine.RegisterTransition(AvatarAIType.FrostScreenSmash_CreateFrost, AvatarAIType.FrostScreenSmash_EnterBackground, false, () =>
        {
            return AITimer >= FrostScreenSmash_CreateFrostTime;
        });
        StateMachine.RegisterTransition(AvatarAIType.FrostScreenSmash_EnterBackground, AvatarAIType.FrostScreenSmash_TwinkleTelegraphs, false, () =>
        {
            return AITimer >= FrostScreenSmash_EnterBackgroundTime;
        });
        StateMachine.RegisterTransition(AvatarAIType.FrostScreenSmash_TwinkleTelegraphs, AvatarAIType.FrostScreenSmash_SmashForeground, false, () =>
        {
            return AITimer >= FrostScreenSmash_TelegraphWaitTime * FrostScreenSmash_SmashCount + FrostScreenSmash_TelegraphCastDelay;
        });
        StateMachine.RegisterTransition(AvatarAIType.FrostScreenSmash_SmashForeground, null, false, () =>
        {
            return TwinkleSmashDetails.Count <= 0;
        }, () =>
        {
            NPC.Center = Target.Center - Vector2.UnitY * 1400f;
        });

        // Load the AI state behaviors.
        StateMachine.RegisterStateBehavior(AvatarAIType.FrostScreenSmash_CreateFrost, DoBehavior_FrostScreenSmash_CreateFrost);
        StateMachine.RegisterStateBehavior(AvatarAIType.FrostScreenSmash_EnterBackground, DoBehavior_FrostScreenSmash_EnterBackground);
        StateMachine.RegisterStateBehavior(AvatarAIType.FrostScreenSmash_TwinkleTelegraphs, DoBehavior_FrostScreenSmash_TwinkleTelegraphs);
        StateMachine.RegisterStateBehavior(AvatarAIType.FrostScreenSmash_SmashForeground, DoBehavior_FrostScreenSmash_SmashForeground);

        AttackDimensionRelationship[AvatarAIType.FrostScreenSmash_CreateFrost] = AvatarDimensionVariants.CryonicDimension;
        AttackDimensionRelationship[AvatarAIType.FrostScreenSmash_EnterBackground] = AvatarDimensionVariants.CryonicDimension;
        AttackDimensionRelationship[AvatarAIType.FrostScreenSmash_TwinkleTelegraphs] = AvatarDimensionVariants.CryonicDimension;
        AttackDimensionRelationship[AvatarAIType.FrostScreenSmash_SmashForeground] = AvatarDimensionVariants.CryonicDimension;
    }

    public void DoBehavior_FrostScreenSmash_CreateFrost()
    {
        if (AITimer == 1)
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.FogRelease with { Volume = 1.3f });

        // Stay in the background.
        ZPosition = Lerp(ZPosition, 7.5f, 0.15f);

        // Make the frost appear.
        RiftEclipseFogShaderData.FogDrawIntensityOverride = InverseLerp(0f, FrostScreenSmash_CreateFrostTime, AITimer).Squared() * FrostScreenSmash_MaxFrostIntensity;

        // Slow down.
        NPC.velocity *= 0.9f;

        // Stay invisible.
        NPC.Opacity = 0f;

        // Grip hands.
        HandGraspAngle = HandGraspAngle.AngleTowards(-0.31f, 0.05f);

        // Update limbs.
        PerformStandardLimbUpdates();
    }

    public void DoBehavior_FrostScreenSmash_EnterBackground()
    {
        // Grip hands.
        HandGraspAngle = HandGraspAngle.AngleTowards(-0.31f, 0.09f);

        // Enter the background.
        float animationCompletion = InverseLerp(0f, FrostScreenSmash_EnterBackgroundTime - 60f, AITimer);
        ZPosition = MathF.Max(ZPosition, animationCompletion.Cubed() * 11f);

        // Drift above the player.
        NPC.SmoothFlyNear(Target.Center - Vector2.UnitY * 396f, 0.032f, 0.95f);

        // Stay invisible.
        NPC.Opacity = 0f;

        // Keep the frost in place.
        RiftEclipseFogShaderData.FogDrawIntensityOverride = FrostScreenSmash_MaxFrostIntensity;

        // Create a red sparkle at the Avatar's position as he completely disappears.
        if (AITimer == FrostScreenSmash_EnterBackgroundTime - 60)
        {
            TwinkleParticle redTwinkle = new TwinkleParticle(NPC.Center, Vector2.Zero, FrostScreenSmash_RedSparkleColor, 60, 6, Vector2.One * 2f, Color.White * 0.5f);
            redTwinkle.Spawn();
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle with { MaxInstances = 5, PitchVariance = 0.16f, Pitch = -0.26f });
        }

        // Update limbs.
        PerformStandardLimbUpdates();
    }

    public void DoBehavior_FrostScreenSmash_TwinkleTelegraphs()
    {
        // Stay invisible.
        NPC.Opacity = 0f;

        // Stay far, far in the background.
        ZPosition = 50f;

        // Create telegraphs on the first frame.
        if ((AITimer - FrostScreenSmash_TelegraphCastDelay) % FrostScreenSmash_TelegraphWaitTime == 1 && AITimer >= FrostScreenSmash_TelegraphCastDelay)
        {
            // Chirp and shake the screen slightly.
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Chirp with { Volume = 1.75f });
            ScreenShakeSystem.StartShake(4f);

            // Decide offset positions.
            int dangerousTwinkleIndex = Main.rand.Next(FrostScreenSmash_TwinkleCount);
            float frostOffsetAngle = Main.rand.NextFloat(TwoPi);
            List<Vector2> frostOffsets = [];
            for (int i = 0; i < FrostScreenSmash_TwinkleCount; i++)
            {
                Vector2 baseOffset = (TwoPi * i / FrostScreenSmash_TwinkleCount + frostOffsetAngle).ToRotationVector2() * new Vector2(432f, 432f);
                frostOffsets.Add(baseOffset + Main.rand.NextVector2Circular(10f, 10f));
            }

            int previousTwinkleIndex = -1;
            if (TwinkleSmashDetails.Count >= 1)
                previousTwinkleIndex = TwinkleSmashDetails.Last().DangerousTwinkleIndex;

            // Ensure that the dangerous twinkle index favors the sides.
            do
            {
                dangerousTwinkleIndex = Main.rand.Next(FrostScreenSmash_TwinkleCount);
            }
            while (Abs(Sin(TwoPi * dangerousTwinkleIndex / FrostScreenSmash_TwinkleCount + frostOffsetAngle)) >= 0.7f || dangerousTwinkleIndex == previousTwinkleIndex);

            // Create telegraph twinkles.
            Vector2 screenSmashTeleportOffset = Vector2.Zero;
            for (int i = 0; i < frostOffsets.Count; i++)
            {
                Color twinkleColor = FrostScreenSmash_CyanSparkleColor;
                twinkleColor.R += (byte)Main.rand.Next(30);

                if (i == dangerousTwinkleIndex)
                {
                    twinkleColor = FrostScreenSmash_RedSparkleColor;
                    screenSmashTeleportOffset = frostOffsets[i];
                    NPC.netUpdate = true;
                }

                TwinkleParticle twinkle = new TwinkleParticle(NPC.Center + frostOffsets[i], Vector2.Zero, twinkleColor, FrostScreenSmash_TelegraphWaitTime - Main.rand.Next(24), 6, Vector2.One * 2f, Color.White * 0.5f, new(frostOffsets[i], () =>
                {
                    return Target.Center;
                }));
                twinkle.Spawn();
            }
            TwinkleSmashDetails.Add(new(frostOffsets, screenSmashTeleportOffset, dangerousTwinkleIndex));

            // Play a twinkle sound.
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle with { MaxInstances = 5, PitchVariance = 0.16f, Pitch = -0.26f });
        }

        // Keep the frost in place.
        RiftEclipseFogShaderData.FogDrawIntensityOverride = FrostScreenSmash_MaxFrostIntensity;

        // Update limbs.
        // Technically, the Avatar is invisible during this state but it's good to ensure that when he isn't his body parts aren't comically offset from him.
        PerformStandardLimbUpdates();
    }

    public void DoBehavior_FrostScreenSmash_SmashForeground()
    {
        SolynAction = s => StandardSolynBehavior_FlyNearPlayer(s, NPC);

        // Enable damage if in the foreground.
        Vector2 perpendicularHurtZoneDirection = FrostScreenSmashLineDirection.RotatedBy(-PiOver2);
        if (ZPosition < 0.1f && ZPosition >= -0.89f)
            NPC.damage = NPC.defDamage;
        else
            NPC.damage = 0;
        if (ZPosition >= 4f)
            FrostScreenSmashLineCenter = Target.Center + perpendicularHurtZoneDirection * 56f;

        // Start the teleport jumpscare on the first frame.
        int wrappedTimer = AITimer % (FrostScreenSmash_ForegroundDashDuration + FrostScreenSmash_SmashForegroundAgainDelay);
        if (wrappedTimer == 1 && TwinkleSmashDetails.Count != 0)
        {
            ZPosition = 9f;
            NPC.Opacity = 1f;
            NPC.Center = Target.Center + TwinkleSmashDetails[0].ScreenSmashTeleportOffset - Vector2.UnitY * 70f;
            NPC.velocity = NPC.SafeDirectionTo(Target.Center) * (Target.Velocity.Length() * 0.08f + 32.5f);
            FrostScreenSmashLineSlope = Tan(TwinkleSmashDetails[0].ScreenSmashTeleportOffset.ToRotation() + PiOver2);

            NPC.netUpdate = true;

            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.JumpscareWeak);
            GeneralScreenEffectSystem.RadialBlur.Start(NPC.Center, 1f, 20);
            GeneralScreenEffectSystem.ChromaticAberration.Start(NPC.Center, 1.3f, 60);
            ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 17f);
        }

        // Slow down.
        NPC.velocity *= 0.8f;

        // Disable incoming damage, to prevent ram dash cheesing.
        NPC.dontTakeDamage = true;

        // Update limbs.
        float verticalOffset = Cos01(TwoPi * FightTimer / 90f) * 12f + 200f;
        HeadPosition = Vector2.Lerp(HeadPosition, NPC.Center + Vector2.UnitY * verticalOffset * HeadScale * NeckAppearInterpolant, 0.3f);

        // Disable distortion damage effects.
        IdealDistortionIntensity = 0f;

        // Move the arms up and down.
        PerformBasicFrontArmUpdates(2.6f, Vector2.UnitX * 90f, Vector2.UnitX * -90f);

        LegScale = Vector2.Zero;
        LeftFrontArmScale = 0.4f;
        RightFrontArmScale = 0.4f;
        DrawnAsSilhouette = true;

        // Smash into the screen incredibly quickly.
        ZPosition = Clamp(ZPosition - 0.67f, -0.9f, 9f);
        if (wrappedTimer >= FrostScreenSmash_ForegroundDashDuration - 2)
        {
            LeftFrontArmScale = 1f;
            RightFrontArmScale = 1f;
            LegScale = Vector2.One;
            NPC.Opacity = 0f;
        }

        // Keep the frost in place.
        RiftEclipseFogShaderData.FogDrawIntensityOverride = FrostScreenSmash_MaxFrostIntensity;

        // Shatter the screen.
        if (wrappedTimer == FrostScreenSmash_ForegroundDashDuration - 2)
        {
            RadialScreenShoveSystem.Start(NPC.Center, 120);
            ScreenShatterSystem.CreateShatterEffect(NPC.Center - Main.screenPosition, false, 1, 7);
            AvatarPhase2KeyboardShader.KeyboardBrightnessIntensity = 1f;
            if (TwinkleSmashDetails.Count >= 1)
                TwinkleSmashDetails.RemoveAt(0);
            else
                ZPosition = 0f;
        }
    }
}
