using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static NoxusBoss.Core.Autoloaders.SolynBooks.SolynBookAutoloader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// The chance of a seamap collection book appearing in water this frame, assuming other conditions are valid.
    /// </summary>
    public static int SeamapCollectionSpawnChance => 54000;

    private static void TryToSpawnSeamapCollection()
    {
        foreach (Player player in Main.ActivePlayers)
        {
            if (player.dead)
                continue;

            TryToSpawnSeamapCollectionForPlayer(player);
        }
    }

    private static void TryToSpawnSeamapCollectionForPlayer(Player player)
    {
        // Only spawn if the player is at or above the surface.
        if (player.position.Y > Main.worldSurface * 16f - 512f)
            return;

        // Only spawn if the player is at the ocean.
        bool atOcean = player.Center.X <= 7200f || player.Center.X >= Main.maxTilesX * 16f - 7200f;
        if (!atOcean)
            return;

        if (Main.rand.NextBool(SeamapCollectionSpawnChance))
        {
            Vector2 bookSpawnPosition = player.Center + Main.rand.NextVector2Unit() * new Vector2(Main.rand.NextFloat(300f, 800f), Main.rand.NextFloat(800f));

            // Only spawn the book if the chosen spawn position is decently submerged.
            if (!Collision.WetCollision(bookSpawnPosition, 16, 16) || !Collision.WetCollision(bookSpawnPosition - Vector2.UnitY * 400f, 16, 16))
                return;

            Item.NewItem(new EntitySource_WorldEvent(), bookSpawnPosition, Books["SeamapCollection"].Type);
        }
    }
}
