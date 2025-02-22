using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Items;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Graphics.UI.Books;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static NoxusBoss.Core.Autoloaders.SolynBooks.SolynBookAutoloader;

namespace NoxusBoss.Core.Autoloaders.SolynBooks;

[Autoload(false)]
public class AutoloadableSolynBook : ModItem
{
    private readonly string name;

    private static readonly Asset<Texture2D> starFilled = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Rank_Light");

    private static readonly Asset<Texture2D> starBlank = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Rank_Dim");

    /// <summary>
    /// The optional tile ID that this item should place.
    /// </summary>
    public int? PlacementTileID
    {
        get;
        set;
    }

    /// <summary>
    /// The data associated with this book.
    /// </summary>
    public LoadableBookData Data
    {
        get;
        init;
    }

    /// <summary>
    /// An optional action that can be performed for the purpose of modifying text lines.
    /// </summary>
    public Action<Item, List<TooltipLine>>? ModifyLinesAction
    {
        get;
        set;
    }

    /// <summary>
    /// An optional action that can be performed for the purpose of modifying tooltip rendering.
    /// </summary>
    public Action<Item, int, int>? PreDrawTooltipAction
    {
        get;
        set;
    }

    public override string Name => name;

    public override string Texture => Data.TexturePath;

    protected override bool CloneNewInstances => true;

    public AutoloadableSolynBook(LoadableBookData data)
    {
        data.TexturePath = data.TexturePath.Replace("../", "NoxusBoss/Assets/Textures/Content/Items/SolynBooks/");
        name = Path.GetFileName(data.TexturePath);
        Data = data;
    }

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.rare = ItemRarityID.Quest;
        Item.maxStack = 9999;

        if (PlacementTileID.HasValue)
            Item.DefaultToPlaceableTile(PlacementTileID.Value);

        bool dontSpawn = !SolynBooksSystem.BooksObtainable || SolynBookExchangeRegistry.RedeemedBooks.Contains(Name);
        if (dontSpawn && !Main.gameMenu)
            Item.stack = 0;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.RemoveAll(t => t.Name.Contains("Tooltip"));

        if (SolynBooksSystem.IsUnobtainedItemInUI(Item))
        {
            string obtainmentHintText = this.GetLocalizedValue("ObtainmentHint");
            if (!string.IsNullOrEmpty(obtainmentHintText))
                tooltips.Add(new TooltipLine(Mod, "ObtainmentHint", obtainmentHintText));
        }
        else
            tooltips.Add(new TooltipLine(Mod, "Tooltip", this.GetLocalizedValue("Tooltip")));

        ModifyLinesAction?.Invoke(Item, tooltips);

        if (!SolynBooksSystem.IsUnobtainedItemInUI(Item))
        {
            if (Item.Wrath().SlotInBookshelfUI && SolynBookExchangeRegistry.RedeemedBooksCreditRelationship.TryGetValue(Name, out string? redeemerName))
            {
                string creditText = Language.GetText("Mods.NoxusBoss.UI.SolynBookExchange.CreditText").Format(redeemerName);
                tooltips.Add(new TooltipLine(Mod, "CreditLine", Environment.NewLine + creditText));
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "StarsLine", Language.GetTextValue("Mods.NoxusBoss.UI.SolynBookExchange.RarityText"))
                {
                    OverrideColor = ModContent.GetInstance<SolynRewardRarity>().RarityColor
                });
            }
        }
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (string.IsNullOrWhiteSpace(line.Text))
            yOffset += 1;

        // Account for the special source code tooltip (that you're currently reading!) by moving down tooltips a bit.
        // This effect is not applied to the item name.
        if (Item.type == Books["FuturisticTreatise"].Type && line.Name != "ItemName" && !SolynBooksSystem.IsUnobtainedItemInUI(Main.HoverItem))
            line.Y += (int)(GennedAssets.Textures.SolynBooks.FuturisticTreatiseCode.Value.Height * SolynBooksSystem.FuturisticTreatiseCodeScale);

        if (line.Name == "StarsLine")
        {
            // Render the sets of stars next to the 'Rarity: ' line in accordance with how rare the books is.
            // By default, only three stars are rendered, but if a book has a rarity exceeding three, more are added.
            float starScale = 1.25f;
            float textWidth = line.Font.MeasureString(line.Text).X;
            int starCount = Math.Max(3, Data.Rarity);
            for (int i = 0; i < starCount; i++)
            {
                Texture2D starTexture = (i >= Data.Rarity ? starBlank : starFilled).Value;
                Vector2 starDrawPosition = new Vector2(line.X + i * starScale * 15f + textWidth, line.Y + 2f);
                Main.spriteBatch.Draw(starTexture, starDrawPosition, null, Color.White, 0f, Vector2.Zero, starScale, 0, 0f);
            }
        }
        return true;
    }

    public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        PreDrawTooltipAction?.Invoke(Item, x, y);
        return true;
    }

    public static void DrawWithOutline(Texture2D texture, Vector2 drawPosition, float opacity, float rotation)
    {
        for (int i = 0; i < 32; i++)
        {
            Vector2 drawOffset = (TwoPi * i / 32f).ToRotationVector2() * 2f;
            Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, new Color(255, 255, 120, 0) * opacity, rotation, Vector2.Zero, 1f, 0, 0f);
        }
        Main.spriteBatch.Draw(texture, drawPosition, null, Color.White * opacity, rotation, Vector2.Zero, 1f, 0, 0f);
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        DrawWithOutline(TextureAssets.Item[Type].Value, Item.position - Main.screenPosition - Vector2.UnitY * 5f, 1f, rotation);
        return false;
    }
}
