using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.FormPresets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.Data;
using NoxusBoss.Core.Graphics.BackgroundManagement;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Graphics.UI.GraphicalUniverseImager;
using NoxusBoss.Core.World.GameScenes.TerminusStairway;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.Subworlds;

public class EternalGardenSky : Background
{
    /// <summary>
    /// The position of Nameless' eye in the background.
    /// </summary>
    public Vector2 EyePosition
    {
        get;
        set;
    }

    /// <summary>
    /// The amount of parallax for this background.
    /// </summary>
    public static float ScreenParallaxSpeed => 0.2f;

    /// <summary>
    /// The ambient color of the background texture.
    /// </summary>
    public static Color BackgroundAmbientColor
    {
        get
        {
            // The RGB values of the background are specially chosen such that it gains a majority blue overtone by default (because the auroras and lake are majority blue), while
            // suppressing purples, since they are in the minority (No, the cosmic background effect doesn't count, that's not really emitting light).
            Color standardAmbientLightColor = new Color(12, 29, 48);
            if (NamelessDeityFormPresetRegistry.UsingLucillePreset)
                standardAmbientLightColor = new(49, 10, 10);

            // Naturally, as the stars recede everything should go towards a neutral dark. There's a small amount of ambient lighting present for gameplay visibility reasons, but that's pretty much it.
            Color darkAmbientLightColor = new Color(4, 4, 4);

            // Lastly, once Nameless' eye appears everything should receive a reddish orange tint over everything, since that's the color of the eye.
            Color eyeAmbientLightColor = new Color(26, 11, 9);

            // Interpolate the ambient colors based on the current situation.
            Color ambientLightColor = Color.Lerp(standardAmbientLightColor, darkAmbientLightColor, NamelessDeitySky.StarRecedeInterpolant);
            ambientLightColor = Color.Lerp(ambientLightColor, eyeAmbientLightColor, Pow(NamelessDeitySky.SkyEyeScale * NamelessDeitySky.SkyEyeOpacity, 1.5f));

            return ambientLightColor;
        }
    }

    /// <summary>
    /// The ambient color of the background lake.
    /// </summary>
    public static Color LakeAmbientColor
    {
        get
        {
            // Same idea as BackgroundAmbientColor for the most part.
            Color standardAmbientLightColor = Color.SkyBlue;
            Color darkAmbientLightColor = new Color(34, 34, 34);
            Color eyeAmbientLightColor = new Color(233, 194, 201);

            if (NamelessDeityFormPresetRegistry.UsingLucillePreset)
                standardAmbientLightColor = new(235, 105, 156);

            // Interpolate the ambient colors based on the current situation.
            Color ambientLightColor = Color.Lerp(standardAmbientLightColor, darkAmbientLightColor, NamelessDeitySky.StarRecedeInterpolant);
            ambientLightColor = Color.Lerp(ambientLightColor, eyeAmbientLightColor, Pow(NamelessDeitySky.SkyEyeScale * NamelessDeitySky.SkyEyeOpacity, 1.5f));

            return ambientLightColor;
        }
    }

    /// <summary>
    /// The optimized render target that contains render information of the parts of the sky that should be reflected.
    /// </summary>
    public static DownscaleOptimizedScreenTarget ReflectionTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The optimized render target that contains render information about the aurora.
    /// </summary>
    public static DownscaleOptimizedScreenTarget AuroraTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The render target that contains render information about the lake.
    /// </summary>
    public static InstancedRequestableTarget LakeTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The set of all background frame textures.
    /// </summary>
    public static LazyAsset<Texture2D>[] BackgroundFrameTextures
    {
        get;
        private set;
    }

    /// <summary>
    /// The set of all lake frame textures.
    /// </summary>
    public static LazyAsset<Texture2D>[] LakeFrameTextures
    {
        get;
        private set;
    }

    public override float CloudOpacity => InverseLerp(1f, 0f, Opacity);

    public override float Priority => 1f;

    protected override Background CreateTemplateEntity() => new EternalGardenSky();

    public override void SetStaticDefaults()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        BackgroundFrameTextures = new LazyAsset<Texture2D>[4];
        LakeFrameTextures = new LazyAsset<Texture2D>[4];
        for (int i = 0; i < BackgroundFrameTextures.Length; i++)
        {
            BackgroundFrameTextures[i] = LazyAsset<Texture2D>.FromPath($"Terraria/Images/Background_{i + 251}");
            LakeFrameTextures[i] = LazyAsset<Texture2D>.FromPath(GetAssetPath("Skies/EternalGarden", $"GardenLake{i + 1}"));
        }

