using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static NoxusBoss.Core.Autoloaders.SolynBooks.SolynBookAutoloader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// A cooldown timer that ensures that "How to Fly" books cannot spawn in rapid succession.
    /// </summary>
    public static int HowToFlySpawnCooldown
    {
        get;
        set;
    }

    /// <summary>
    /// The chance of a "How to Fly" book appearing from the sky this frame, assuming other conditions are valid.
    /// </summary>
    public static int HowToFlySpawnChance => 194400;

    private static void TryToSpawnHowToFlyBook()
    {
        if (HowToFlySpawnCooldown >= 1)
        {
            HowToFlySpawnCooldown--;
            return;
        }

        foreach (Player player in Main.ActivePlayers)
        {
            if (player.dead)
                continue;

            TryToSpawnHowToFlyBookForPlayer(player);
        }
    }

    private static void TryToSpawnHowToFlyBookForPlayer(Player player)
    {
        // Only spawn the book if the player has wings.
        if (player.wings == 0)
            return;

        // Only spawn if the player is at or above the surface.
        if (player.position.Y > Main.worldSurface * 16f - 512f)
            return;

        // Only spawn if there's room above the player's head.
        if (!Collision.CanHitLine(player.Center, 1, 1, player.Center - Vector2.UnitY * 900f, 1, 1))
            return;

        if (Main.rand.NextBool(HowToFlySpawnChance))
        {
            Vector2 bookSpawnPosition = player.Center + new Vector2(Main.rand.NextFloat(400f, 850f) * Main.rand.NextFromList(-1f, 1f), -750f);
            Item.NewItem(new EntitySource_WorldEvent(), bookSpawnPosition, Books["HowToFly"].Type);
        }

        HowToFlySpawnCooldown = MinutesToFrames(120f);
    }
}
