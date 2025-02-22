using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles.Paintings;
using NoxusBoss.Core.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace NoxusBoss.Content.Items.Placeable.Paintings;

public class Xenqiterthralyensyr : ModItem, ILocalizedModType
{
    public override string Texture => GetAssetPath("Content/Items/Placeable/Paintings", Name);

    public override void SetStaticDefaults()
    {
        // Make this item not trash-able via shift click, to ensure that the player doesn't just delete it while on autopilot.
        On_ItemSlot.LeftClick_SellOrTrash += MakeItemUntrashableWithShift;
    }

    private bool MakeItemUntrashableWithShift(On_ItemSlot.orig_LeftClick_SellOrTrash orig, Item[] inv, int context, int slot)
    {
        if (inv[slot].type != Type)
            return orig(inv, context, slot);
        return false;
    }

    public override void SetDefaults()
    {
        Item.width = 44;
        Item.height = 24;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        Item.value = 0;
        Item.createTile = ModContent.TileType<XenqiterthralyensyrTile>();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        // Make the tooltip line use Nameless' dialog.
        TooltipLine? firstLine = tooltips.FirstOrDefault(t => t.Name == "Tooltip0");
        if (firstLine is not null)
            firstLine.OverrideColor = DialogColorRegistry.NamelessDeityTextColor;
    }
}
