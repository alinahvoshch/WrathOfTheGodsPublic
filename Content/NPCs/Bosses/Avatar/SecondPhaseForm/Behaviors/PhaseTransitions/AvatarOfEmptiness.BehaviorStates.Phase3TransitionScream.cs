using Luminance.Common.Easings;
using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.Configuration;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    /// <summary>
    /// How long the Avatar spends charging up energy during his phase 3 transition scream state.
    /// </summary>
    public static int Phase3TransitionScream_ChargeUpTime => GetAIInt("Phase3TransitionScream_ChargeUpTime");

    /// <summary>
    /// The maximum duration that screen blurs can last during the Avatar's phase 3 transition scream state.
    /// </summary>
    public static int Phase3TransitionScream_MaxScreenBlurTime => GetAIInt("Phase3TransitionScream_MaxScreenBlurTime");

    /// <summary>
    /// The rate at which the Avatar screams during his phase 3 transition scream state.
    /// </summary>
    public static int Phase3TransitionScream_ScreamRate => 12;

    /// <summary>
    /// How long the Avatar's screen blur effect goes on for after screaming during his phase 3 transition scream state..
    /// </summary>
    public static int Phase3TransitionScream_ScreenBlurTime => 60;

    /// <summary>
    /// The rate at which the Avatar releases wave particles during his phase 3 transition scream state.
    /// </summary>
    public static int Phase3TransitionScream_WaveReleaseRate => 5;

    /// <summary>
    /// The maximum speed that the Avatar can impart on players when shoving them away during his phase 3 transition scream state.
    /// </summary>
    public static float Phase3TransitionScream_MaxShoveSpeed => 42f;

    [AutomatedMethodInvoke]
    public void LoadState_Phase3TransitionScream()
    {
        StateMachine.RegisterTransition(AvatarAIType.Phase3TransitionScream, null, false, () =>
        {
            return AITimer >= Phase3TransitionScream_ChargeUpTime + Phase3TransitionScream_ScreenBlurTime;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(AvatarAIType.Phase3TransitionScream, DoBehavior_Phase3TransitionScream);
    }

    public void DoBehavior_Phase3TransitionScream()
    {
        // Decide the distortion intensity.
        IdealDistortionIntensity = 0f;
        DistortionIntensity = 0f;

        // Slow down.
        NPC.velocity *= 0.84f;

        // Look at the target.
        LookAt(Target.Center);

        // No.
        NPC.dontTakeDamage = true;

        // Enter the foreground.
        ZPosition = Lerp(ZPosition, 0f, 0.17f);

        // Make the head dangle.
        PerformBasicHeadUpdates(1.3f);
        HeadPosition += Main.rand.NextVector2Circular(4f, 2.5f);

        // Make the Avatar put his hands near his head.
        if (AITimer < Phase3TransitionScream_ChargeUpTime)
        {
            float animationInterpolant = InverseLerp(0f, Phase3TransitionScream_ChargeUpTime, AITimer);
            float stretchInterpolant = EasingCurves.Quintic.Evaluate(EasingType.InOut, animationInterpolant.Squared());
            float stretchOffset = Lerp(Main.rand.NextFloatDirection() * 32f + 120f, 800f, stretchInterpolant);

            Vector2 leftArmDestination = HeadPosition + new Vector2(-stretchOffset, 300f) * NPC.scale * RightFrontArmScale;
            Vector2 rightArmDestination = HeadPosition + new Vector2(stretchOffset, 300f) * NPC.scale * RightFrontArmScale;
            LeftArmPosition = Vector2.Lerp(LeftArmPosition, leftArmDestination, 0.23f) + Main.rand.NextVector2Circular(6f, 2f) * animationInterpolant;
            RightArmPosition = Vector2.Lerp(RightArmPosition, rightArmDestination, 0.23f) + Main.rand.NextVector2Circular(6f, 2f) * animationInterpolant;

            // Shake in place.
            NPC.velocity += Main.rand.NextVector2CircularEdge(1f, 0.4f) * stretchInterpolant * 7f;

            // Shake the screen.
            ScreenShakeSystem.SetUniversalRumble(stretchInterpolant * 9.5f, TwoPi, null, 0.2f);
        }

        else
            PerformBasicFrontArmUpdates(0.3f, new Vector2(-100f, 400f), new Vector2(100f, 400f));

        // Send all players flying away.
        if (AITimer == Phase3TransitionScream_ChargeUpTime)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Shriek with { Volume = 1.72f });
            foreach (Player player in Main.ActivePlayers)
            {
                Vector2 offsetFromAvatar = NPC.Center - player.Center;
                float distanceFromAvatar = offsetFromAvatar.Length();
                Vector2 directionToAvatar = offsetFromAvatar.SafeNormalize(Vector2.UnitY);

                float shoveSpeed = 32000f / Pow(distanceFromAvatar + 1.05f, 0.85f);
                if (shoveSpeed > Phase3TransitionScream_MaxShoveSpeed)
                    shoveSpeed = Phase3TransitionScream_MaxShoveSpeed;

                player.velocity -= directionToAvatar * shoveSpeed;
                player.mount?.Dismount(player);

                if (Main.myPlayer == player.whoAmI)
                {
                    int blurDuration = (int)(InverseLerp(3200f, 1000f, distanceFromAvatar) * Phase3TransitionScream_MaxScreenBlurTime);
                    GeneralScreenEffectSystem.ChromaticAberration.Start(player.Center, 1f, blurDuration);

                    float shakePower = InverseLerp(3200f, 1000f, distanceFromAvatar) * 24f;
                    ScreenShakeSystem.StartShake(shakePower, shakeStrengthDissipationIncrement: 0.75f);
                }
            }
        }

        // Apply a blur effect to the screen.
        int timeSinceScream = AITimer - Phase3TransitionScream_ChargeUpTime;
        float screamInterpolant = InverseLerp(0f, Phase3TransitionScream_ScreenBlurTime, timeSinceScream);
        if (Main.netMode != NetmodeID.Server && !WoTGConfig.Instance.PhotosensitivityMode)
        {
            float blurIntensity = InverseLerpBump(0f, 0.2f, 0.5f, 1f, screamInterpolant);
            if (timeSinceScream < 0)
                blurIntensity = 0f;

            ManagedScreenFilter blurShader = ShaderManager.GetFilter("NoxusBoss.RadialMotionBlurShader");
            blurShader.TrySetParameter("blurIntensity", blurIntensity * 2f);
            blurShader.Activate();
        }

        if (AITimer >= Phase3TransitionScream_ChargeUpTime && AITimer <= Phase3TransitionScream_ChargeUpTime + Phase3TransitionScream_ScreenBlurTime * 0.85f && AITimer % Phase3TransitionScream_WaveReleaseRate == 1)
        {
            float waveExpandRate = screamInterpolant * 0.9f + 1.2f;
            ExpandingChromaticBurstParticle wave = new ExpandingChromaticBurstParticle(HeadPosition, Vector2.Zero, Color.Red, 24, 0.25f, waveExpandRate);
            wave.Spawn();
        }
    }
}
