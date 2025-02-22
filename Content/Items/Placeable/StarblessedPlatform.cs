using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Placeable;

public class StarblessedPlatform : ModItem
{
    /// <summary>
    /// The maximum distance, in tile coordinates, source tiles can be away from each other before they cease to be able to connect.
    /// </summary>
    public const int MaximumReach = 222;

    public override string Texture => GetAssetPath("Content/Items/Placeable", Name);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 20;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(TileID.Platforms);
        Item.width = 24;
        Item.height = 16;
        Item.rare = ModContent.RarityType<SolynRewardRarity>();
        Item.createTile = ModContent.TileType<StarblessedPlatformSource>();
        Item.consumable = true;
    }
}

