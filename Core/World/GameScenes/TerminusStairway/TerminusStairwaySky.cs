using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Graphics;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using NoxusBoss.Core.World.Subworlds;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace NoxusBoss.Core.World.GameScenes.TerminusStairway;

public class TerminusStairwaySky : CustomSky
{
    private bool isActive;

    internal static float intensity;

    /// <summary>
    /// The animation completion interpolant of this sky.
    /// </summary>
    public static float CompletionInterpolant
    {
        get;
        set;
    }

    /// <summary>
    /// The render target responsible for holding visual data about the gradient.
    /// </summary>
    public static DownscaleOptimizedScreenTarget GradientTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The palette used for the bottom part of the background that's cycled based on how high up the player is.
    /// </summary>
    public static Palette BottomBackgroundPalette
    {
        get;
        private set;
    }

    /// <summary>
    /// The palette used for the top part of the background that's cycled based on how high up the player is.
    /// </summary>
    public static Palette TopBackgroundPalette
    {
        get;
        private set;
    }

    /// <summary>
    /// The desired amount of clouds to exist in the sky.
    /// </summary>
    public static int DesiredCloudCount => 200;

    /// <summary>
    /// The set of all clouds within the sky.
    /// </summary>
    public static readonly List<TerminusStairwayCloud> Clouds = new List<TerminusStairwayCloud>(256);

    /// <summary>
    /// The key associated with this sky.
    /// </summary>
    public const string ScreenShaderKey = "NoxusBoss:TerminusStairwaySky";

    public override void OnLoad()
    {
        GradientTarget = new(0.35f, DrawGradient);
    }

    public override void Update(GameTime gameTime)
    {
        CreateClouds();
        intensity = Saturate(intensity + isActive.ToDirectionInt());
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        CompletionInterpolant = TerminusStairwaySystem.WalkAnimationInterpolant;
        Render();
    }

    /// <summary>
    /// Creates clouds idly until a certain threshold of them exist.
    /// </summary>
    public static void CreateClouds()
    {
        Vector2 screenSize = ViewportSize;
        while (Clouds.Count < DesiredCloudCount)
        {
            Vector2 cloudPosition = new Vector2(Main.rand.NextFloat(1.15f, 3f) * screenSize.X * Main.rand.NextFromList(-1f, 1f), Main.rand.NextFloat(screenSize.Y));

            // If there's a significant shortage of clouds, that almost certainly means that they haven't been properly initialized yet.
            // To account for this, clouds under these circumstances are randomly created all around the screen, so that it doesn't take a long time for
            // clouds to come on-screen from the left.
            if (Clouds.Count < DesiredCloudCount * 0.67f)
                cloudPosition.X = Main.rand.NextFloat(-1f, 2f) * screenSize.X;

            Clouds.Add(new(cloudPosition, Main.rand.NextBool()));
        }
    }

