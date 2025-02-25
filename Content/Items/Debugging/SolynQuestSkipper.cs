using NoxusBoss.Core.SolynEvents;
using NoxusBoss.Core.World.WorldGeneration;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Debugging;

public class SolynQuestSkipper : DebugItem
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

        foreach (SolynEvent solynEvent in ModContent.GetContent<SolynEvent>())
            solynEvent.SafeSetStage(solynEvent.TotalStages);

        PermafrostKeepWorldGen.PlayerGivenKey = true;
        p.GetValueRef<bool>(PermafrostKeepWorldGen.PlayerWasGivenKeyVariableName).Value = true;

        WorldSaveSystem.HasCompletedGenesis = true;
        return null;
    }
}
