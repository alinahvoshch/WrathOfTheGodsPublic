using NoxusBoss.Core.Autoloaders.SolynBooks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// A cooldown timer that ensures that clover manuals cannot spawn in rapid succession.
    /// </summary>
    public static int CloverManualSpawnCooldown
    {
        get;
        set;
    }

    /// <summary>
    /// The chance of a Clover Manual appearing atop a player this frame, assuming other conditions are valid. This is affected by the luck stat, and is just the base probability.
    /// </summary>
    public static int CloverManualObtainmentBaseSpawnChance => 7777777;

    private static void TryToSpawnCloverManual()
    {
        if (CloverManualSpawnCooldown >= 1)
        {
            CloverManualSpawnCooldown--;
            return;
        }

        foreach (Player player in Main.ActivePlayers)
            TryToSpawnCloverManualForPlayer(player);
    }

    private static void TryToSpawnCloverManualForPlayer(Player player)
    {
        float luckInterpolant = InverseLerp(0f, 1.33f, player.luck);
        float luckBoost = 1f + Pow(luckInterpolant, 3.77f) * 27.777f;
        if (luckBoost < 1f)
            luckBoost = 1f;

        float obtainmentChance = luckBoost / CloverManualObtainmentBaseSpawnChance;
        if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(obtainmentChance))
        {
            Item.NewItem(new EntitySource_WorldEvent(), player.Center, SolynBookAutoloader.Books["CloverManual"].Type);

            CloverManualSpawnCooldown = MinutesToFrames(120f);
        }
    }
}
