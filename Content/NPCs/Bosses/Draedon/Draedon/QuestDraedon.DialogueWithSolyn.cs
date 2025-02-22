using NoxusBoss.Content.NPCs.Bosses.Draedon.DraedonDialogue;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.World.GameScenes.SolynEventHandlers;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Draedon;

public partial class QuestDraedon : ModNPC
{
    /// <summary>
    /// Whether Draedon is waiting for the player to respond to his deal and start the battle.
    /// </summary>
    public bool WaitingOnPlayerResponse
    {
        get;
        private set;
    }

    public static readonly DialogueChain InitialInteractionDialogue_FirstTime =
        new DialogueChain(SecondsToFrames(0.33f)).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Draedon1", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Draedon2", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Solyn1", true).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Draedon3", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Draedon4", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Solyn2", true).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Draedon5", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Draedon6", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Draedon7", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Draedon8", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Draedon9", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Draedon10", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Draedon11", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Solyn3", true).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteraction.Solyn4", true);

    public static readonly DialogueChain InitialInteractionDialogue_Successive =
        new DialogueChain(SecondsToFrames(0.33f)).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.SolynDraedonInteractionSuccessive", false);

    public static DialogueChain InitialInteractionDialogue => DraedonCombatQuestSystem.HasSpokenToDraedonBefore ? InitialInteractionDialogue_Successive : InitialInteractionDialogue_FirstTime;

    /// <summary>
    /// The AI method that makes Draedon and Solyn talk before the battle begins.
    /// </summary>
    public void DoBehavior_DialogueWithSolyn()
    {
        int solynIndex = NPC.FindFirstNPC(ModContent.NPCType<Solyn>());
        NPC? solyn = solynIndex >= 0 ? Main.npc[solynIndex] : null;
        CalamityCompatibility.ResetRippers(PlayerToFollow);

        // Look at the player.
        NPC.spriteDirection = NPC.HorizontalDirectionTo(PlayerToFollow.Center).NonZeroSign();

        int speakTimer = AITimer - 60;
        InitialInteractionDialogue.Update(speakTimer, solyn, NPC);

        // Let the player respond and begin the battle once Draedon and Solyn are done speaking.
        // This, along with some code in the corresponding UI, are what determine if Draedon will transition to the next state.
        WaitingOnPlayerResponse = speakTimer >= InitialInteractionDialogue.Duration - 45;

        bool sitDown = speakTimer >= 210 && !DraedonCombatQuestSystem.HasSpokenToDraedonBefore;

        if (Frame <= 10f)
        {
            if (FrameTimer >= 7f)
            {
                Frame++;
                FrameTimer = 0f;
            }
        }
        else
        {
            if (sitDown)
            {
                if (FrameTimer >= 7f)
                {
                    Frame++;
                    FrameTimer = 0f;
                    if (Frame >= 47)
                        Frame = 23;
                }
            }
            else
                Frame = (int)Lerp(11f, 15f, FrameTimer / 30f % 1f);
        }

        if (CalamityCompatibility.Enabled)
        {
            Music = MusicLoader.GetMusicSlot("CalamityMod/Sounds/Music/DraedonExoSelect");
            NPC.boss = true;
        }
    }
}
