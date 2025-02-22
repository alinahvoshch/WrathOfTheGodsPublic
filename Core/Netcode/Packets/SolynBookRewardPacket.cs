using NoxusBoss.Core.Graphics.UI.Books;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Core.Netcode.Packets;

public class SolynBookRewardPacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        Player player = Main.player[(int)context[0]];
        packet.Write(player.whoAmI);

        List<Item> rewards = player.GetModPlayer<SolynBookRewardsPlayer>().UnclaimedRewards;
        packet.Write(rewards.Count);

        for (int i = 0; i < rewards.Count; i++)
            ItemIO.Send(rewards[i], packet, true);
    }

    public override void Read(BinaryReader reader)
    {
        Player player = Main.player[reader.ReadInt32()];
        int rewardCount = reader.ReadInt32();

        player.GetModPlayer<SolynBookRewardsPlayer>().UnclaimedRewards.Clear();
        for (int i = 0; i < rewardCount; i++)
            player.GetModPlayer<SolynBookRewardsPlayer>().UnclaimedRewards.Add(ItemIO.Receive(reader, true));
    }
}
