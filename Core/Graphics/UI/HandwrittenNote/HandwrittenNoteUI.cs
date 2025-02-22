using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Core.Graphics.RenderTargets;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace NoxusBoss.Core.Graphics.UI.HandwrittenNote;

public class HandwrittenNoteUI : ModSystem
{
    internal static LazyAsset<Texture2D> ChineseText;

    internal static LazyAsset<Texture2D> EnglishText;

    internal static LazyAsset<Texture2D> RussianText;

    /// <summary>
    /// Whether the note UI is active or not.
    /// </summary>
    public static bool Active
    {
        get;
        set;
    }

    /// <summary>
    /// The general scale of the note.
    /// </summary>
    public static float Scale => 0.96f;

    /// <summary>
    /// The animation completion of the note.
    /// </summary>
    public static float AnimationCompletion
    {
        get;
        set;
    }

    /// <summary>
    /// The render target that holds draw data for the note's text, because apparently text can't handle universal rotations without this.
    /// </summary>
    public static InstancedRequestableTarget TextTarget
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            TextTarget = new InstancedRequestableTarget();
            Main.ContentThatNeedsRenderTargets.Add(TextTarget);

            ChineseText = LazyAsset<Texture2D>.FromPath(GetAssetPath("UI/HandwrittenNote", "Chinese"));
            EnglishText = LazyAsset<Texture2D>.FromPath(GetAssetPath("UI/HandwrittenNote", "English"));
            RussianText = LazyAsset<Texture2D>.FromPath(GetAssetPath("UI/HandwrittenNote", "Russian"));
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
        layers.Insert(mouseIndex, new LegacyGameInterfaceLayer("Wrath of the Gods: Solyn's Handwritten Note", () =>
        {
            RenderNote();
            return true;
        }, InterfaceScaleType.None));
    }

    /// <summary>
    /// Renders the note.
    /// </summary>
    public static void RenderNote()
    {
        AnimationCompletion = Saturate(AnimationCompletion + Active.ToDirectionInt() * 0.0167f);
        if (AnimationCompletion <= 0f)
            return;

        // Disable the UI if the player clicks anywhere.
        if (Main.mouseLeft && Main.mouseLeftRelease && AnimationCompletion >= 0.4f)
            Active = false;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

        ManagedShader paperShader = ShaderManager.GetShader("NoxusBoss.HandwrittenNoteBackgroundShader");
        paperShader.SetTexture(WavyBlotchNoiseDetailed, 1, SamplerState.LinearWrap);
        paperShader.Apply();

        Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
        Texture2D background = GennedAssets.Textures.Extra.Paper.Value;
        Vector2 size = new Vector2(1024f, 576f) * Scale;

        float opacity = Pow(InverseLerp(0f, 0.67f, AnimationCompletion), 1.5f);
        Color noteColor = new Color(249, 240, 232) * opacity;
        Color shadowColor = Color.Black * opacity.Squared() * 0.15f;
        float noteRotation = EasingCurves.Quartic.Evaluate(EasingType.InOut, -0.21f, 0.048f, AnimationCompletion);
        Vector2 noteCenter = screenCenter + Vector2.UnitY * Scale * EasingCurves.Quartic.Evaluate(EasingType.InOut, 600f, 0f, AnimationCompletion);

        // Draw the note background and its backshadow.
        for (int i = 0; i < 40; i++)
        {
            float interpolant = i / 40f;
            Vector2 shadowOffset = new Vector2(1.4f, 0.95f).RotatedBy(noteRotation) * Scale * i;
            Main.spriteBatch.Draw(background, noteCenter + shadowOffset, null, shadowColor * (1f - interpolant).Squared(), noteRotation, background.Size() * 0.5f, size / background.Size(), 0, 0f);
        }
        Main.spriteBatch.Draw(background, noteCenter, null, noteColor, noteRotation, background.Size() * 0.5f, size / background.Size(), 0, 0f);

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        // Draw the note text to the target.
        TextTarget.Request((int)size.X, (int)size.Y, 0, () =>
        {
            Main.spriteBatch.Begin();

            Vector2 center = ViewportSize * 0.5f;
            if (FontRegistry.EnglishGameCulture.IsActive)
                DrawText(center, size, opacity, EnglishText);
            else if (FontRegistry.ChineseGameCulture.IsActive)
                DrawText(center, size, opacity, ChineseText);
            else if (FontRegistry.RussianGameCulture.IsActive)
                DrawText(center, size, opacity, RussianText);
            else
                DrawText_Default(size, opacity);

            Main.spriteBatch.End();
        });

        if (TextTarget.TryGetTarget(0, out RenderTarget2D? target) && target is not null)
            Main.spriteBatch.Draw(target, noteCenter, null, Color.White, noteRotation, target.Size() * 0.5f, 1f, 0, 0f);

        Main.spriteBatch.ResetToDefaultUI();
    }

    /// <summary>
    /// Renders Solyn's note text directly via custom image.
    /// </summary>
    public static void DrawText(Vector2 noteCenter, Vector2 size, float opacity, LazyAsset<Texture2D> text)
    {
        Texture2D texture = text.Value;
        Main.spriteBatch.Draw(texture, noteCenter, null, Color.White * opacity, 0f, texture.Size() * 0.5f, size / texture.Size() * Scale * 0.95f, 0, 0f);
    }

    /// <summary>
    /// Renders Solyn's note text manually via font, for cases in which there is no localized image.
    /// </summary>
    public static void DrawText_Default(Vector2 size, float opacity)
    {
        float textScale = Scale * 0.6f;
        float textPadding = Scale * 40f;
        string overallText = Language.GetTextValue("Mods.NoxusBoss.Dialog.SolynHandwrittenNoteFarewellText");
        string[] lines = Utils.WordwrapString(overallText, FontAssets.DeathText.Value, (int)((size.X - textPadding * 2f) / textScale), 40, out int lineCount);

        Vector2 textStart = Vector2.One * textPadding;
        for (int i = 0; i <= lineCount; i++)
        {
            bool useSpecialFont = i == lineCount;
            string text = lines[i];
            DynamicSpriteFont font = useSpecialFont ? FontRegistry.Instance.SolynText : FontAssets.DeathText.Value;

            Vector2 textDrawPosition = textStart + Vector2.UnitY * i * textScale * 40f;
            Color baseColor = useSpecialFont ? Color.HotPink : new Color(30, 30, 30);
            float scaleFactor = useSpecialFont ? 1.33f : 1f;
            Color textShadowColor = useSpecialFont ? Color.Black : Color.Transparent;

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, textDrawPosition, baseColor * opacity, textShadowColor, 0f, Vector2.Zero, new(textScale * scaleFactor), -1, 1.4f);
        }
    }
}
