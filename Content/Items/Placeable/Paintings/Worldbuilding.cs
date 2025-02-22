using NoxusBoss.Content.Tiles.Paintings;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Placeable.Paintings;

public class Worldbuilding : ModItem, ILocalizedModType
{
    public override string Texture => GetAssetPath("Content/Items/Placeable/Paintings", Name);

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 32;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.value = Item.buyPrice(0, 15, 0, 0);
        Item.createTile = ModContent.TileType<WorldbuildingTile>();
    }
}
