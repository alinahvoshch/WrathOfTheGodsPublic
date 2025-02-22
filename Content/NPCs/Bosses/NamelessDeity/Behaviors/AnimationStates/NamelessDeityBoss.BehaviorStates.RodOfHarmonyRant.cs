using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    public static readonly int RodOfHarmonyRantDelay = SecondsToFrames(3.167f);

    public static readonly int RodOfHarmonyRantDuration = SecondsToFrames(52.892f);

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_RodOfHarmonyRant()
    {
        // Load the transition from the rant to the next in the cycle.
        StateMachine.RegisterTransition(NamelessAIType.RodOfHarmonyRant, NamelessAIType.ResetCycle, false, () =>
        {
            return AITimer >= RodOfHarmonyRantDelay + RodOfHarmonyRantDuration + 60;
        }, () => Music = MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/NamelessDeity"));

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.RodOfHarmonyRant, DoBehavior_RodOfHarmonyRant);
    }

    public void DoBehavior_RodOfHarmonyRant()
    {
        Vector2 hoverDestination = Target.Center + new Vector2((Target.Center.X < NPC.Center.X).ToDirectionInt() * 362f, Sin01(TwoPi * AITimer / 180f) * 90f - 262f);
        ref float hasInitialized = ref NPC.ai[2];
        ref float brokeRodOfHarmony = ref NPC.ai[3];

        // Make fists disappear.
        RipperUIDestructionSystem.FistOpacity = Saturate(RipperUIDestructionSystem.FistOpacity - 0.2f);

        // Use the ultimate """music""".
        if (AITimer >= RodOfHarmonyRantDelay)
        {
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/RodOfHarmonyRant");
            Main.musicFade[Music] = 1f;
        }
        else
            Music = 0;

        // Disable damage.
        NPC.dontTakeDamage = true;

        // Disallow turning off the music, and punish the player by restarting.
        if (Main.musicVolume < 0.3f)
        {
            AITimer = Math.Min(AITimer, RodOfHarmonyRantDelay);
            NPC.netUpdate = true;
            Main.musicVolume = 0.3f;
        }

        // Perform initializations immediately.
        if (hasInitialized == 0f)
        {
            ImmediateTeleportTo(hoverDestination, false);
            ZPosition = 0f;
            NPC.Opacity = 1f;
            NPC.netUpdate = true;

            hasInitialized = 1f;
        }

        // Start the rant.
        if (RoHDestructionSystem.AnimationTimer <= 0 && brokeRodOfHarmony == 0f)
            RoHDestructionSystem.Start();
        if (RoHDestructionSystem.AnimationTimer >= RoHDestructionSystem.SlashDelay)
            brokeRodOfHarmony = 1f;

        // Make the boss bar close.
        CalamityCompatibility.MakeCalamityBossBarClose(NPC);

        // Shake the screen at the start as Nameless yells.
        if (AITimer == RodOfHarmonyRantDelay + 3f || AITimer == RodOfHarmonyRantDelay + 37f)
            ScreenShakeSystem.StartShake(12f, shakeStrengthDissipationIncrement: 0.36f);

        // Flap wings.
        UpdateWings(AITimer / (Main.zenithWorld ? 150f : 60f));

        // Fly near the target.
        NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, 0.04f, 0.956f, 40f);

        // Make the kaleidoscope effect fade out near the end.
        if (AITimer >= RodOfHarmonyRantDelay + RodOfHarmonyRantDuration)
            NamelessDeitySky.KaleidoscopeInterpolant *= 0.81f;
        else
            NamelessDeitySky.KaleidoscopeInterpolant = 1f;

        // Update hands.
        DefaultUniversalHandMotion();
    }
}