    /// <summary>
    /// Renders the sky with a given completion interpolant. The greater the completion interpolant, the more space-like the sky becomes.
    /// </summary>
    public static void Render()
    {
        if (TotalScreenOverlaySystem.OverlayInterpolant >= 0.3f)
        {
            CreateClouds();
            intensity = 1f;
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin();

        GradientTarget.Render(Color.White);

        CalculateGradientInterpolants(CompletionInterpolant, out float brightenInterpolant, out float spaceInterpolant);
        RenderClouds(false, CompletionInterpolant, brightenInterpolant, spaceInterpolant);
        RenderBackgroundElements(Main.screenPosition, CompletionInterpolant);
        RenderClouds(true, CompletionInterpolant, brightenInterpolant, spaceInterpolant);

        // Draw the mist at the bottom of the screen.
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        DrawBottomMist(spaceInterpolant);

        // Draw space, if necessary.
        if (spaceInterpolant > 0f)
        {
            Vector2 parallax = Vector2.UnitY * InverseLerp(0f, (float)Main.worldSurface * 16f, Main.screenPosition.Y) * -0.1f;
            EternalGardenSkyStarRenderer.Render(Pow(spaceInterpolant, 2.7f), Matrix.CreateTranslation(parallax.X, parallax.Y, 0f));
        }

        // Reset the sprite batch again so that the Nameless Deity text can be rendered.
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        Main.spriteBatch.End();
        Main.spriteBatch.Begin();

        if (Main.gameMenu)
        {
            intensity = 1f;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
        else
        {
            DrawAscendText();
            Main.spriteBatch.ResetToDefault();
        }
    }

    /// <summary>
    /// Calculates the brighten and space interpolant from the current walk completion interpolant.
    /// </summary>
    /// <param name="completionInterpolant">The walk completion interpolant.</param>
    /// <param name="brightenInterpolant">The sky brightening interpolant.</param>
    /// <param name="spaceInterpolant">The space interpolant.</param>
    public static void CalculateGradientInterpolants(float completionInterpolant, out float brightenInterpolant, out float spaceInterpolant)
    {
        brightenInterpolant = InverseLerp(0.2f, 0.45f, completionInterpolant);
        spaceInterpolant = InverseLerp(0.5f, 0.95f, completionInterpolant);
    }

    /// <summary>
    /// Retrieves the current gradient colors for the top and bottom of the sky.
    /// </summary>
    /// <param name="brightenInterpolant">The brighten interpolant for the sky.</param>
    /// <param name="spaceInterpolant">The space interpolant for the sky.</param>
    /// <param name="top">The color at the top of the gradient.</param>
    /// <param name="bottom">The color at the bottom of the gradient.</param>
    public static void GetGradientColors(float brightenInterpolant, float spaceInterpolant, out Color top, out Color bottom)
    {
        Vector3 topSpaceColor = TerminusStairwaySystem.ReadPalette("SpaceTopColor")[0];
        Vector3 bottomSpaceColor = TerminusStairwaySystem.ReadPalette("SpaceBottomColor")[0];

        if (TopBackgroundPalette is null || BottomBackgroundPalette is null)
        {
            Vector3[] bottomPaletteVector = TerminusStairwaySystem.ReadPalette("BottomBackgroundGradient");
            Vector3[] topPaletteVector = TerminusStairwaySystem.ReadPalette("TopBackgroundGradient");
            BottomBackgroundPalette = new(bottomPaletteVector);
            TopBackgroundPalette = new(topPaletteVector);
        }

        top = TopBackgroundPalette.SampleColor(brightenInterpolant);
        bottom = BottomBackgroundPalette.SampleColor(brightenInterpolant);

        top = Color.Lerp(top, new(topSpaceColor), spaceInterpolant);
        bottom = Color.Lerp(bottom, new(bottomSpaceColor), spaceInterpolant.Squared());
    }

    private static void DrawGradient(int identifier)
    {
        CalculateGradientInterpolants(CompletionInterpolant, out float brightenInterpolant, out float spaceInterpolant);
        GetGradientColors(brightenInterpolant, spaceInterpolant, out Color top, out Color bottom);

        float bottomThreshold = SmoothStep(0.9f, 1f, brightenInterpolant);
        Vector3 topOfWorldSpaceColor = TerminusStairwaySystem.ReadPalette("TopOfWorldAdditiveSpaceColor")[0];
        ManagedShader backgroundShader = ShaderManager.GetShader("NoxusBoss.TerminusBackgroundShader");

        // Edge case for if the mod is unloading.
        if (backgroundShader.Shader.Value.IsDisposed)
            return;

        backgroundShader.TrySetParameter("bottomThreshold", bottomThreshold);
        backgroundShader.TrySetParameter("gradientTop", top.ToVector4());
        backgroundShader.TrySetParameter("gradientBottom", bottom.ToVector4());
        backgroundShader.TrySetParameter("screenPosition", Main.screenPosition);
        backgroundShader.TrySetParameter("additiveTopOfWorldColor", new Vector4(topOfWorldSpaceColor, 0f) * spaceInterpolant);
        backgroundShader.Apply();

        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea / WhitePixel.Size();
        Main.spriteBatch.Draw(WhitePixel, screenArea * 0.5f, null, Color.White * intensity, 0f, WhitePixel.Size() * 0.5f, textureArea, 0, 0f);
    }

    internal static void DrawBottomMist(float spaceInterpolant)
    {
        ManagedShader cloudShader = ShaderManager.GetShader("NoxusBoss.TerminusBottomCloudShader");
        cloudShader.TrySetParameter("screenOffset", Main.screenPosition / Main.ScreenSize.ToVector2() * new Vector2(0.15f, -0.03f));
        cloudShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        cloudShader.Apply();

        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea / WhitePixel.Size();
        Main.spriteBatch.Draw(WhitePixel, screenArea * 0.5f, null, Color.White * intensity * (1f - spaceInterpolant).Squared(), 0f, WhitePixel.Size() * 0.5f, textureArea, 0, 0f);
    }

    private static void DrawAscendText()
    {
        float textOpacity = InverseLerpBump(120f, 240f, 420f, 540f, TerminusStairwaySystem.Time);
        if (textOpacity <= 0f)
            return;

        DynamicSpriteFont font = FontRegistry.Instance.NamelessDeityText;
        string text = Language.GetTextValue("Mods.NoxusBoss.Dialog.NamelessDeityAscendText");

        // Draw the text.
        float textScale = 1.35f;
        Color textColor = DialogColorRegistry.NamelessDeityTextColor * textOpacity;
        Vector2 drawPosition = new Vector2(Main.instance.GraphicsDevice.Viewport.Width * 0.5f, 300f) - font.MeasureString(text) * textScale * 0.5f;
        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, drawPosition, textColor, 0f, Vector2.Zero, Vector2.One * textScale);
    }

    internal static void RenderBackgroundElements(Vector2 screenPosition, float completionInterpolant)
    {
        float screenParallaxMultiplier = Main.gameMenu ? 0.1f : 0.4f;

        int backgroundIndex = (int)Round(Lerp(180f, 183f, Main.GlobalTimeWrappedHourly * 2f % 1f));
        Main.instance.LoadBackground(backgroundIndex);
        Texture2D texture = TextureAssets.Background[backgroundIndex].Value;

        // In accordance with Terraria precedence.
        float scale = 2.5f;

        float skyHeight = -100f - SmoothStep(0f, 700f, completionInterpolant.Squared());

        // Keep in mind that y paralex should always be half of x's or it will feel odd compared to how Terraria does it.
        // Keep in mind when you change screen parralax it affects the y offset for the bg in the world.
        int x = (int)(screenPosition.X * screenParallaxMultiplier);
        x %= (int)(texture.Width * scale);
        int y = (int)(screenPosition.Y * 0.5f * screenParallaxMultiplier);

        // Y offset to align with whatever position you want it in the world (is affected by screenParallaxMultiplier as stated before).
        y -= Main.gameMenu ? 300 : 1800;

        float screenWidth = Main.instance.GraphicsDevice.Viewport.Width * 0.5f;
        float screenHeight = Main.instance.GraphicsDevice.Viewport.Height * 0.5f;
        Vector2 position = texture.Size() * scale * 0.5f;
        Color color = new Color(TerminusStairwaySystem.ReadPalette("BackgroundElementOverlayColor")[0]);

        // This loops the BG horizontally.
        for (int i = -1; i <= 2; i++)
        {
            Vector2 localPosition = new Vector2(screenWidth - x + texture.Width * i * scale, screenHeight - y);
            Main.spriteBatch.Draw(texture, localPosition - position, null, color, 0f, new Vector2(0f, skyHeight), scale, SpriteEffects.None, 0f);
        }
    }

    internal static void RenderClouds(bool inFrontOfFloatingIslands, float completionInterpolant, float brightenInterpolant, float spaceInterpolant)
    {
        float opacity = InverseLerp(0.75f, 0.5f, completionInterpolant);
        Vector2 movementOffset = Main.LocalPlayer.velocity * (Main.gameMenu ? 0f : -0.09f) + Vector2.UnitX * (Main.gameMenu ? 0.1f : 0.25f);
        if (inFrontOfFloatingIslands)
            movementOffset *= 1.85f;
        if (Main.gamePaused)
            movementOffset = Vector2.Zero;

        IEnumerable<TerminusStairwayCloud> orderedClouds = Clouds.OrderBy(c => TextureAssets.Cloud[c.TextureVariant].Value.Width * c.Scale);
        GetGradientColors(brightenInterpolant, spaceInterpolant, out Color top, out Color bottom);

        foreach (TerminusStairwayCloud cloud in orderedClouds)
        {
            if (cloud.InFrontOfFloatingIslands != inFrontOfFloatingIslands)
                continue;

            float scale = cloud.Scale;

            cloud.Position += movementOffset / (scale + 1f);

            Texture2D cloudTexture = TextureAssets.Cloud[cloud.TextureVariant].Value;
            Vector2 cloudDrawPosition = cloud.Position;
            cloudDrawPosition.Y += completionInterpolant.Squared() * Main.screenHeight * 1.5f;

            Color baseColor = Color.Lerp(top, bottom, Pow(InverseLerp(0f, Main.screenHeight, cloudDrawPosition.Y), 0.56f));
            Color color = Color.Lerp(Color.Black, baseColor, Saturate(scale - 0.1f) + 0.55f);

            if (cloud.InFrontOfFloatingIslands)
                color *= 1.132f;

            Main.spriteBatch.Draw(cloudTexture, cloudDrawPosition, null, color * opacity, 0f, cloudTexture.Size() * 0.5f, scale, 0, 0f);
        }

        Clouds.RemoveAll(c => c.Position.X >= Main.instance.GraphicsDevice.Viewport.Width * (Main.gameMenu ? 1.8f : 5.5f) || c.Position.Y <= -900f);
    }

    #region Boilerplate
    public override void Activate(Vector2 position, params object[] args)
    {
        isActive = true;
    }

    public override void Deactivate(params object[] args)
    {
        if (!Main.gameMenu)
            Clouds.Clear();

        isActive = false;
    }

    public override float GetCloudAlpha() => 1f;

    public override bool IsActive()
    {
        return isActive || intensity > 0f;
    }

    public override Color OnTileColor(Color inColor)
    {
        Main.ColorOfTheSkies = Color.Lerp(Main.ColorOfTheSkies, Color.Black, 0.9f);
        return Color.Lerp(inColor, new(255, 0, 81), intensity * 0.7f);
    }

    public override void Reset()
    {
        isActive = false;
    }
    #endregion Boilerplate
}
