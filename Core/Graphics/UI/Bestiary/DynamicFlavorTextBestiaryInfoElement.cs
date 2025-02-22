using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.Utilities;

namespace NoxusBoss.Core.Graphics.UI.Bestiary;

public class DynamicFlavorTextBestiaryInfoElement(string[] languageKeys, DynamicSpriteFont font) : IBestiaryInfoElement
{
    private string chosenText;

    private readonly string[] keys = languageKeys;

    private readonly DynamicSpriteFont font = font;

    public UIElement? ProvideUIElement(BestiaryUICollectionInfo info)
    {
        if (info.UnlockState < BestiaryEntryUnlockState.CanShowStats_2)
            return null;

        // Initialize the RNG if necessary and choose new text.
        Main.rand ??= new UnifiedRandom();
        string oldText = chosenText;
        do
            chosenText = Language.GetTextValue(Main.rand.Next(keys));
        while (chosenText == oldText);

        UIPanel panel = new UIPanel(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Panel", AssetRequestMode.ImmediateLoad), null, 12, 7)
        {
            Width = new StyleDimension(-11f, 1f),
            Height = new StyleDimension(109f, 0f),
            BackgroundColor = new Color(43, 56, 101),
            BorderColor = Color.Transparent,
            Left = new StyleDimension(3f, 0f),
            PaddingLeft = 4f,
            PaddingRight = 4f
        };

        UITextDynamic text = new UITextDynamic(chosenText, Color.Lerp(DialogColorRegistry.NamelessDeityTextColor, Color.White, 0.67f), 0.32f, font)
        {
            HAlign = 0f,
            VAlign = 0f,
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            IsWrapped = true,
            PreDrawText = PrepareForDrawing,
            PostDrawText = AfterDrawing,
        };
        AddDynamicResize(panel, text);
        panel.Append(text);
        return panel;
    }

    private void PrepareForDrawing()
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

        // Apply the aberration dye effect on top of the text.
        float aberrationPower = Pow(AperiodicSin(Main.GlobalTimeWrappedHourly * 32f), 2f) * 0.4f;
        ManagedScreenFilter aberrationShader = ShaderManager.GetFilter("NoxusBoss.ChromaticAberrationShader");
        aberrationShader.TrySetParameter("splitIntensity", aberrationPower);
        aberrationShader.TrySetParameter("impactPoint", Vector2.One * 0.5f);
        aberrationShader.Apply();
    }

    private void AfterDrawing()
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
    }

    private static void AddDynamicResize(UIElement container, UITextDynamic text)
    {
        text.OnInternalTextChange += () =>
        {
            container.Height = new StyleDimension(text.MinHeight.Pixels, 0f);
        };
    }
}
