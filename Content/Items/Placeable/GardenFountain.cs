using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Placeable;

public class GardenFountain : ModItem
{
    public override string Texture => GetAssetPath("Content/Items/Placeable", Name);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<GardenFountainTile>());
        Item.width = 16;
        Item.height = 10;
        Item.rare = ModContent.RarityType<SolynRewardRarity>();
        Item.value = 0;
    }
}
