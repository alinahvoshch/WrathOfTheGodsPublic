using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.DataStructures.Conditions;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// How much the Ancient Writings book costs from the clothier.
    /// </summary>
    private static int AncientWritingsBuyPrice => Item.buyPrice(120, 0, 0, 0);

    private static void LoadAncientWritingsObtainment()
    {
        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, loot) =>
        {
            if (npc.type == NPCID.Clothier)
                loot.Add(new CommonDrop(SolynBookAutoloader.Books["AncientWritings"].Type, 1));
        };
        GlobalNPCEventHandlers.ModifyShopEvent += shop =>
        {
            if (shop.NpcType == NPCID.Clothier)
            {
                Item ancientWritings = new Item(SolynBookAutoloader.Books["AncientWritings"].Type)
                {
                    shopCustomPrice = AncientWritingsBuyPrice
                };
                shop.Add(ancientWritings, CustomConditions.BooksObtainable);
            }
        };
        SolynBookAutoloader.Books["AncientWritings"].ModifyLinesAction = (item, tooltips) =>
        {
            if (!item.isAShopItem)
                return;

            tooltips.RemoveAll(l => l.Name.Contains("Tooltip"));
            tooltips.Add(new TooltipLine(ModContent.GetInstance<NoxusBoss>(), "Tooltip", item.ModItem.GetLocalizedValue("ShopTooltip")));
        };
    }
}
