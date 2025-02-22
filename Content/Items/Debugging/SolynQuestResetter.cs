using NoxusBoss.Core.World.GameScenes.SolynEventHandlers;
using NoxusBoss.Core.World.WorldGeneration;
using Terraria;
using Terraria.ID;

namespace NoxusBoss.Content.Items.Debugging;

public class SolynQuestResetter : DebugItem
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

        StargazingQuestSystem.Completed = false;

        PermafrostKeepQuestSystem.KeepVisibleOnMap = false;
        PermafrostKeepQuestSystem.Ongoing = false;
        PermafrostKeepQuestSystem.Completed = false;

        CeaselessVoidQuestSystem.Ongoing = false;
        CeaselessVoidQuestSystem.Completed = false;

        DraedonCombatQuestSystem.Ongoing = false;
        DraedonCombatQuestSystem.Completed = false;
        DraedonCombatQuestSystem.HasSpokenToDraedonBefore = false;

        PermafrostKeepWorldGen.PlayerGivenKey = false;
        p.GetValueRef<bool>(PermafrostKeepWorldGen.PlayerWasGivenKeyVariableName).Value = false;
        return null;
    }
}
