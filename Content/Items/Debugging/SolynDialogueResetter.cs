using NoxusBoss.Core.DialogueSystem;
using NoxusBoss.Core.World.GameScenes.SolynEventHandlers;
using Terraria;
using Terraria.ID;

namespace NoxusBoss.Content.Items.Debugging;

public class SolynDialogueResetter : DebugItem
{
    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 36;
        Item.useAnimation = 40;
        Item.useTime = 40;
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = null;
        Item.rare = ItemRarityID.Blue;
        Item.value = 0;
    }

    public override bool? UseItem(Player p)
    {
        if (Main.myPlayer == NetmodeID.MultiplayerClient || p.itemAnimation != p.itemAnimationMax - 1)
            return false;

        DialogueSaveSystem.seenDialogue.Clear();
        RandomSolynSpawnSystem.SolynHasAppearedBefore = false;
        return null;
    }
}
