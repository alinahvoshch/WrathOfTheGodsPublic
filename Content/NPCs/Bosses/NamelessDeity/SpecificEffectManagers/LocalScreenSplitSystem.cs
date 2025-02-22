using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;

public class LocalScreenSplitSystem : ModSystem
{
    public static Vector2[] SplitCenters
    {
        get;
        set;
    } = new Vector2[MaxSplitCount];

    public static float[] SplitAngles
    {
        get;
        set;
    } = new float[MaxSplitCount];

    public static int[] SplitTimers
    {
        get;
        set;
    } = new int[MaxSplitCount];

    public static int[] SplitLifetimes
    {
        get;
        set;
    } = new int[MaxSplitCount];

    public static float[] SplitWidths
    {
        get;
        set;
    } = new float[MaxSplitCount];

    public static float[] MaxSplitWidths
    {
        get;
        set;
    } = new float[MaxSplitCount];

    public static float[] SplitSlopes => SplitAngles.Select(Tan).ToArray();

    public static float[] SplitCompletionRatios
    {
        get
        {
            float[] ratios = new float[MaxSplitCount];
            for (int i = 0; i < MaxSplitCount; i++)
            {
                ratios[i] = SplitTimers[i] / (float)SplitLifetimes[i];
                if (!float.IsNormal(ratios[i]))
                    ratios[i] = 0f;
            }

            return ratios;
        }
    }

    public const int MaxSplitCount = 10;

    public override void PostUpdateProjectiles()
    {
        for (int i = 0; i < MaxSplitCount; i++)
        {
            // Increment the Split timer if it's active. Once its reaches its natural maximum the effect ceases.
            if (SplitTimers[i] >= 1)
            {
                SplitTimers[i]++;

                if (SplitTimers[i] >= SplitLifetimes[i])
                    SplitTimers[i] = 0;
            }

            SplitWidths[i] = Convert01To010(SplitCompletionRatios[i]) * MaxSplitWidths[i];
        }

        if (Main.netMode == NetmodeID.Server)
            return;

        bool anyActive = SplitWidths.Any(w => w >= 0.01f);
        ManagedScreenFilter overlayShader = ShaderManager.GetFilter("NoxusBoss.NamelessScreenSplitShader");
        if (anyActive)
        {
            // Calculate the source positions of the split in UV coordinates.
            Vector2[] splitCenters = new Vector2[MaxSplitCount];
            for (int i = 0; i < splitCenters.Length; i++)
                splitCenters[i] = WorldSpaceToScreenUV(SplitCenters[i]);

            float brightness = 0.67f;
            float zoom = 1.75f;
            Texture2D texture = GennedAssets.Textures.Extra.Iridescence;
            if (NamelessDeityBoss.Myself_CurrentState == NamelessDeityBoss.NamelessAIType.VergilScreenSlices)
            {
                brightness = 1.8f;
                zoom = 0.4f;
                texture = GennedAssets.Textures.Extra.DivineLight;
            }

            overlayShader.TrySetParameter("splitCenters", splitCenters);
            overlayShader.TrySetParameter("splitDirections", SplitAngles.Select(a => a.ToRotationVector2().RotatedBy(PiOver2)).ToArray());
            overlayShader.TrySetParameter("splitWidths", SplitWidths.Select(a => a / Main.screenWidth).ToArray());
            overlayShader.TrySetParameter("splitSlopes", SplitSlopes);
            overlayShader.TrySetParameter("activeSplits", SplitCompletionRatios.Select(a => a > 0f && a < 1f).ToArray());
            overlayShader.TrySetParameter("offsetsAreAllowed", GetFromCalamityConfig("ScreenshakePower", 1f) >= 0.01f);
            overlayShader.TrySetParameter("splitBrightnessFactor", brightness);
            overlayShader.TrySetParameter("splitTextureZoomFactor", zoom);
            overlayShader.TrySetParameter("opacity", 1f);
            overlayShader.SetTexture(texture, 1, SamplerState.LinearWrap);
            overlayShader.SetTexture(PerlinNoise, 2, SamplerState.LinearWrap);
            overlayShader.Activate();
        }
    }

    public static void Start(Vector2 splitCenter, int splitTime, float splitAngle, float splitWidth)
    {
        for (int i = 0; i < MaxSplitCount; i++)
        {
            if (SplitTimers[i] > 0)
                continue;

            SplitCenters[i] = splitCenter;
            SplitTimers[i] = 1;
            SplitLifetimes[i] = splitTime;
            SplitAngles[i] = splitAngle;
            SplitWidths[i] = MaxSplitWidths[i] = splitWidth;
            break;
        }
    }
}