        Main.QueueMainThreadAction(() =>
        {
            ReflectionTarget = new DownscaleOptimizedScreenTarget(0.8f, PrepareBackgroundTarget);
            AuroraTarget = new DownscaleOptimizedScreenTarget(0.64f, PrepareAuroraTarget);

            Main.ContentThatNeedsRenderTargets.Add(LakeTarget = new InstancedRequestableTarget());
        });

        TerminusStairwaySkyScene.LoadGUIOption();
        GraphicalUniverseImagerOptionManager.RegisterNew(new GraphicalUniverseImagerOption("Mods.NoxusBoss.UI.GraphicalUniverseImager.EternalGardenAurora", true,
            GennedAssets.Textures.GraphicalUniverseImager.ShaderSource_Garden, RenderGUIPortrait, RenderGUIBackground));
    }

    private static void RenderGUIPortrait(GraphicalUniverseImagerSettings settings)
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        Main.spriteBatch.Draw(WhitePixel, new Rectangle(0, 0, Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height), new Color(13, 13, 13));
        RenderAtCameraPosition(1, Vector2.UnitY * -1200f, ViewportSize, -1f, 1f, 1f);
        Main.spriteBatch.End();
    }

    private static void RenderGUIBackground(float minDepth, float maxDepth, GraphicalUniverseImagerSettings settings)
    {
        float opacity = ModContent.GetInstance<GraphicalUniverseImagerSky>().EffectiveIntensity;
        Main.spriteBatch.Draw(WhitePixel, new Rectangle(0, 0, Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height), Color.Black * opacity);

        if (minDepth < 0f && maxDepth > 0f)
        {
            SetSpriteSortMode(SpriteSortMode.Deferred, GetCustomSkyBackgroundMatrix());
            Rectangle area = new Rectangle(0, 0, Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
            area.Y -= 200;

            ReflectionTarget.Render(Color.White * opacity, area, 2);
            SetSpriteSortMode(SpriteSortMode.Deferred);
        }
    }

    private static void PrepareAuroraTarget(int identifier)
    {
        string auroraPaletteKey = NamelessDeityFormPresetRegistry.UsingLucillePreset ? "LucilleAurora" : "Aurora";
        Vector3[] auroraPalette = LocalDataManager.Read<Vector3[]>("Core/World/Subworlds/EternalGardenBackgroundPalettes.json")[auroraPaletteKey];

        ManagedShader auroraShader = ShaderManager.GetShader("NoxusBoss.AuroraShader");
        auroraShader.TrySetParameter("gradient", auroraPalette);
        auroraShader.TrySetParameter("gradientCount", auroraPalette.Length);
        auroraShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.056f);
        auroraShader.TrySetParameter("noiseSelfInfluence", 0.137f);
        auroraShader.TrySetParameter("foreshortening", 9.3f);
        auroraShader.TrySetParameter("samplePointCount", 15f);
        auroraShader.TrySetParameter("detail", 0.2f);
        auroraShader.TrySetParameter("noiseUndulationIntensity", 0.177f);
        auroraShader.TrySetParameter("noiseExponent", 2.1f);
        auroraShader.TrySetParameter("planeOrigin", new Vector3(0.5f, 0.5f, 0f));
        auroraShader.TrySetParameter("planeNormal", Vector3.Normalize(new Vector3(0f, -1f, 0.35f)));
        auroraShader.TrySetParameter("cameraViewExaggeration", new Vector2(0.25f, 1f));
        auroraShader.SetTexture(DendriticNoiseZoomedOut, 1, SamplerState.LinearWrap);
        auroraShader.Apply();

        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea / WhitePixel.Size();
        Main.spriteBatch.Draw(WhitePixel, screenArea * 0.5f, null, Color.White, 0f, WhitePixel.Size() * 0.5f, textureArea, 0, 0f);
    }

    private void PrepareBackgroundTarget(int identifier)
    {
        EternalGardenSkyStarRenderer.Render(InverseLerp(0.67f, 0f, NamelessDeitySky.StarRecedeInterpolant), Matrix.Identity);
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, CullOnlyScreen, null, GetCustomSkyBackgroundMatrix());

        AuroraTarget.Render(Color.White * (1f - NamelessDeitySky.StarRecedeInterpolant).Cubed(), identifier);

        // Draw Nameless' eye if it's present.
        if (NamelessDeitySky.SkyEyeScale >= 0.03f && NamelessDeitySky.SkyEyeOpacity > 0f)
            NamelessDeitySky.ReplaceMoonWithNamelessDeityEye(EyePosition + new Vector2(ViewportSize.X * 0.35f, 132f), Matrix.Identity);
    }

    public static void RenderAtCameraPosition(int identifier, Vector2 cameraPosition, Vector2 backgroundSize, float minDepth, float maxDepth, float opacity)
    {
        Main.spriteBatch.Draw(WhitePixel, new Rectangle(0, 0, (int)backgroundSize.X, (int)backgroundSize.Y), Color.Black * NamelessDeitySky.StarRecedeInterpolant);

        if (minDepth < 0f && maxDepth > 0f)
            ReflectionTarget.Render(Color.White * opacity, identifier);

        int y = (int)(cameraPosition.Y * ScreenParallaxSpeed * 0.5f);
        RenderForestBackground(backgroundSize, y, opacity);
        if (minDepth < 0f && maxDepth > 0f)
        {
            PrepareLakeTarget(identifier, backgroundSize);
            RenderLake(identifier, backgroundSize, y);
        }
    }

    public override void Render(Vector2 backgroundSize, float minDepth, float maxDepth)
    {
        DisableVanillaBackgroundObjects();
        RenderAtCameraPosition(0, Main.screenPosition, new Vector2(Main.screenWidth, Main.screenHeight), minDepth, maxDepth, 1f);
    }

    public static void DisableVanillaBackgroundObjects()
    {
        // Disable ambient sky objects like wyverns and eyes appearing in the background.
        SkyManager.Instance["Ambience"].Deactivate();
        SkyManager.Instance["Slime"].Opacity = -1f;
        SkyManager.Instance["Slime"].Deactivate();
    }

    public static void RenderForestBackground(Vector2 backgroundSize, int y, float opacity)
    {
        int frameOffset = (int)(Main.GameUpdateCount / 10U) % 4;
        Texture2D backgroundTexture = BackgroundFrameTextures[frameOffset].Value;

        // Loop the background horizontally.
        for (int i = -2; i <= 2; i++)
        {
            // Draw the base background.
            Vector2 layerPosition = new Vector2(backgroundSize.X * 0.5f + backgroundTexture.Width * i, backgroundSize.Y - y + ScreenParallaxSpeed * 100f);
            Main.spriteBatch.Draw(backgroundTexture, layerPosition - backgroundTexture.Size() * 0.5f, null, BackgroundAmbientColor * opacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }

    public static void PrepareLakeTarget(int identifier, Vector2 backgroundSize)
    {
        // Prepare the lake target.
        LakeTarget.Request((int)backgroundSize.X, (int)backgroundSize.Y, identifier, () =>
        {
            int frameOffset = (int)(Main.GameUpdateCount / 10U) % 4;
            Texture2D lakeTexture = LakeFrameTextures[frameOffset].Value;

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            for (int i = -2; i <= 2; i++)
            {
                Vector2 layerPosition = new Vector2(backgroundSize.X * 0.5f + lakeTexture.Width * i, backgroundSize.Y * 0.5f);
                Main.spriteBatch.Draw(lakeTexture, layerPosition - lakeTexture.Size() * 0.5f, null, Color.SkyBlue, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
        });
    }

    public static void RenderLake(int identifier, Vector2 backgroundSize, int y)
    {
        if (!LakeTarget.TryGetTarget(identifier, out RenderTarget2D? lakeTarget) || lakeTarget is null)
            return;
        if (!ReflectionTarget.DownscaledTarget.TryGetTarget(identifier, out RenderTarget2D? reflectionTarget) || reflectionTarget is null)
            return;

        SetSpriteSortMode(SpriteSortMode.Immediate);

        float horizontalStretchOffset = 0.5f;
        ManagedShader reflectionShader = ShaderManager.GetShader("NoxusBoss.LakeReflectionShader");
        reflectionShader.TrySetParameter("gaudyBullshitMode", false);
        reflectionShader.TrySetParameter("reflectionOpacityFactor", NamelessDeitySky.StarRecedeInterpolant >= 1f ? 0.5f : Pow(1f - NamelessDeitySky.StarRecedeInterpolant, 12.5f));
        reflectionShader.TrySetParameter("reflectionZoom", new Vector2(0.85f, 1f));
        reflectionShader.TrySetParameter("reflectionParallaxOffset", Vector2.UnitX * -0.03f);
        reflectionShader.TrySetParameter("reflectionXCoordInterpolationStart", 0.5f - horizontalStretchOffset);
        reflectionShader.TrySetParameter("reflectionXCoordInterpolationEnd", 0.5f + horizontalStretchOffset);
        reflectionShader.SetTexture(reflectionTarget, 1);
        reflectionShader.SetTexture(DendriticNoise, 2, SamplerState.AnisotropicWrap);
        reflectionShader.Apply();

        Vector2 lakeDrawPosition = new Vector2(backgroundSize.X * 0.5f, backgroundSize.Y) + Vector2.UnitY * (ScreenParallaxSpeed * 100f - y);
        Main.spriteBatch.Draw(lakeTarget, lakeDrawPosition, null, LakeAmbientColor * 0.24f, 0f, lakeTarget.Size() * 0.5f, 1f, SpriteEffects.None, 0f);

        SetSpriteSortMode(SpriteSortMode.Deferred);
    }
}
