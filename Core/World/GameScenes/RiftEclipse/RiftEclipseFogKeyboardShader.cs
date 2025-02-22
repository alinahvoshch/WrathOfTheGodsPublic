using System.Diagnostics.CodeAnalysis;
using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.Graphics.Shaders;
using ReLogic.Peripherals.RGB;
using Terraria;
using Terraria.GameContent.RGB;

namespace NoxusBoss.Core.World.GameScenes.RiftEclipse;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = KeyboardShaderLoader.IDE0051SuppressionReason)]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = KeyboardShaderLoader.IDE0060SuppressionReason)]
public class RiftEclipseFogKeyboardShader : ChromaShader
{
    public static readonly ChromaCondition IsActive = new KeyboardShaderLoader.SimpleCondition(player => RiftEclipseFogEventManager.EventOngoing && player.position.Y <= Main.worldSurface * 16f);

    public override void Update(float elapsedTime) { }

    [RgbProcessor(EffectDetailLevel.Low)]
    private static void ProcessLowDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time) =>
        ProcessHighDetail(device, fragment, quality, time);

    [RgbProcessor(EffectDetailLevel.High)]
    private static void ProcessHighDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
    {
        for (int i = 0; i < fragment.Count; i++)
        {
            Vector2 uvPositionOfIndex = fragment.GetCanvasPositionOfIndex(i) / fragment.CanvasSize + Vector2.UnitX * time * 0.06f;
            float noiseInterpolant = NoiseHelper.GetDynamicNoise(uvPositionOfIndex, time * 0.1f);
            noiseInterpolant += NoiseHelper.GetDynamicNoise(uvPositionOfIndex * 0.5f, time * 0.83f) * 0.3f;
            noiseInterpolant = EasingCurves.Cubic.Evaluate(EasingType.InOut, Saturate(noiseInterpolant));

            Vector4 gridColor = Vector4.Lerp(new(61f, 62f, 86f, 1f), new(0f, 0f, 0f, 1f), noiseInterpolant) / 255f;

            float redInterpolant = Pow(NoiseHelper.GetStaticNoise(uvPositionOfIndex * 0.5f + Vector2.UnitX * time * 0.02f), 13f);
            gridColor = Vector4.Lerp(gridColor, new(1f, 0f, 0.24f, 1f), redInterpolant);

            float cyanInterpolant = Pow(NoiseHelper.GetStaticNoise(uvPositionOfIndex * 0.5f + Vector2.UnitX * time * 0.023f + Vector2.UnitY * 0.57f), 13f);
            gridColor = Vector4.Lerp(gridColor, new(0f, 0.8f, 1f, 1f), cyanInterpolant);

            gridColor = Vector4.Lerp(Vector4.Zero, gridColor, RiftEclipseFogEventManager.FogDrawIntensity);

            fragment.SetColor(i, gridColor);
        }
    }
}
