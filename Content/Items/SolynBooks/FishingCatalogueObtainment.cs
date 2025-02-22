using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// The chance for the Fishing Catalogue book to be given as an Angler reward.
    /// </summary>
    public static int FishingCatalogueObtainmentRewardChance => 9;

    /// <summary>
    /// The cyclic rate at which the player is given a fishing catalogue as a guaranteed drop.
    /// </summary>
    public static int FishingCatalogueObtainmentRewardCycle => 14;

    private static void LoadFishingCatalogueObtainment()
    {
        PlayerDataManager.AnglerQuestRewardEvent += (player, rewardsMultiplier, rewards) =>
        {
            if (Main.rand.NextBool(FishingCatalogueObtainmentRewardChance) || player.Player.anglerQuestsFinished % FishingCatalogueObtainmentRewardCycle == FishingCatalogueObtainmentRewardCycle - 1)
                rewards.Add(new Item(SolynBookAutoloader.Books["FishingCatalogue"].Type));
        };
    }
}
