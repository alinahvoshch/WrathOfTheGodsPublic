using CalamityMod.BiomeManagers;
using CalamityMod.Events;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.CrossCompatibility.Inbound;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// The chance of an Acidic Prayerbook appearing from the sky this frame, assuming other conditions are valid.
    /// </summary>
    public static int AcidicPrayerbookSpawnChance => 74000;

    private static void TryToSpawnAcidicPrayerbook_Wrapper()
    {
        if (CalamityCompatibility.Enabled)
            TryToSpawnAcidicPrayerbook();
    }

    [JITWhenModsEnabled(CalamityCompatibility.ModName)]
    private static void TryToSpawnAcidicPrayerbook()
    {
        foreach (Player player in Main.ActivePlayers)
            TryToSpawnAcidicPrayerbookForPlayer(player);
    }

    [JITWhenModsEnabled(CalamityCompatibility.ModName)]
    private static void TryToSpawnAcidicPrayerbookForPlayer(Player player)
    {
        // Only spawn if the player is at or above the surface.
        if (player.position.Y > Main.worldSurface * 16f - 512f)
            return;

        // Only spawn if there's room above the player's head.
        if (!Collision.CanHitLine(player.Center, 1, 1, player.Center - Vector2.UnitY * 900f, 1, 1))
            return;

        // Only spawn if tier three acid rain is ongoing.
        if (!AcidRainEvent.AcidRainEventIsOngoing || !CommonCalamityVariables.PolterghastDefeated)
            return;

        // Only spawn if the player is at the sulphurous sea.
        if (!player.InModBiome<SulphurousSeaBiome>())
            return;

        if (Main.rand.NextBool(AcidicPrayerbookSpawnChance))
        {
            Vector2 bookSpawnPosition = player.Center + new Vector2(Main.rand.NextFloat(400f, 850f) * Main.rand.NextFromList(-1f, 1f), -750f);
            Item.NewItem(new EntitySource_WorldEvent(), bookSpawnPosition, SolynBookAutoloader.Books["AcidicPrayerbook"].Type);
        }
    }
}
