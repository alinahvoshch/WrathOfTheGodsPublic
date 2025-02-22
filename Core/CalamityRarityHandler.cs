using NoxusBoss.Core.CrossCompatibility.Inbound;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core;

// Convenient utilities that basically just allow for the safe setting of Calamity rarities and sell prices without a strong reference.
public static class CalamityRarityHandler
{
    public static readonly int RarityVioletBuyPrice = Item.buyPrice(1, 50, 0, 0);

    public static void UseVioletRarity(this Item item)
    {
        item.rare = ItemRarityID.Purple;
        item.value = RarityVioletBuyPrice;
        if (ModReferences.Calamity?.TryFind("Violet", out ModRarity rarity) ?? false)
            item.rare = rarity.Type;
    }
}
