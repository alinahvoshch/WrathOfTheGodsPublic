using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Graphics.Shaders;
using ReLogic.Peripherals.RGB;
using Terraria;
using Terraria.GameContent.RGB;
using Terraria.ModLoader;
using static NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = KeyboardShaderLoader.IDE0051SuppressionReason)]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = KeyboardShaderLoader.IDE0060SuppressionReason)]
public class AvatarPhase2KeyboardShader : ChromaShader
{
    /// <summary>
    /// The brightness intensity. As this value approaches 1, the keyboard colors approach white.
    /// </summary>
    public static float KeyboardBrightnessIntensity
    {
        get;
        set;
    }

    /// <summary>
    /// The palette that this keyboard shader cycles through during the standard second phase.
    /// </summary>
    public static readonly Palette Phase2Palette = new Palette().
        AddColor(new Color(0, 0, 0)).
        AddColor(new Color(8, 9, 12)).
        AddColor(new Color(74, 1, 8)).
        AddColor(new Color(144, 4, 19)).
        AddColor(new Color(255, 16, 60));

    public static readonly ChromaCondition IsActive = new KeyboardShaderLoader.SimpleCondition(player => CommonConditions.Boss.HighestTierBossOrEvent == ModContent.NPCType<AvatarOfEmptiness>());

    public override void Update(float elapsedTime)
    {
        KeyboardBrightnessIntensity = Saturate(KeyboardBrightnessIntensity * 0.96f - 0.005f);
    }

    [RgbProcessor(EffectDetailLevel.Low)]
    private static void ProcessLowDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
    {
        ProcessHighDetail(device, fragment, quality, time);
    }

    [RgbProcessor(EffectDetailLevel.High)]
    private static void ProcessHighDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
    {
        bool paradiseReclaimedOngoing = Myself?.As<AvatarOfEmptiness>()?.ParadiseReclaimedIsOngoing ?? false;
        int universalAnnihilationTimer = 0;
        if (Myself is not null && Myself.As<AvatarOfEmptiness>().CurrentState == AvatarAIType.UniversalAnnihilation)
        {
            int annihilationSphereSummonDelay = UniversalAnnihilation_StarChargeUpTime + UniversalAnnihilation_StarExplodeDelay + UniversalAnnihilation_AnnihilationSphereAppearDelay;
            universalAnnihilationTimer = Myself.As<AvatarOfEmptiness>().AITimer - annihilationSphereSummonDelay;
        }

        for (int i = 0; i < fragment.Count; i++)
        {
            Vector2 unscaledPosition = fragment.GetCanvasPositionOfIndex(i);
            Vector2 canvasPosition = unscaledPosition * new Vector2(fragment.CanvasSize.Y / fragment.CanvasSize.X, 1f);
            Vector4 color;

            if (paradiseReclaimedOngoing)
                color = CalculateParadiseReclaimedBackgroundColor(canvasPosition);
            else if (universalAnnihilationTimer >= 1)
            {
                float annihilationInterpolant = InverseLerp(0f, 240f, universalAnnihilationTimer);
                color = CalculateUniversalAnnihilationBackgroundColor(unscaledPosition, fragment.CanvasSize * 0.5f, annihilationInterpolant);
            }
            else if (AvatarOfEmptinessSky.Dimension == AvatarDimensionVariants.DarkDimension)
                color = Vector4.Zero;
            else if (AvatarOfEmptinessSky.Dimension == AvatarDimensionVariants.AntishadowDimension)
                color = CalculateAntishadowBackgroundColor();
            else if (AvatarOfEmptinessSky.Dimension == AvatarDimensionVariants.CryonicDimension)
                color = CalculateCryonicDimensionBackgroundColor(canvasPosition);
            else if (AvatarOfEmptinessSky.Dimension == AvatarDimensionVariants.VisceralDimension)
                color = CalculateVisceralDimensionBackgroundColor(canvasPosition);
            else
                color = CalculatePhase2BackgroundColor(canvasPosition);

            color = Vector4.Lerp(color, Vector4.One, KeyboardBrightnessIntensity);
            fragment.SetColor(i, Vector4.Clamp(color, Vector4.Zero, Vector4.One));
        }
    }

    /// <summary>
    /// Calculates the color of the Avatar's paradise reclaimed background.
    /// </summary>
    /// <param name="position">The position of the key.</param>
    private static Vector4 CalculateParadiseReclaimedBackgroundColor(Vector2 position)
    {
        return Vector4.One * SmoothStep(0f, 1f, NoiseHelper.GetDynamicNoise(position * 3f, Main.GlobalTimeWrappedHourly * 3.5f));
    }

