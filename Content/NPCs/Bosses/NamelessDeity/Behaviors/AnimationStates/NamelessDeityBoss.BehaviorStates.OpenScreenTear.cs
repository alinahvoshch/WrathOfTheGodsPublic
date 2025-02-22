using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers.NamelessDeitySky;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    /// <summary>
    /// How long Nameless' hands spend attempting to grip the seam he creates during his Open Screen Tear state.
    /// </summary>
    public static int OpenScreenTear_SeamGripTime => GetAIInt("OpenScreenTear_SeamGripTime");

    /// <summary>
    /// How long Nameless' spends ripping open the seam he creates during his Open Screen Tear state.
    /// </summary>
    public static int OpenScreenTear_SeamRipOpenTime => GetAIInt("OpenScreenTear_SeamRipOpenTime");

    /// <summary>
    /// How long Nameless' special background waits before fading in during his Open Screen Tear state.
    /// </summary>
    public static int OpenScreenTear_BackgroundAppearDelay => GetAIInt("OpenScreenTear_BackgroundAppearDelay");

    /// <summary>
    /// How long Nameless' special background spends fading in during his Open Screen Tear state.
    /// </summary>
    public static int OpenScreenTear_BackgroundAppearTime => GetAIInt("OpenScreenTear_BackgroundAppearTime");

    /// <summary>
    /// How long Nameless waits before appearing during his Open Screen Tear state.
    /// </summary>
    public static int OpenScreenTear_NamelessDeityAppearDelay => GetAIInt("OpenScreenTear_NamelessDeityAppearDelay");

    /// <summary>
    /// How long Nameless' Open Screen Tear state goes on for overall.
    /// </summary>
    public static int OpenScreenTear_OverallDuration
    {
        get
        {
            int extraTime = 0;

            // Wait just a little bit longer if the Test of Resolve is ongoing, to sync the phase 3 music to Nameless' scream.
            if (TestOfResolveSystem.IsActive)
                extraTime = 14;

            return OpenScreenTear_SeamGripTime + OpenScreenTear_SeamRipOpenTime + OpenScreenTear_BackgroundAppearDelay +
                OpenScreenTear_BackgroundAppearTime + OpenScreenTear_NamelessDeityAppearDelay + extraTime;
        }
    }

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_OpenScreenTear()
    {
        StateMachine.RegisterTransition(NamelessAIType.OpenScreenTear, NamelessAIType.IntroScreamAnimation, false, () =>
        {
            return AITimer >= OpenScreenTear_OverallDuration;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.OpenScreenTear, DoBehavior_OpenScreenTear);
    }

    public void DoBehavior_OpenScreenTear()
    {
        int gripTime = OpenScreenTear_SeamGripTime;
        int ripOpenTime = OpenScreenTear_SeamRipOpenTime;
        int backgroundAppearDelay = OpenScreenTear_BackgroundAppearDelay;
        int backgroundAppearTime = OpenScreenTear_BackgroundAppearTime;
        int myselfAppearDelay = OpenScreenTear_NamelessDeityAppearDelay;

        // NO. You do NOT get adrenaline for sitting around and doing nothing.
        if (NPC.HasPlayerTarget)
            CalamityCompatibility.ResetRippers(Main.player[NPC.TranslatedTargetIndex]);

        // Use a specific hand texture.
        if (RenderComposite.Find<ArmsStep>().HandTexture is not null)
            RenderComposite.Find<ArmsStep>().HandTexture.ForceToTexture("Hand5");

        // Close the HP bar.
        CalamityCompatibility.MakeCalamityBossBarClose(NPC);

        // Disable music.
        Music = 0;

        // Keep the seam scale at its minimum at first.
        SeamScale = InverseLerp(0f, 20f, AITimer).Squared() * 2.3f;

        // Play a screen slice sound on the first frame.
        if (AITimer == 1f)
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.IntroScreenSlice with { Volume = 1.2f });

        // Update the drone loop sound from the Awaken state. It'll naturally terminate once Nameless starts attacking.
        IntroDroneLoopSound?.Update(Main.LocalPlayer.Center);

        // Stay above the target.
        NPC.Center = Target.Center - Vector2.UnitY * 4000f;

        // Stay invisible.
        NPC.Opacity = 0f;

        // Ensure that hands draw at full opacity irrespective of the fact that Nameless is currently invisible.
        HandsShouldInheritOpacity = false;

        // It's important that the hands be drawn manually. They are almost certainly going to be cut off by the limited render target size, given that Nameless is far above the target.
        DrawHandsSeparateFromRT = true;

        // Create many hands that will tear apart the screen on the first few frames.
        if (AITimer <= 16f && AITimer % 2f == 0f)
        {
            int handIndex = AITimer / 2;
            float verticalOffset = handIndex * 40f + 250f;
            if (handIndex % 2 == 0)
                verticalOffset *= -1f;

            Hands.Add(new(Target.Center - Vector2.UnitX.RotatedBy(-SeamAngle) * verticalOffset, false)
            {
                Velocity = Main.rand.NextVector2CircularEdge(17f, 17f),
                Opacity = 0f
            });
            return;
        }

        // Make chromatic aberration effects happen periodically.
        if (AITimer % 20f == 19f && HeavenlyBackgroundIntensity <= 0.1f)
        {
            float aberrationIntensity = Utils.Remap(AITimer, 0f, 120f, 0.4f, 1.6f);
            GeneralScreenEffectSystem.ChromaticAberration.Start(Target.Center, aberrationIntensity, 10);
        }

        // Have the hands move above and below the player, on the seam.
        float handMoveInterpolant = Pow(InverseLerp(0f, gripTime, AITimer), 3.2f) * 0.5f;

        Vector2 verticalOffsetDirection = Vector2.UnitX.RotatedBy(-SeamAngle - 0.015f);
        for (int i = 0; i < Hands.Count; i++)
        {
            bool left = i % 2 == 0;
            Vector2 handDestination = Target.Center + verticalOffsetDirection * -left.ToDirectionInt() * (i * 75f + 150f);
            handDestination += verticalOffsetDirection.RotatedBy(PiOver2 * -left.ToDirectionInt()) * 40f;
            if (handDestination.Y <= Target.Center.Y)
                handDestination.X -= 100f;

            handDestination.X -= left.ToDirectionInt() * 52f;

            Hands[i].ScaleFactor = 0.78f;
            Hands[i].Opacity = InverseLerp(0f, 12f, AITimer);
            Hands[i].FreeCenter = Vector2.Lerp(Hands[i].FreeCenter, handDestination, handMoveInterpolant) + Main.rand.NextVector2Circular(4f, 4f);
            if (Hands[i].FreeCenter.WithinRange(handDestination, 60f))
                Hands[i].RotationOffset = Hands[i].RotationOffset.AngleLerp(PiOver2 * left.ToDirectionInt(), 0.3f);
            else
                Hands[i].RotationOffset = (handDestination - Hands[i].FreeCenter).ToRotation();
        }

        // Rip open the seam.
        SeamScale += Pow(InverseLerp(0f, ripOpenTime, AITimer - gripTime - 30f), 1.5f) * 250f;
        if (SeamScale >= 2f && HeavenlyBackgroundIntensity <= 0.3f)
        {
            if (ScreenShakeSystem.OverallShakeIntensity <= 11f)
                ScreenShakeSystem.StartShake(8f);
        }

        if (AITimer == gripTime + 30f)
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.ScreenTear);

        // Delete the hands once the seam is fully opened.
        if (AITimer == gripTime + ripOpenTime + 60f)
        {
            DestroyAllHands(true);
            NPC.netUpdate = true;
        }

        // Make the natural background appear.
        HeavenlyBackgroundIntensity = InverseLerp(0f, backgroundAppearTime, AITimer - gripTime - ripOpenTime - backgroundAppearDelay);

        if (AITimer >= gripTime + ripOpenTime + backgroundAppearDelay + backgroundAppearTime)
        {
            SkyEyeScale *= 0.7f;
            if (SkyEyeScale <= 0.15f)
                SkyEyeScale = 0f;

            if (AITimer == OpenScreenTear_OverallDuration - 1)
            {
                // Mark Nameless as having been met for next time, so that the player doesn't have to wait as long.
                if (!WorldSaveSystem.HasMetNamelessDeity && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    WorldSaveSystem.HasMetNamelessDeity = true;
                    NetMessage.SendData(MessageID.WorldData);
                }
            }
        }

        // Update the seam shader.
        UpdateScreenTear();
    }

    public static void UpdateScreenTear()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        ManagedScreenFilter seamShader = ShaderManager.GetFilter("NoxusBoss.NamelessDeityScreenTearShader");
        seamShader.TrySetParameter("seamAngle", SeamAngle);
        seamShader.TrySetParameter("seamSlope", Tan(-SeamAngle));
        seamShader.TrySetParameter("seamBrightness", 0.029f);
        seamShader.TrySetParameter("warpIntensity", 0.04f);
        seamShader.TrySetParameter("offsetsAreAllowed", HeavenlyBackgroundIntensity <= 0.01f);
        seamShader.TrySetParameter("uOpacity", Clamp(1f - HeavenlyBackgroundIntensity + 0.001f, 0.001f, 1f));
        seamShader.TrySetParameter("uIntensity", SeamScale * InverseLerp(0.5f, 0.1f, HeavenlyBackgroundIntensity));
        seamShader.TrySetParameter("offsetsAreAllowed", HeavenlyBackgroundIntensity <= 0.01f);
        seamShader.SetTexture(GennedAssets.Textures.Extra.DivineLight, 1, SamplerState.LinearWrap);
        seamShader.Activate();
    }
}
