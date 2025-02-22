using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.MainMenuThemes;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Core;
using NoxusBoss.Core.CrossCompatibility.Inbound.RealisticSky;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;

public class AvatarRiftSky : CustomSky
{
    private bool isActive;

    private static readonly float[] lightningFlashLifetimeRatios = new float[MaxLightningFlashes];

    private static readonly float[] lightningFlashIntensities = new float[MaxLightningFlashes];

    private static readonly Vector2[] lightningFlashPositions = new Vector2[MaxLightningFlashes];

    internal static float intensity;

    public static int LightningSpawnCountdown
    {
        get;
        set;
    }

    public const int MaxLightningFlashes = 15;

    /// <summary>
    /// The palette that is used for gradient mapping with this sky.
    /// </summary>
    public static readonly Vector4[] BackgroundPalette =
    [
        new Color(27, 27, 39).ToVector4(),
        new Color(62, 35, 50).ToVector4(),
        new Color(84, 32, 49).ToVector4(),
        new Color(229, 44, 43).ToVector4(),
    ];

    /// <summary>
    /// The render target responsible for holding visual data about the sky.
    /// </summary>
    public static DownscaleOptimizedScreenTarget SkyTarget
    {
        get;
        private set;
    }

    public const string ScreenShaderKey = "NoxusBoss:AvatarRiftSky";

    public override void OnLoad()
    {
        SkyTarget = new(0.5f, RenderSkyToTarget);
    }

    public override void Update(GameTime gameTime)
    {
        if (LightningSpawnCountdown > 0)
        {
            float lightningSpawnChance = InverseLerp(0f, 180f, LightningSpawnCountdown).Squared() * 0.42f;
            if (Main.rand.NextBool(lightningSpawnChance))
                CreateLightningFlash(new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat(0.05f, 0.167f)));

            LightningSpawnCountdown--;
        }

        bool usingMainMenuTheme = Main.gameMenu && MenuLoader.CurrentMenu == ModContent.GetInstance<AvatarRiftSkyMainMenu>();
        if (AvatarRift.Myself is null && !usingMainMenuTheme)
            isActive = false;

        // Make the intensity go up or down based on whether the sky is in use.
        if (!Main.gamePaused)
            intensity = Saturate(intensity + isActive.ToDirectionInt() * 0.183f);
        if (Main.gameMenu)
            intensity = 0f;

        for (int i = 0; i < MaxLightningFlashes; i++)
        {
            ref float lifetimeRatio = ref lightningFlashLifetimeRatios[i];
            if (lifetimeRatio <= 0f)
                continue;

            lifetimeRatio += Pow(lightningFlashIntensities[i], 1.44f) * 0.04f;
            if (lifetimeRatio >= 1f)
                lifetimeRatio = 0f;
        }
    }

    /// <summary>
    /// Creates a new lightning flash with a given UV position in the sky.
    /// </summary>
    public static void CreateLightningFlash(Vector2 lightningPosition)
    {
        for (int i = 0; i < MaxLightningFlashes; i++)
        {
            if (lightningFlashLifetimeRatios[i] <= 0f)
            {
                lightningFlashPositions[i] = lightningPosition;
                lightningFlashLifetimeRatios[i] = 0.001f;
                lightningFlashIntensities[i] = Main.rand.NextFloat(0.4f, 1f);
                break;
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        RealisticSkyCompatibility.SunBloomOpacity = 0f;
        RealisticSkyCompatibility.TemporarilyDisable();

        if (maxDepth < 7.5f || minDepth >= float.MaxValue || ModLoadingChecker.ModsReloading)
            return;

        float effectiveIntensity = intensity;
        bool usingMainMenuTheme = Main.gameMenu && MenuLoader.CurrentMenu == ModContent.GetInstance<AvatarRiftSkyMainMenu>();
        if (usingMainMenuTheme)
            effectiveIntensity = 1f;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin();
        SkyTarget.Render(Color.White * effectiveIntensity);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, GetCustomSkyBackgroundMatrix());
    }

    private static void RenderSkyToTarget(int identifier)
    {
        if (typeof(ShaderManager).GetField("shaders", UniversalBindingFlags)?.GetValue(null) is not Dictionary<string, ManagedShader>)
            return;

        ManagedShader skyShader = ShaderManager.GetShader("NoxusBoss.AvatarPhase1BackgroundShader");
        skyShader.TrySetParameter("gradient", BackgroundPalette);
        skyShader.TrySetParameter("gradientCount", BackgroundPalette.Length);
        skyShader.TrySetParameter("lightningFlashLifetimeRatios", lightningFlashLifetimeRatios.ToArray());
        skyShader.TrySetParameter("lightningFlashPositions", lightningFlashPositions.ToArray());
        skyShader.TrySetParameter("lightningFlashIntensities", lightningFlashIntensities.ToArray());
        skyShader.SetTexture(WavyBlotchNoise, 1, SamplerState.LinearWrap);
        skyShader.SetTexture(WatercolorNoiseA, 2, SamplerState.LinearWrap);
        skyShader.SetTexture(CrackedNoiseA, 3, SamplerState.LinearWrap);
        skyShader.Apply();

        Texture2D texture = WhitePixel.Value;
        Vector2 skySize = ViewportSize;
        Vector2 scale = skySize / texture.Size();
        Main.spriteBatch.Draw(texture, skySize * 0.5f, null, Color.White, 0f, texture.Size() * 0.5f, scale, 0, 0f);
    }

    public override Color OnTileColor(Color inColor)
    {
        ModifySunlightColors(ref Main.ColorOfTheSkies, ref inColor);
        return inColor;
    }

    public static void ModifySunlightColors(ref Color backgroundColor, ref Color tileColor)
    {
        backgroundColor = Color.Lerp(backgroundColor, new(20, 6, 10), intensity);
        tileColor = Color.Lerp(tileColor, new(27, 2, 12), intensity * 0.95f);
    }

    #region Boilerplate
    public override void Activate(Vector2 position, params object[] args)
    {
        isActive = true;
    }

    public override void Deactivate(params object[] args)
    {
        isActive = false;
    }

    public override float GetCloudAlpha() => Main.gameMenu ? 0f : (1f - intensity);

    public override bool IsActive() => isActive || intensity > 0f;

    public override void Reset()
    {
        isActive = false;
    }

    #endregion Boilerplate
}
