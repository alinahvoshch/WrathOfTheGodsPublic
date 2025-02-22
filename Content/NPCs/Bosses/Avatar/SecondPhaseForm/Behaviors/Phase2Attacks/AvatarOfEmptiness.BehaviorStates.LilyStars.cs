using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Luminance.Core.Sounds;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    /// <summary>
    /// The lily firing sound loop.
    /// </summary>
    public LoopedSoundInstance LilyFiringLoop
    {
        get;
        private set;
    }

    /// <summary>
    /// How long the Avatar spends charging up during his lily blobs attack.
    /// </summary>
    public static int LilyStars_ChargeUpDuration => GetAIInt("LilyStars_ChargeUpDuration");

    /// <summary>
    /// How long the Avatar spends attacking his lily blobs attack.
    /// </summary>
    public static int LilyStars_AttackDuration => GetAIInt("LilyStars_AttackDuration");

    /// <summary>
    /// The maximum intensity of the Avatar's lily during the lily blobs attack.
    /// </summary>
    public static float LilyStars_MaxLilyBrightnessIntensity => 1.05f;

    [AutomatedMethodInvoke]
    public void LoadState_LilyStars()
    {
        StateMachine.RegisterTransition(AvatarAIType.LilyStars_ChargePower, AvatarAIType.LilyStars_ReleaseStars, false, () =>
        {
            return AITimer >= LilyStars_ChargeUpDuration;
        });
        StateMachine.RegisterTransition(AvatarAIType.LilyStars_ReleaseStars, null, false, () =>
        {
            return AITimer >= LilyStars_AttackDuration;
        });

        StatesToNotStartTeleportDuring.Add(AvatarAIType.LilyStars_ChargePower);
        StatesToNotStartTeleportDuring.Add(AvatarAIType.LilyStars_ReleaseStars);

        // Load the AI state behaviors.
        StateMachine.RegisterStateBehavior(AvatarAIType.LilyStars_ChargePower, DoBehavior_LilyStars_ChargePower);
        StateMachine.RegisterStateBehavior(AvatarAIType.LilyStars_ReleaseStars, DoBehavior_LilyStars_ReleaseStars);
    }

    public void DoBehavior_LilyStars_ChargePower()
    {
        // Teleport into the background on the first frame.
        if (AITimer == 2)
        {
            StartTeleportAnimation(() =>
            {
                ZPosition = 1.467f;
                return Target.Center - Vector2.UnitY * 350f;
            });
        }

        SolynAction = s => StandardSolynBehavior_FlyNearPlayer(s, NPC);

        if (AITimer == 5)
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.LilyActivate);

        // Decide the body brightness of the Avatar.
        BodyBrightness = 0.1f;

        // Decide the distortion intensity.
        IdealDistortionIntensity = 0.4f;

        // Clench the fist.
        HandGraspAngle = InverseLerp(0f, 32f, AITimer) * -0.3f;

        // Slow down and charge energy.
        NPC.velocity *= 0.91f;

        // Shake the screen.
        float screenShakeIntensity = InverseLerp(0f, LilyStars_ChargeUpDuration * 0.58f, AITimer).Squared() * 14.5f;
        ScreenShakeSystem.SetUniversalRumble(screenShakeIntensity, TwoPi, null, 0.2f);

        // Make the Avatar's lily glow.
        float lilyGlowIntensity = InverseLerp(0f, LilyStars_ChargeUpDuration * 0.53f, AITimer) * LilyStars_MaxLilyBrightnessIntensity;
        LilyGlowIntensityBoost = MathF.Max(LilyGlowIntensityBoost, lilyGlowIntensity);

        // Make the wind speed up.
        float windSpeedBoost = InverseLerp(0f, LilyStars_ChargeUpDuration * 0.6f, AITimer) * 2f;
        AvatarOfEmptinessSky.WindSpeedFactor = windSpeedBoost + 1f;

        // Update limbs.
        PerformStandardLimbUpdates();
    }

    public void DoBehavior_LilyStars_ReleaseStars()
    {
        int armLifetime = 60;
        int attackDuration = LilyStars_AttackDuration;

        // Decide the body brightness of the Avatar.
        BodyBrightness = 0.1f;

        // Decide the distortion intensity.
        IdealDistortionIntensity = 0.5f;

        SolynAction = s => StandardSolynBehavior_FlyNearPlayer(s, NPC);

        ShadowArms.RemoveAll(a => a.Scale <= 0f);
        if (AITimer % 10 == 0 && AITimer <= LilyStars_AttackDuration - 35)
            ShadowArms.Add(new(NPC.Center, Vector2.Zero));

        foreach (AvatarShadowArm arm in ShadowArms)
        {
            float angleTime = arm.RandomID % 100 + arm.Time * 0.06f;
            float angleOffset = Sin(angleTime) * 0.94f;
            float handOffset = Lerp(500f, 1120f, Sin01(angleTime * 3f));
            float maxScale = Lerp(1.15f, 1.3f, arm.RandomID / 14f % 1f);
            arm.Center = NPC.Center - Vector2.UnitY.RotatedBy(angleOffset) * handOffset;
            arm.Scale = InverseLerp(armLifetime, armLifetime - 16f, arm.Time) * InverseLerp(attackDuration - 3f, attackDuration - 16f, AITimer) * maxScale;
            arm.VerticalFlip = arm.Center.X < NPC.Center.X;
            arm.Time++;
        }

        // Create impact effects on the first frame.
        Vector2 energySource = SpiderLilyPosition + Vector2.UnitY * NPC.scale * 76f;
        if (AITimer == 1)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ExplosionTeleport);
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.LilyFireStart);
            ScreenShakeSystem.StartShake(28f, shakeStrengthDissipationIncrement: 0.4f);
            GeneralScreenEffectSystem.ChromaticAberration.Start(NPC.Center, 3f, 90);

            if (Main.netMode != NetmodeID.MultiplayerClient)
                NewProjectileBetter(NPC.GetSource_FromAI(), energySource, Vector2.Zero, ModContent.ProjectileType<DarkWave>(), 0, 0f);
        }

        // Create roar visuals.
        if (AITimer <= 24 && AITimer % 7 == 0)
        {
            Color burstColor = Color.Lerp(Color.Red, Color.HotPink, Main.rand.NextFloat(0.6f));
            ExpandingChromaticBurstParticle burst = new ExpandingChromaticBurstParticle(HeadPosition - Vector2.UnitY * NPC.scale * 112f, Vector2.Zero, burstColor, 12, 0.1f, 1.3f);
            burst.Spawn();

            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Angry with { MaxInstances = 0 });
        }

        // Unclench fists.
        HandGraspAngle = HandGraspAngle.AngleLerp(0f, 0.071f);

        // Stay behind the target.
        NPC.SmoothFlyNear(Target.Center - Vector2.UnitY * 320f, 0.15f, 0.85f);

        // Keep the lily bright.
        float lilyIntensity = LilyStars_MaxLilyBrightnessIntensity + Sin(TwoPi * FightTimer / 12f) * 0.15f;
        LilyGlowIntensityBoost = Lerp(LilyGlowIntensityBoost, lilyIntensity, 0.33f);

        // Keep the wind fast.
        AvatarOfEmptinessSky.WindSpeedFactor = 2f;

        // Update limbs.
        PerformStandardLimbUpdates(1.9f);

        // Keep the screen shaking.
        ScreenShakeSystem.SetUniversalRumble(5f, TwoPi, null, 0.2f);

        // Keep the screen slightly red tinted.
        TotalScreenOverlaySystem.OverlayColor = Color.Red;
        TotalScreenOverlaySystem.OverlayInterpolant = 0.09f;

        // Initialize and update the firing loop.
        float fireSoundVolume = InverseLerp(-1f, -45f, AITimer - attackDuration) * 2.3f;
        if (LilyFiringLoop is null || LilyFiringLoop.HasBeenStopped)
        {
            LilyFiringLoop = LoopedSoundManager.CreateNew(GennedAssets.Sounds.Avatar.LilyFiringLoop, () =>
            {
                return !NPC.active || CurrentState != AvatarAIType.LilyStars_ReleaseStars;
            });
        }
        LilyFiringLoop?.Update(Target.Center, s => s.Volume = fireSoundVolume);

        // Release lilies.
        if (AITimer % 5 == 0 && AITimer >= 60 && AITimer <= attackDuration - 45)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 aimAheadDirection = Target.Velocity;
                if (AITimer % 25 >= 20)
                    aimAheadDirection = aimAheadDirection.RotatedBy(PiOver2);

                Vector2 lilyStartDestination = Target.Center + aimAheadDirection * Main.rand.NextFloat(36f, 67f) + Main.rand.NextVector2Circular(100f, 100f);
                Vector2 lilyStartingVelocity = Main.rand.NextVector2CircularEdge(20f, 20f);

                if (Main.rand.NextBool(10))
                {
                    lilyStartDestination = Target.Center + Main.rand.NextVector2Circular(74f, 74f);
                    lilyStartingVelocity *= 0.35f;
                }

                NewProjectileBetter(NPC.GetSource_FromAI(), energySource, lilyStartingVelocity, ModContent.ProjectileType<LilyStar>(), DisgustingStarDamage, 0f, -1, ZPosition, lilyStartDestination.X, lilyStartDestination.Y);
            }
        }
    }
}
