using Luminance.Core.Graphics;
using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using NoxusBoss.Content.Tiles;
using NoxusBoss.Core.DataStructures;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Placeable;

public class LotusOfCreation : ModItem
{
    public class LotusOfCreationRarity : ModRarity
    {
        /// <summary>
        /// The palette that this rarity cycles through.
        /// </summary>
        public static readonly Palette RarityPalette = new Palette().
            AddColor(new Color(0, 0, 0)).
            AddColor(new Color(71, 35, 137)).
            AddColor(new Color(120, 60, 231)).
            AddColor(new Color(46, 156, 211));

        public override Color RarityColor => RarityPalette.SampleColor(Main.GlobalTimeWrappedHourly * 0.2f % 1f) * 1.4f;
    }

    public override string Texture => GetAssetPath("Content/Items/Placeable", Name);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        On_Player.SellItem += DisallowSelling;
        new ManagedILEdit("Use Special Sell Text for Lotus of Creation", Mod, edit =>
        {
            IL_Main.MouseText_DrawItemTooltip += edit.SubscriptionWrapper;
        }, edit =>
        {
            IL_Main.MouseText_DrawItemTooltip -= edit.SubscriptionWrapper;
        }, UseSpecialSellText).Apply(false);
    }

    private bool DisallowSelling(On_Player.orig_SellItem orig, Player self, Item item, int stack)
    {
        if (item.type == Type)
            return false;

        return orig(self, item, stack);
    }

    private void UseSpecialSellText(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);

        if (!cursor.TryGotoNext(MoveType.Before, c => c.MatchLdcI4(49)))
        {
            edit.LogFailure("The 49 integer constant load could not be found!");
            return;
        }

        if (!cursor.TryGotoNext(MoveType.Before, c => c.MatchStelemRef()))
        {
            edit.LogFailure("The element reference storage could not be found!");
            return;
        }

        cursor.EmitDelegate((string originalText) =>
        {
            if (Main.HoverItem.type == Type)
                return this.GetLocalizedValue("SellText");

            return originalText;
        });
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<LotusOfCreationTile>());
        Item.width = 66;
        Item.height = 20;
        Item.rare = ModContent.RarityType<LotusOfCreationRarity>();
        Item.value = Item.sellPrice(0, 35, 0, 0);
        Item.consumable = true;
    }

    private static void DrawLotusWithShader(Vector2 drawPosition, float scale)
    {
        Texture2D lotusTexture = TextureAssets.Item[ModContent.ItemType<LotusOfCreation>()].Value;

        Vector3[] palette = LotusOfCreationTile.ShaderPalette;
        ManagedShader lotusShader = ShaderManager.GetShader("NoxusBoss.LotusOfCreationShader");
        lotusShader.TrySetParameter("appearanceInterpolant", 1f);
        lotusShader.TrySetParameter("gradient", palette);
        lotusShader.TrySetParameter("gradientCount", palette.Length);
        lotusShader.Apply();

        Main.spriteBatch.Draw(lotusTexture, drawPosition, null, Color.White, 0f, lotusTexture.Size() * 0.5f, scale, 0, 0f);
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, Main.UIScaleMatrix);
        DrawLotusWithShader(position, scale);
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);

        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Main.spriteBatch.PrepareForShaders();
        DrawLotusWithShader(Item.position - Main.screenPosition, scale);
        Main.spriteBatch.ResetToDefault();

        return false;
    }
}

