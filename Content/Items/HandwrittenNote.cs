using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.Graphics.UI.HandwrittenNote;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public class HandwrittenNote : ModItem
{
    public override string Texture => GetAssetPath("Content/Items", Name);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 22;
        Item.rare = ModContent.RarityType<SolynRewardRarity>();
        Item.value = 0;
        Item.useTime = Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.noUseGraphic = true;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI && player.itemAnimation == player.itemAnimationMax)
            HandwrittenNoteUI.Active = !HandwrittenNoteUI.Active;

        return true;
    }
}
