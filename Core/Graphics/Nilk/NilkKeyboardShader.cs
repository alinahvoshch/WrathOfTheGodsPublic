using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Buffs;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Graphics.Shaders;
using ReLogic.Peripherals.RGB;
using Terraria;
using Terraria.GameContent.RGB;

namespace NoxusBoss.Core.Graphics.Nilk;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = KeyboardShaderLoader.IDE0051SuppressionReason)]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = KeyboardShaderLoader.IDE0060SuppressionReason)]
public class NilkKeyboardShader : ChromaShader
{
    public static readonly ChromaCondition IsActive = new KeyboardShaderLoader.SimpleCondition(player => player.HasBuff<NilkDebuff>());

    public override void Update(float elapsedTime) { }

    [RgbProcessor(EffectDetailLevel.Low)]
    private static void ProcessLowDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time) =>
        ProcessHighDetail(device, fragment, quality, time);

    [RgbProcessor(EffectDetailLevel.High)]
    private static void ProcessHighDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
    {
        Palette nilkPalette = new Palette(Color.Red, Color.White, Color.Cyan);
        if (NilkEffectManager.CurrentPalette is not null)
            nilkPalette = new Palette(NilkEffectManager.CurrentPalette);

        for (int i = 0; i < fragment.Count; i++)
        {
            Vector2 uvPositionOfIndex = fragment.GetCanvasPositionOfIndex(i) / fragment.CanvasSize;
            float paletteNoise = NoiseHelper.GetDynamicNoise(uvPositionOfIndex * 0.2f, time * 0.25f);
            if (!float.IsNormal(paletteNoise))
            {
                paletteNoise = 0f;
                uvPositionOfIndex = Vector2.UnitY;
            }

            float angleFromCenter = (uvPositionOfIndex - Vector2.One * 0.5f).ToRotation();
            Vector4 gridColor = nilkPalette.SampleVector(Sin01(TwoPi * paletteNoise + angleFromCenter));
            gridColor = Vector4.Lerp(new(0f, 0f, 0f, 1f), gridColor, NilkEffectManager.NilkInsanityInterpolant);

            fragment.SetColor(i, gridColor);
        }
    }
}
