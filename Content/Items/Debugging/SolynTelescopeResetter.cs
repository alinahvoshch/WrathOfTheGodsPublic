using NoxusBoss.Content.Tiles.TileEntities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace NoxusBoss.Content.Items.Debugging;

public class SolynTelescopeResetter : DebugItem
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

        foreach (TileEntity te in TileEntity.ByID.Values)
        {
            if (te is TESolynTelescope telescope)
                telescope.IsRepaired = false;
        }
        return null;
    }
}
