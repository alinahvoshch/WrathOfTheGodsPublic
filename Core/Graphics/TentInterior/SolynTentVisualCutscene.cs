using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.DataStructures.ShapeCurves;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.World.Subworlds;
using ReLogic.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.Utilities;

namespace NoxusBoss.Core.Graphics.TentInterior;

public abstract class SolynTentVisualCutscene : ModType
{
    /// <summary>
    /// A timer which dictates how long it's been since the cutscene began.
    /// </summary>
    public int Time
    {
        get;
        protected internal set;
    }

    /// <summary>
    /// Whether this cutscene is active or not.
    /// </summary>
    public bool IsActive
    {
        get;
        protected set;
    }

    /// <summary>
    /// The amount by which the space background visual should fade out.
    /// </summary>
    public float BackgroundFadeOut
    {
        get;
        protected set;
    }

    /// <summary>
    /// The standard duration of this cutscene.
    /// </summary>
    public abstract int StandardDuration
    {
        get;
    }

    /// <summary>
    /// The render target that contains all text data.
    /// </summary>
    public static InstancedRequestableTarget TextTarget
    {
        get;
        private set;
    }


    protected sealed override void Register()
    {
        TextTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(TextTarget);

        SetStaticDefaults();
        ModTypeLookup<SolynTentVisualCutscene>.Register(this);
    }

    protected static void RenderText(string textKey, Color color, float scale = 1.1f, Vector2? drawPositionOverride = null)
    {
        if (color == Color.Transparent)
            return;

        string text = Language.GetTextValue(textKey);
        if (text.Length <= 0)
            return;

        int identifier = textKey.GetHashCode();
        DynamicSpriteFont font = FontRegistry.Instance.SolynText;
        Vector2 textArea = font.MeasureString(text);
        Vector2 renderTargetArea = Vector2.One * MathF.Max(textArea.X, textArea.Y) * 1.42f;
        Vector2 origin = textArea * 0.5f;
        TextTarget.Request((int)renderTargetArea.X, (int)renderTargetArea.Y, identifier, () =>
        {
            Main.spriteBatch.Begin();

            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                Vector2 lineOrigin = font.MeasureString(lines[i]) * 0.5f;
                ChatManager.DrawColorCodedString(Main.spriteBatch, font, lines[i], renderTargetArea * 0.5f + Vector2.UnitY * i * 50f, Color.White, 0f, lineOrigin, Vector2.One);
            }
            Main.spriteBatch.End();
        });

        if (!TextTarget.TryGetTarget(identifier, out RenderTarget2D? textTarget) || textTarget is null)
            return;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

        Vector2 textDrawPosition = ViewportSize * (drawPositionOverride ?? new Vector2(0.5f, 0.37f));
        Main.spriteBatch.Draw(textTarget, textDrawPosition, null, color, 0f, textTarget.Size() * 0.5f, scale, 0, 0f);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin();
    }

    protected static void DrawConstellation(string constellationName, Vector2 drawPosition, float appearInterpolant, float rotation, Vector2 upscaleFactor, float starVarianceFactor = 0.09f, float opacityFactor = 1f)
    {
        // Don't do anything if no valid constellation could not be found.
        if (!ShapeCurveManager.TryFind(constellationName, out ShapeCurve? curve) || curve is null)
            return;

        Texture2D beautifulStarTexture = GennedAssets.Textures.GreyscaleTextures.Star.Value;
        UnifiedRandom rng = new UnifiedRandom(1904);

        for (int i = 0; i < curve.ShapePoints.Count; i++)
        {
            Vector2 constellationOffset = (curve.ShapePoints[i] - curve.Center) * upscaleFactor + rng.NextVector2Circular(120f, 120f) * starVarianceFactor;
            Vector2 starDrawPosition = (drawPosition + constellationOffset).RotatedBy(rotation, drawPosition);
            starDrawPosition += rng.NextVector2Circular(500f, 500f) * (1f - appearInterpolant);

            Star star = new Star()
            {
                twinkle = Lerp(0.4f, 0.87f, Cos01(i / (float)curve.ShapePoints.Count * 2360f + Main.GlobalTimeWrappedHourly * 1.75f)),
                scale = Sqrt(appearInterpolant) * 2f
            };

            float opacity = star.twinkle * Cbrt(appearInterpolant) * opacityFactor * 0.42f;
            float starRotation = Main.GlobalTimeWrappedHourly * 0.5f + i;
            float hueInterpolant = rng.NextFloat().Squared();
            Color bloomColor = (EternalGardenSkyStarRenderer.StarPalette.SampleColor(hueInterpolant) with { A = 0 }) * opacity * 0.32f;
            Color twinkleColor = new Color(255, 255, 255, 0) * opacity * 0.6f;
            TwinkleParticle.DrawTwinkle(beautifulStarTexture, starDrawPosition, 4, starRotation, bloomColor, twinkleColor, Vector2.One * star.scale * star.twinkle * 0.025f, 0.7f);
        }
    }

    /// <summary>
    /// Starts this cutscene.
    /// </summary>
    public void Start()
    {
        IsActive = true;
        Time = 1;
        BackgroundFadeOut = 0f;
    }

    /// <summary>
    /// Immediately ends this cutscene.
    /// </summary>
    public void End()
    {
        IsActive = false;
        Time = 0;
        BackgroundFadeOut = 0f;
    }

    /// <summary>
    /// Updates this cutscene, incrementing its timer by default.
    /// </summary>
    public virtual void Update()
    {
        Time++;
        if (Time >= StandardDuration)
            End();
    }

    /// <summary>
    /// Renders this cutscene to the black background in Solyn's tent.
    /// </summary>
    public abstract void Render();
}