    /// <summary>
    /// Calculates the color of the Avatar's phase 3 universal annihilation background.
    /// </summary>
    /// <param name="position">The position of the key.</param>
    private static Vector4 CalculateUniversalAnnihilationBackgroundColor(Vector2 position, Vector2 center, float animationCompletion)
    {
        float distanceFromCenter = position.Distance(center);
        float edgeRadius = InverseLerp(0f, 0.4f, animationCompletion) * 0.45f;
        float distanceFromEdge = Distance(distanceFromCenter, edgeRadius);

        float consumptionInterpolant = InverseLerp(0.3f, 0.9f, animationCompletion).Squared();
        float opacity = InverseLerp(0.1f + consumptionInterpolant, 0.05f, distanceFromEdge);

        float colorInterpolant = NoiseHelper.GetDynamicNoise(position, Main.GlobalTimeWrappedHourly * 3f);

        return Color.Lerp(Color.DeepSkyBlue, Color.White, colorInterpolant + consumptionInterpolant).ToVector4() * opacity;
    }

    /// <summary>
    /// Calculates the color of the Avatar's phase 3 antishadow onslaught background.
    /// </summary>
    /// <param name="position">The position of the key.</param>
    private static Vector4 CalculateAntishadowBackgroundColor()
    {
        return new(1f, 0f, 0.2f, 1f);
    }

    /// <summary>
    /// Calculates the color of the Avatar's phase 3 cryonic dimension background.
    /// </summary>
    /// <param name="position">The position of the key.</param>
    private static Vector4 CalculateCryonicDimensionBackgroundColor(Vector2 position)
    {
        float bump = Convert01To010(position.X) + 0.2f;
        float bumpTop = bump * 0.2f;
        float bumpBottom = bump * 0.47f;
        float topFadeOut = InverseLerp(bumpTop, bumpBottom, position.Y);
        float bottomFadeOut = InverseLerp(bumpTop, bumpBottom, 1f - position.Y);

        float arcCurvature = 9f;
        float windDirection = SignedPow(position.Y - 0.5f, 0.75f);
        Vector2 scrollOffset = new Vector2(AvatarOfEmptinessSky.WindTimer, Pow(position.X.Modulo(1f) - 0.5f, 2f) * windDirection * -arcCurvature);
        Vector2 curvedWindCoords = (position * 1.2f + scrollOffset) * new Vector2(0.4f, 1f);
        float opacity = (NoiseHelper.GetStaticNoise(curvedWindCoords) * 1.2f).Squared() * topFadeOut * bottomFadeOut;

        Color background = new Color(17, 16, 20);

        return background.ToVector4() + Color.DeepSkyBlue.ToVector4() * opacity;
    }

    /// <summary>
    /// Calculates the color of the Avatar's phase 3 visceral dimension background.
    /// </summary>
    /// <param name="position">The position of the key.</param>
    private static Vector4 CalculateVisceralDimensionBackgroundColor(Vector2 position)
    {
        Vector2 polar = new Vector2(position.AngleFrom(Vector2.One * 0.5f) / 3.141f + 0.5f, position.Distance(Vector2.One * 0.5f));
        polar.X += Sin(TwoPi * polar.Y * 2f) * 0.54f;

        float noise = NoiseHelper.GetStaticNoise(polar + Vector2.UnitX * Main.GlobalTimeWrappedHourly);
        float distanceFadeout = InverseLerp(0.45f, 0.22f, polar.Y);

        return Color.Lerp(new(54, 17, 20), new(255, 0, 0), noise).ToVector4() * distanceFadeout;
    }

    /// <summary>
    /// Calculates the color of the Avatar's phase 2 screen shader background.
    /// </summary>
    /// <param name="position">The position of the key.</param>
    private static Vector4 CalculatePhase2BackgroundColor(Vector2 position)
    {
        float arcCurvature = 8f;
        float windDirection = SignedPow(position.Y - 0.5f, 0.75f);
        Vector2 scrollOffset = new Vector2(AvatarOfEmptinessSky.WindTimer, Pow(position.X.Modulo(1f) - 0.5f, 2f) * windDirection * -arcCurvature);
        Vector2 curvedWindCoords = (position * 3.1f + scrollOffset) * new Vector2(0.2f, 0.6f);
        float hue = InverseLerp(0.03f, 1f, Pow(NoiseHelper.GetStaticNoise(curvedWindCoords) + 0.001f, 2.5f));

        if (float.IsNaN(hue) || float.IsInfinity(hue) || hue > 1f || hue < 0f)
            hue = 0f;

        Color background = new Color(7, 3, 13);
        Vector4 wind = Phase2Palette.SampleVector(hue * 0.98f);

        return background.ToVector4() + wind;
    }

    public static float SignedPow(float x, float n)
    {
        return Pow(Abs(x) + 0.00002f, n) * Sign(x);
    }
}
