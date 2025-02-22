using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Placeable;

public class GraphicalUniverseImager : ModItem
{
    public override string Texture => GetAssetPath("Content/Items/Placeable", Name);

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<GraphicalUniverseImagerTile>());
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
    }
}
