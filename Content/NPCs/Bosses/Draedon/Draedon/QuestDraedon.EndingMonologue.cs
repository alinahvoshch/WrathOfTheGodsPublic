using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Draedon.DraedonDialogue;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.SolynEvents;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Draedon;

public partial class QuestDraedon : ModNPC
{
    public static readonly DialogueChain EndingDialogue =
        new DialogueChain(SecondsToFrames(0.33f)).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.DraedonEndingMonologue.Draedon1", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.DraedonEndingMonologue.Draedon2", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.DraedonEndingMonologue.Solyn1", true).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.DraedonEndingMonologue.Solyn2", true).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.DraedonEndingMonologue.Draedon3", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.DraedonEndingMonologue.Draedon4", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.DraedonEndingMonologue.Draedon5", false).
        AddWithAutomaticTiming("Mods.NoxusBoss.Dialog.DraedonEndingMonologue.Draedon6", false);

    /// <summary>
    /// The AI method that makes Draedon do his ending monologue before leaving.
    /// </summary>
    public void DoBehavior_EndingMonologue()
    {
        int solynIndex = NPC.FindFirstNPC(ModContent.NPCType<Solyn>());
        if (solynIndex == -1)
        {
            NPC.active = false;
            return;
        }

        NPC solyn = Main.npc[solynIndex];

        Vector2 hoverDestination = PlayerToFollow.Center - Vector2.UnitX * 120f;
        NPC.SmoothFlyNear(hoverDestination, 0.06f, 0.9f);
        NPC.spriteDirection = (int)NPC.HorizontalDirectionTo(PlayerToFollow.Center);

        if (FrameTimer % 7f == 6f)
        {
            Frame++;
            if (Frame >= 48)
                Frame = 23;
        }

        int speakTimer = AITimer - 60;
        EndingDialogue.Update(speakTimer, solyn, NPC);

        if (CalamityCompatibility.Enabled)
        {
            Music = MusicLoader.GetMusicSlot("CalamityMod/Sounds/Music/DraedonExoSelect");
            NPC.boss = true;
        }

        if (speakTimer >= EndingDialogue.Duration)
        {
            ChangeAIState(DraedonAIType.Leave);
            ModContent.GetInstance<MarsCombatEvent>().SafeSetStage(2);
            SolynEvent.Solyn?.SwitchState(SolynAIType.WaitToTeleportHome);
        }
    }
}
