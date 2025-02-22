using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Graphics.Shaders;
using ReLogic.Peripherals.RGB;
using Terraria.GameContent.RGB;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = KeyboardShaderLoader.IDE0051SuppressionReason)]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = KeyboardShaderLoader.IDE0060SuppressionReason)]
public class AvatarPhase1KeyboardShader : ChromaShader
{
    public static readonly ChromaCondition IsActive = new KeyboardShaderLoader.SimpleCondition(player => CommonConditions.Boss.HighestTierBossOrEvent == ModContent.NPCType<AvatarRift>());

    /// <summary>
    /// The palette that this keyboard shader cycles through.
    /// </summary>
    public static readonly Palette KeyboardPalette = new Palette().
        AddColor(new Color(27, 27, 39)).
        AddColor(new Color(62, 35, 50)).
        AddColor(new Color(84, 32, 49)).
        AddColor(new Color(229, 44, 43));

    [RgbProcessor(EffectDetailLevel.Low)]
    private static void ProcessLowDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
    {
        ProcessHighDetail(device, fragment, quality, time);
    }

    [RgbProcessor(EffectDetailLevel.High)]
    private static void ProcessHighDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
    {
        for (int i = 0; i < fragment.Count; i++)
        {
            Vector2 unscaledPosition = fragment.GetCanvasPositionOfIndex(i);
            Vector2 canvasPosition = unscaledPosition * new Vector2(fragment.CanvasSize.Y / fragment.CanvasSize.X, 1f);

            float windMask = InverseLerp(0.4f, 0.75f, NoiseHelper.GetStaticNoise(canvasPosition * new Vector2(1f, 4f) + Vector2.UnitX * time * 0.2f));
            float hue = NoiseHelper.GetStaticNoise(canvasPosition + new Vector2(time * 0.1f, windMask * 0.05f)).Squared() * windMask;

            fragment.SetColor(i, Vector4.Clamp(KeyboardPalette.SampleVector(hue * 0.97f), Vector4.Zero, Vector4.One));
        }
    }
}
