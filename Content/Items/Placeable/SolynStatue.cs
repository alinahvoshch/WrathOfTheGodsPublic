using NoxusBoss.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Placeable;

public class SolynStatue : ModItem
{
    public override string Texture => GetAssetPath("Content/Items/Placeable", Name);

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.ArmorStatue);
        Item.createTile = ModContent.TileType<SolynStatueTile>();
        Item.placeStyle = 0;
    }
}

