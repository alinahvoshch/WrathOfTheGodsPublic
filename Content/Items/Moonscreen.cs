using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Content.Items;

public class Moonscreen : ModItem
{
    /// <summary>
    /// Whether the Moonscreen effect is enabled.
    /// </summary>
    public bool Opened;

    public override string Texture => GetAssetPath("Content/Items", Name);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 40;
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
    }

    public override void SaveData(TagCompound tag)
    {
        tag["Opened"] = Opened;
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.TryGet("Opened", out bool opened))
            Opened = opened;
    }

    public override void NetSend(BinaryWriter writer) => writer.Write((byte)Opened.ToInt());

    public override void NetReceive(BinaryReader reader) => Opened = reader.ReadByte() == 1;

    public override bool CanRightClick() => true;

    public override void RightClick(Player player) => Opened = !Opened;

    public override bool ConsumeItem(Player player) => false;

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (Opened)
        {
            Texture2D openedTexture = GennedAssets.Textures.Items.MoonscreenOpened;
            spriteBatch.Draw(openedTexture, Item.position - Main.screenPosition, null, lightColor, 0f, Vector2.Zero, 1f, 0, 0);
            return false;
        }

        return true;
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (Opened)
        {
            Texture2D openedTexture = GennedAssets.Textures.Items.MoonscreenOpened;
            spriteBatch.Draw(openedTexture, position, null, Color.White, 0f, origin, scale, 0, 0);
            return false;
        }

        return true;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (!Opened)
        {
            string[] tooltipLines = this.GetLocalizedValue("TooltipClosed").Split('\n');
            TooltipLine? firstLine = tooltips.FirstOrDefault(t => t.Name == "Tooltip0");
            TooltipLine? secondLine = tooltips.FirstOrDefault(t => t.Name == "Tooltip1");

            if (firstLine is not null)
                firstLine.Text = tooltipLines[0];
            if (secondLine is not null)
                secondLine.Text = tooltipLines[1];
        }
    }
}
