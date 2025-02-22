using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// The chance that a steam geyser will release a sulfuric leaflet on a given frame.
    /// </summary>
    public static int SulfuricLeafletReleaseChance => 33000;

    private static void LoadSulfuricLeafletObtainmentObtainment_Wrapper()
    {
        if (CalamityCompatibility.Enabled)
            LoadSulfuricLeafletObtainmentObtainment();
    }

    [JITWhenModsEnabled(CalamityCompatibility.ModName)]
    private static void LoadSulfuricLeafletObtainmentObtainment()
    {
        GlobalTileEventHandlers.NearbyEffectsEvent += (x, y, type, closer) =>
        {
            if (Main.gamePaused)
                return;

            if (!Main.rand.NextBool(SulfuricLeafletReleaseChance))
                return;

            Tile t = Framing.GetTileSafely(x, y);
            Vector2 spawnPosition = new Vector2(x * 16f + 24f, y * 16f - 4f);
            bool releasesSteam = t.TileFrameX % 36 == 0 && t.TileFrameY % 36 == 0 && Collision.CanHitLine(spawnPosition, 1, 1, spawnPosition - Vector2.UnitY * 100f, 1, 1);
            if (releasesSteam && type == ModContent.TileType<SteamGeyser1>() || type == ModContent.TileType<SteamGeyser2>() || type == ModContent.TileType<SteamGeyser3>())
            {
                int leaflet = Item.NewItem(new EntitySource_TileUpdate(x, y), spawnPosition, SolynBookAutoloader.Books["SulfuricLeaflet"].Type);
                if (Main.item.IndexInRange(leaflet))
                    Main.item[leaflet].velocity = -Vector2.UnitY.RotatedByRandom(0.32f) * 12.5f;
            }
        };
    }
}
