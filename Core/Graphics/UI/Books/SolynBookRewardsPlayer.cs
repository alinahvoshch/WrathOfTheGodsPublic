using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Netcode;
using NoxusBoss.Core.Netcode.Packets;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Core.Graphics.UI.Books;

public class SolynBookRewardsPlayer : ModPlayer
{
    /// <summary>
    /// The set of all unclaimed rewards earned from redeeming books.
    /// </summary>
    public List<Item> UnclaimedRewards
    {
        get;
        private set;
    } = new List<Item>();

    private static List<SolynReward> CheckForProgressionRewards(float oldProgressionRatio)
    {
        float newProgressionRatio = SolynBookExchangeRegistry.RedeemedBooks.Count / (float)SolynBookExchangeRegistry.ObtainableBooks.Count;
        List<SolynReward> rewards = [];

        foreach (SolynProgressionRatioReward progressionReward in SolynBookRewardsSystem.ProgressionRewardMappings)
        {
            float progressionRatio = progressionReward.BookProgressionPercentage * 0.01f;
            if (oldProgressionRatio < progressionRatio && newProgressionRatio >= progressionRatio)
                rewards.Add(progressionReward.Reward);
        }

        return rewards;
    }

    /// <summary>
    /// Generates rewards for a given redeemed book in accordance with its reward mappings.
    /// </summary>
    /// <param name="oldProgressionRatio">The ratio of redeemed books to obtainable books prior to the redeeming of this book.</param>
    /// <param name="redeemedBook">The book that was redeemed for rewards.</param>
    public void GenerateRewards(float oldProgressionRatio, AutoloadableSolynBook redeemedBook)
    {
        List<SolynReward> rewards = CheckForProgressionRewards(oldProgressionRatio);
        if (SolynBookRewardsSystem.RewardMappings.TryGetValue(redeemedBook, out List<SolynReward>? rewardsForBook))
            rewards.AddRange(rewardsForBook);

        foreach (SolynReward reward in rewards)
        {
            int stack = reward.MaxStack;
            if (reward.MaxStack > reward.MinStack)
                stack = Main.rand.Next(reward.MinStack, reward.MaxStack + 1);

            Item? existingItemOfId = UnclaimedRewards.FirstOrDefault(i => i.type == reward.ItemID);
            if (existingItemOfId is not null && existingItemOfId.stack + stack < existingItemOfId.maxStack)
                existingItemOfId.stack += stack;
            else
            {
                Item item = new Item(reward.ItemID, stack);
                item.Wrath().ToBeGiftedBySolyn = reward.GiftedDirectlyFromSolyn;
                UnclaimedRewards.Add(item);
            }
        }

        PacketManager.SendPacket<SolynBookRewardPacket>(Player.whoAmI);
    }

    public override void SaveData(TagCompound tag) => tag["UnredeemedRewards"] = UnclaimedRewards.Select(ItemIO.Save).ToList();

    public override void LoadData(TagCompound tag) => UnclaimedRewards = tag.GetList<TagCompound>("UnredeemedRewards").Select(ItemIO.Load).ToList();
}
