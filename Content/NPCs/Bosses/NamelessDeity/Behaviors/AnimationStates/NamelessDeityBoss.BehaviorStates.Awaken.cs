using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.SoundSystems;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using static NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers.NamelessDeitySky;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    public LoopedSoundInstance IntroDroneLoopSound;

    /// <summary>
    /// How long it takes for stars to begin receding during Nameless' Awaken state.
    /// </summary>
    public static int Awaken_StarRecedeDelay => GetAIInt("Awaken_StarRecedeDelay");

    /// <summary>
    /// How long it takes for stars to recede during Nameless' Awaken state.
    /// </summary>
    public static int Awaken_StarRecedeTime => GetAIInt("Awaken_StarRecedeTime");

    /// <summary>
    /// How long it takes for Nameless' eye to materialize in the background during his Awaken state.
    /// </summary>
    public static int Awaken_EyeAppearTime => GetAIInt("Awaken_EyeAppearTime");

    /// <summary>
    /// How long Nameless' eye spends observing in the background during his Awaken state.
    /// </summary>
    public static int Awaken_EyeObserveTime => GetAIInt("Awaken_EyeObserveTime");

    /// <summary>
    /// How long Nameless waits before contracting his pupil during his Awaken state.
    /// </summary>
    public static int Awaken_PupilContractDelay => GetAIInt("Awaken_PupilContractDelay");

    /// <summary>
    /// How long Nameless' Awaken state goes on for overall.
    /// </summary>
    public static int Awaken_OverallDuration => Awaken_StarRecedeDelay + Awaken_StarRecedeTime + Awaken_EyeAppearTime + Awaken_EyeObserveTime;

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_Awaken()
    {
        // Load the transition from Awaken to OpenScreenTear (or IntroScreamAnimation, if Nameless has already been defeated).
        StateMachine.RegisterTransition(NamelessAIType.Awaken, NamelessAIType.OpenScreenTear, false, () =>
        {
            return AITimer >= Awaken_OverallDuration;
        }, () =>
        {
            SkyEyeOpacity = 0f;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.Awaken, DoBehavior_Awaken);
    }

    public void DoBehavior_Awaken()
    {
        int starRecedeDelay = Awaken_StarRecedeDelay;
        int starRecedeTime = Awaken_StarRecedeTime;
        int eyeAppearTime = Awaken_EyeAppearTime;
        int eyeObserveTime = Awaken_EyeObserveTime;
        int pupilContractDelay = Awaken_PupilContractDelay;

        // NO. You do NOT get adrenaline for sitting around and doing nothing.
        if (NPC.HasPlayerTarget)
            CalamityCompatibility.ResetRippers(Main.player[NPC.TranslatedTargetIndex]);

        // Create suspense shortly before the animation concludes.
        if (AITimer == Awaken_OverallDuration - 150f)
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.IntroSuspenseBuild with { Volume = 1.5f });

        // Close the HP bar.
        CalamityCompatibility.MakeCalamityBossBarClose(NPC);

        // Disable music.
        Music = 0;

        // Make some screen shake effects happen.
        if (AITimer < starRecedeDelay)
        {
            float screenShakeIntensityInterpolant = Pow(AITimer / starRecedeDelay, 1.84f);
            ScreenShakeSystem.SetUniversalRumble(Lerp(2f, 11.5f, screenShakeIntensityInterpolant), TwoPi / 9f, Vector2.UnitY, 0.2f);
            return;
        }

        // Make the stars recede away in fear.
        StarRecedeInterpolant = InverseLerp(starRecedeDelay, starRecedeDelay + starRecedeTime, AITimer);
        if (AITimer == starRecedeDelay + 10f)
        {
            // Play the star recede sound.
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.StarRecede with { Volume = 1.2f });

            // Start the intro drone sound.
            IntroDroneLoopSound?.Stop();
            IntroDroneLoopSound = LoopedSoundManager.CreateNew(GennedAssets.Sounds.NamelessDeity.IntroDrone with { Volume = 1.5f }, () =>
            {
                return !NPC.active || IsAttackState(CurrentState) || HeavenlyBackgroundIntensity >= 0.15f;
            });
        }

        // Update the drone loop sound.
        IntroDroneLoopSound?.Update(Main.LocalPlayer.Center);

        // Perform some camera effects.
        CalamityCompatibility.ResetStealthBarOpacity(Main.LocalPlayer);
        float zoomOutInterpolant = InverseLerp(starRecedeDelay + starRecedeTime + eyeAppearTime + eyeObserveTime + pupilContractDelay - 17f, starRecedeDelay + starRecedeTime + eyeAppearTime + eyeObserveTime + pupilContractDelay - 4f, AITimer);
        CameraPanSystem.PanTowards(new Vector2(Main.LocalPlayer.Center.X, 3000f), Pow(StarRecedeInterpolant * (1f - zoomOutInterpolant), 0.17f));
        CameraPanSystem.ZoomIn(Pow(StarRecedeInterpolant * (1f - zoomOutInterpolant), 0.4f) * 0.6f);

        // Inputs are disabled and UIs are hidden during the camera effects. This is safely undone once Nameless begins screaming, or if he goes away for some reason.
        if (AITimer == starRecedeDelay + 1f)
        {
            BlockerSystem.Start(true, true, () =>
            {
                // The block should immediately terminate if Nameless leaves for some reason.
                if (!NPC.active)
                    return false;

                // The block naturally terminates once Nameless isn't doing any introductory attack scenes.
                if (CurrentState is not NamelessAIType.Awaken and not NamelessAIType.OpenScreenTear)
                    return false;

                return true;
            });
        }

        // All code beyond this point only executes once all the stars have left.
        if (StarRecedeInterpolant < 1f)
            return;

        // Make the eye appear.
        SkyEyeOpacity = InverseLerp(starRecedeDelay + starRecedeTime, starRecedeDelay + starRecedeTime + eyeAppearTime, AITimer);
        if (AITimer == starRecedeDelay + starRecedeTime + 1f)
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.Appear with { Volume = 1.3f });

        float pupilRollInterpolant = Pow(InverseLerp(1f, eyeAppearTime + 8f, AITimer - starRecedeDelay - starRecedeTime - 15f), 0.34f);
        float pupilScaleInterpolant = InverseLerp(0f, 10f, AITimer - starRecedeDelay - starRecedeTime - eyeAppearTime + 20f);
        float pupilContractInterpolant = Pow(InverseLerp(18f, pupilContractDelay, AITimer - starRecedeDelay - starRecedeTime - eyeAppearTime - eyeObserveTime), 0.25f);

        // Play a disgusting eye roll sound shortly after appearing.
        if (AITimer == starRecedeDelay + starRecedeTime + 20f)
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.EyeRoll with { Volume = 1.2f });

        // Make the eye look at the player after rolling eyes.
        Vector2 eyeRollDirection = Vector2.UnitY.RotatedBy(TwoPi * pupilRollInterpolant);
        SkyPupilOffset = Vector2.Lerp(SkyPupilOffset, Vector2.UnitY * (pupilContractInterpolant * 36f + 8f) + eyeRollDirection * 20f, 0.3f);
        SkyPupilScale = Pow(pupilScaleInterpolant, 1.7f) - pupilContractInterpolant * 0.5f;

        // Make the eye disappear before the seam appears.
        if (AITimer >= starRecedeDelay + starRecedeTime + eyeAppearTime + eyeObserveTime + pupilContractDelay - 48f)
        {
            if (AITimer == starRecedeDelay + starRecedeTime + eyeAppearTime + eyeObserveTime + pupilContractDelay - 35f)
            {
                Color twinkleColor = Color.Lerp(Color.HotPink, Color.Cyan, Main.rand.NextFloat(0.36f, 0.64f));
                TwinkleParticle twinkle = new TwinkleParticle(Main.screenPosition + new Vector2(Main.screenWidth * 0.5f, 470f), Vector2.Zero, twinkleColor, 30, 6, Vector2.One * 2f);
                twinkle.Spawn();
            }

            SkyEyeScale *= 0.7f;
            if (SkyEyeScale <= 0.15f)
                SkyEyeScale = 0f;
        }
    }
}
