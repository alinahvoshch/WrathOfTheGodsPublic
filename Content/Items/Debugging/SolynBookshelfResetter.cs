using NoxusBoss.Core.Graphics.UI.Books;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Debugging;

public class SolynBookshelfResetter : DebugItem
{
    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 36;
        Item.useAnimation = 40;
        Item.useTime = 40;
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = null;
        Item.rare = ItemRarityID.Blue;
        Item.value = 0;
    }

    public override bool? UseItem(Player p)
    {
        if (Main.myPlayer == NetmodeID.MultiplayerClient || p.itemAnimation != p.itemAnimationMax - 1)
            return false;

        SolynBookExchangeRegistry.RedeemedBooks.Clear();

        foreach (Player player in Main.ActivePlayers)
            player.GetModPlayer<SolynBookRewardsPlayer>().UnclaimedRewards.Clear();
        ModContent.GetInstance<SolynBookExchangeUI>().NewPulseInterpolants.Clear();
        return null;
    }
}
