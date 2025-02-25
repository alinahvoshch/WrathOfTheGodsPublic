using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Draedon.DraedonDialogue;
using NoxusBoss.Core.SolynEvents;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Draedon;

public partial class QuestDraedon : ModNPC
{
    /// <summary>
    /// How long it takes for Mars to appear after the player has indicated that they are ready.
    /// </summary>
    public static int MarsAppearDelay => SecondsToFrames(2.5f);

    /// <summary>
    /// The AI method that makes Draedon wait before summoning Mars.
    /// </summary>
    public void DoBehavior_WaitForMarsToArrive()
    {
        NPC.boss = true;

        if (AITimer == 1)
        {
            NPC.TargetClosest();

            bool hasSpokenToDraedonBefore = MarsCombatEvent.HasSpokenToDraedonBefore;
            string localizationSuffix = hasSpokenToDraedonBefore ? "Successive" : string.Empty;
            DraedonWorldDialogueManager.CreateNew($"Mods.NoxusBoss.Dialog.DraedonAcceptanceResponse{localizationSuffix}", NPC.spriteDirection, NPC.Top - Vector2.UnitY * 32f, 150);

            // Mark Draedon as having been talked to, so that successive dialogue doesn't go on for a whole minute again.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                MarsCombatEvent.HasSpokenToDraedonBefore = true;
                NetMessage.SendData(MessageID.WorldData);
            }
        }

        if (Main.netMode != NetmodeID.MultiplayerClient && AITimer == MarsAppearDelay)
        {
            NPC.NewNPC(NPC.GetSource_FromAI(), (int)PlayerToFollow.Center.X, (int)PlayerToFollow.Center.Y - 900, ModContent.NPCType<MarsBody>(), 1, Target: NPC.target);
            ChangeAIState(DraedonAIType.ObserveBattle);
        }

        if (FrameTimer >= 7f)
        {
            Frame++;
            FrameTimer = 0f;
            if (Frame >= 47)
                Frame = 23;
        }
    }
}
