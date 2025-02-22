using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    private bool shouldStartTeleportAnimation;

    /// <summary>
    /// How long the Avatar has to wait before he can teleport again.
    /// </summary>
    public int TeleportDelayCountdown
    {
        get;
        set;
    }

    /// <summary>
    /// The action the Avatar should perform when teleporting. Return the teleport position when using this.
    /// </summary>
    public Func<Vector2> TeleportAction
    {
        get;
        set;
    }

    /// <summary>
    /// How long the Avatar spends during a teleport animation.
    /// </summary>
    public static int Teleport_TeleportDuration => GetAIInt("Teleport_TeleportDuration");

    [AutomatedMethodInvoke]
    public void LoadState_Teleport()
    {
        // Allow any attack to at any time raise the ShouldStartTeleportAnimation flag to start a teleport.
        // Once the teleport concludes, the previous attack is returned to where it was before.
        StateMachine.ApplyToAllStatesExcept(state =>
        {
            StateMachine.RegisterTransition(state, AvatarAIType.Teleport, true, () => shouldStartTeleportAnimation, () =>
            {
                shouldStartTeleportAnimation = false;
                AITimer = 0;
            });
        });
        StateMachine.RegisterTransition(AvatarAIType.Teleport, null, false, () =>
        {
            return AITimer >= Teleport_TeleportDuration;
        });
        StateMachine.RegisterTransition(AvatarAIType.TeleportAbovePlayer, null, false, () =>
        {
            return AITimer >= (Main.netMode == NetmodeID.SinglePlayer ? 2 : 10);
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(AvatarAIType.Teleport, DoBehavior_Teleport);
        StateMachine.RegisterStateBehavior(AvatarAIType.TeleportAbovePlayer, DoBehavior_TeleportAbovePlayer);
    }

    public void DoBehavior_Teleport()
    {
        LilyGlowIntensityBoost *= 0.85f;

        if (ZPosition < 0f)
            ZPosition = 0f;

        // Keep the distortion intensity locked.
        DistortionIntensity = IdealDistortionIntensity;

        // Slow down.
        NPC.velocity *= 0.83f;

        // Make the bloody tears go away.
        if (BloodyTearsAnimationStartInterpolant > 0f)
        {
            BloodyTearsAnimationEndInterpolant = Saturate(BloodyTearsAnimationEndInterpolant + 0.15f);
            if (BloodyTearsAnimationEndInterpolant >= 1f)
            {
                BloodyTearsAnimationStartInterpolant = 0f;
                BloodyTearsAnimationEndInterpolant = 0f;
            }
        }

        // Calculate the animation completion.
        float animationCompletion = AITimer / (float)Teleport_TeleportDuration;

        // Enter the portal.
        if (PreviousState != AvatarAIType.Awaken_RiftSizeIncrease)
        {
            if (animationCompletion < 0.5f)
                NeckAppearInterpolant = MathF.Min(NeckAppearInterpolant, InverseLerp(0.4f, 0.1f, animationCompletion).Squared());
            else
                NeckAppearInterpolant = InverseLerp(0.5f, 0.8f, animationCompletion).Squared();
            LeftFrontArmScale = Sqrt(NeckAppearInterpolant);
            RightFrontArmScale = Sqrt(NeckAppearInterpolant);
            LeftFrontArmOpacity = InverseLerp(0f, 0.2f, NeckAppearInterpolant);
            RightFrontArmOpacity = LeftFrontArmOpacity;
            LilyScale = NeckAppearInterpolant.Squared();
            LegScale = Vector2.One * NeckAppearInterpolant;
            HeadOpacity = InverseLerp(0f, 0.4f, NeckAppearInterpolant);
        }

        // Make the Avatar's rotation stabilize, just in case it wasn't zero when the teleport was initiated.
        NPC.rotation = NPC.rotation.AngleLerp(0f, 0.02f).AngleTowards(0f, 0.04f);

        // Make the portal close and then open up again.
        NPC.scale = (1f - InverseLerpBump(0.3f, 0.5f, 0.5f, 0.7f, animationCompletion)).Squared();
        RiftVanishInterpolant = 1f - NPC.scale;

        // Perform the teleport.
        if (AITimer == (int)(Teleport_TeleportDuration * 0.5f))
        {
            // Go to the new teleport spot.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.Center = TeleportAction();
                HeadPosition = NPC.Center;
                LeftArmPosition = NPC.Center;
                RightArmPosition = NPC.Center;
                NPC.netUpdate = true;
                NPC.netSpam = 0;
            }

            // Play a teleport sound.
            if (!Main.LocalPlayer.dead)
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.RiftOpen);

            // Shake the screen.
            ScreenShakeSystem.StartShake(8.4f);

            // Shake the screen.
            ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 8f);
            GeneralScreenEffectSystem.ChromaticAberration.Start(NPC.Center, 1.2f, 60);

            // Create teleport particle effects.
            ExpandingGreyscaleCircleParticle circle = new ExpandingGreyscaleCircleParticle(NPC.Center, Vector2.Zero, new(255, 0, 39), 10, 0.28f);
            VerticalLightStreakParticle bigLightStreak = new VerticalLightStreakParticle(NPC.Center, Vector2.Zero, new(105, 215, 239), 10, new(2.4f, 3f));
            MagicBurstParticle magicBurst = new MagicBurstParticle(NPC.Center, Vector2.Zero, new(150, 109, 219), 15, 2f, 0.07f);
            for (int i = 0; i < 30; i++)
            {
                Vector2 smallLightStreakSpawnPosition = NPC.Center + Main.rand.NextVector2Square(-NPC.width, NPC.width) * new Vector2(0.15f, 0.2f);
                Vector2 smallLightStreakVelocity = Vector2.UnitY * Main.rand.NextFloat(-3f, 3f);
                VerticalLightStreakParticle smallLightStreak = new VerticalLightStreakParticle(smallLightStreakSpawnPosition, smallLightStreakVelocity, Color.White, 10, new(0.1f, 0.3f));
                smallLightStreak.Spawn();
            }
            bigLightStreak.Spawn();
            circle.Spawn();
            magicBurst.Spawn();

            // Clear fluids.
            LiquidDrawContents.LiquidPoints.Clear();
        }

        // Update limbs.
        PerformStandardLimbUpdates();
    }

    public void DoBehavior_TeleportAbovePlayer()
    {
        Vector2 teleportDestination = Target.Center - Vector2.UnitY * 350f;
        if (AITimer == 1 && !NPC.WithinRange(teleportDestination, 480f))
            StartTeleportAnimation(() => Target.Center - Vector2.UnitY * 350f);
    }

    public void StartTeleportAnimation(Func<Vector2> teleportAction)
    {
        if (TeleportDelayCountdown >= 1)
            return;

        TeleportAction = teleportAction;
        TeleportDelayCountdown = Teleport_TeleportDuration + 30;
        shouldStartTeleportAnimation = true;
    }
}
