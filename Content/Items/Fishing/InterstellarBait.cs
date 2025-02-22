using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Fishing;

public class InterstellarBait : ModItem
{
    public override string Texture => GetAssetPath("Content/Items/Fishing", Name);

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = ModContent.RarityType<SolynRewardRarity>();
        Item.value = Item.sellPrice(0, 20, 0, 0);
        Item.bait = 80;
    }

    public override bool? CanConsumeBait(Player player) => false;

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) =>
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.FishingBait;
}
