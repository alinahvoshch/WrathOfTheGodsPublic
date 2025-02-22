using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.Graphics.Shaders;
using ReLogic.Peripherals.RGB;
using Terraria;

namespace NoxusBoss.Core.World.GameScenes.TerminusStairway;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = KeyboardShaderLoader.IDE0051SuppressionReason)]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = KeyboardShaderLoader.IDE0060SuppressionReason)]
public class TerminusStairSkyKeyboardShader : ChromaShader
{
    public static readonly ChromaCondition IsActive = new KeyboardShaderLoader.SimpleCondition(player => TerminusStairwaySystem.Enabled);

    public override void Update(float elapsedTime)
    {

    }

    [RgbProcessor(EffectDetailLevel.Low)]
    private static void ProcessLowDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time) =>
        ProcessHighDetail(device, fragment, quality, time);

    [RgbProcessor(EffectDetailLevel.High)]
    private static void ProcessHighDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
    {
        TerminusStairwaySky.CalculateGradientInterpolants(TerminusStairwaySystem.WalkAnimationInterpolant, out float brightenInterpolant, out float spaceInterpolant);
        TerminusStairwaySky.GetGradientColors(brightenInterpolant, spaceInterpolant, out Color top, out Color bottom);

        Vector4 topVector = top.ToVector4();
        Vector4 bottomVector = bottom.ToVector4();

        for (int i = 0; i < fragment.Count; i++)
        {
            Vector2 uvPositionOfIndex = fragment.GetCanvasPositionOfIndex(i) / fragment.CanvasSize;
            Vector4 gridColor = Vector4.Lerp(topVector, bottomVector, Pow(uvPositionOfIndex.Y, 0.51f));

            float stairDistance = Abs(SignedDistanceToLine(uvPositionOfIndex, Vector2.One * 0.5f, 0.67f.ToRotationVector2()));
            Vector4 stairColor = Main.hslToRgb((uvPositionOfIndex.X + Main.GlobalTimeWrappedHourly * 0.25f).Modulo(1f), 1f, 0.9f).ToVector4();
            gridColor = Vector4.Lerp(gridColor, stairColor, InverseLerp(0.06f, 0.03f, stairDistance));

            fragment.SetColor(i, gridColor);
        }
    }
}
