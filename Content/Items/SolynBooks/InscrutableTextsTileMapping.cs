using NoxusBoss.Content.Tiles;
using NoxusBoss.Core.Autoloaders.SolynBooks;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    private static void MapInscrutableTextsTile()
    {
        SolynBookAutoloader.Books["InscrutableTexts"].PlacementTileID = ModContent.TileType<InscrutableTextsPlaced>();
    }
}
