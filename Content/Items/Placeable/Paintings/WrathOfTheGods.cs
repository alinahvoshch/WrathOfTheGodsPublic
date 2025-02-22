using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles.Paintings;
using NoxusBoss.Core.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Placeable.Paintings;

public class WrathOfTheGods : ModItem, ILocalizedModType
{
    public override string Texture => GetAssetPath("Content/Items/Placeable/Paintings", Name);

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
        Item.createTile = ModContent.TileType<WrathOfTheGodsTile>();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        for (int i = 0; i <= 1; i++)
        {
            TooltipLine? tooltip = tooltips.FirstOrDefault(t => t.Name == $"Tooltip{i}");
            if (tooltip is not null)
                tooltip.OverrideColor = DialogColorRegistry.NamelessDeityTextColor;
        }
    }
}
