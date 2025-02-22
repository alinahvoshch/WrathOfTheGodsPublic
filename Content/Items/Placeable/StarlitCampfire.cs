using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles.SolynCampsite;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Placeable;

public class StarlitCampfire : ModItem
{
    public override string Texture => GetAssetPath("Content/Items/Placeable", Name);

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<StarlitCampfireTile>());
        Item.rare = ModContent.RarityType<SolynRewardRarity>();
    }
}
