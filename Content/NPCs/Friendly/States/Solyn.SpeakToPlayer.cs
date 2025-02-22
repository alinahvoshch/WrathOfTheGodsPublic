using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using NoxusBoss.Core.Graphics.UI.SolynDialogue;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    [AutomatedMethodInvoke]
    public void LoadStateTransitions_SpeakToPlayer()
    {
        StateMachine.RegisterTransition(SolynAIType.SpeakToPlayer, null, false, () =>
        {
            return !((Main.LocalPlayer.talkNPC == NPC.whoAmI && CanBeSpokenTo) || ForcedConversation);
        });

        StateMachine.RegisterStateBehavior(SolynAIType.SpeakToPlayer, DoBehavior_SpeakToPlayer);
    }

    /// <summary>
    /// Performs Solyn's speak-to-player state.
    /// </summary>
    public void DoBehavior_SpeakToPlayer()
    {
        ReelInKite = false;
        NPC.velocity.X *= 0.7f;
        NPC.rotation = 0f;
        PerformStandardFraming();

        if (SolynDialogRegistry.SolynIntroduction.NodeSeen("Question1") || CurrentConversation != SolynDialogRegistry.SolynIntroduction)
            NPC.spriteDirection = (Main.LocalPlayer.Center.X - NPC.Center.X).NonZeroSign();

        SolynDialogUI ui = ModContent.GetInstance<SolynDialogSystem>().DialogUI;
        if (AITimer % 18 >= 9 && ui.DialogueText != ui.ResponseToSay && ui.NextCharacterDelay <= 10)
            Frame = 19f;
    }
}
