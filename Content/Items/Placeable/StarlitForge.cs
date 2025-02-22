using NoxusBoss.Content.Tiles;
using NoxusBoss.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Placeable;

public class StarlitForge : ModItem
{
    public override string Texture => GetAssetPath("Content/Items/Placeable", Name);

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.Furnace);
        Item.width = 48;
        Item.height = 34;
        Item.createTile = ModContent.TileType<StarlitForgeTile>();
        Item.UseVioletRarity();
    }
}
