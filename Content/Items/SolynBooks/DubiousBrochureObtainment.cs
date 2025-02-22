using NoxusBoss.Core.Autoloaders.SolynBooks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    private static void LoadDubiousBrochureObtainment()
    {
        On_WorldGen.CheckOrb += (orig, i, j, type) =>
        {
            orig(i, j, type);

            Tile t = Framing.GetTileSafely(i, j);
            if (WorldGen.shadowOrbSmashed && WorldGen.shadowOrbCount == 0 && t.TileFrameX % 36 == 0 && t.TileFrameY % 36 == 0 && t.TileType == TileID.ShadowOrbs)
            {
                int brochureID = WorldGen.crimson ? SolynBookAutoloader.Books["DubiousBrochureCrimson"].Type : SolynBookAutoloader.Books["DubiousBrochureCorruption"].Type;
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, brochureID);
            }
        };
    }
}
