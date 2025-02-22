using Luminance.Core.Graphics;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles.Metaballs;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.HeatDistortion;

public class ScreenDataHeatDistortionSystem : ModSystem
{
    /// <summary>
    /// The render target that contains all heat distortion data.
    /// </summary>
    public static InstancedRequestableTarget HeatDistortionTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The render target that contains all heat distortion exclusion data.
    /// </summary>
    public static InstancedRequestableTarget HeatExclusionTarget
    {
        get;
        private set;
    }

    public static readonly Queue<DrawData> HeatDistortionData = [];

    public static readonly Queue<DrawData> ExclusionData = [];

    public override void OnModLoad()
    {
        HeatDistortionTarget = new InstancedRequestableTarget();
        HeatExclusionTarget = new InstancedRequestableTarget();

        Main.ContentThatNeedsRenderTargets.Add(HeatDistortionTarget);
        Main.ContentThatNeedsRenderTargets.Add(HeatExclusionTarget);
    }

    public override void PostUpdateEverything()
    {
        if (Main.netMode == NetmodeID.Server || (HeatDistortionData.Count <= 0 && ExclusionData.Count <= 0))
            return;

        HeatDistortionTarget.Request(Main.screenWidth, Main.screenHeight, 0, () =>
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, CullOnlyScreen, null, Main.GameViewMatrix.TransformationMatrix);

            while (HeatDistortionData.TryDequeue(out DrawData data))
                data.Draw(Main.spriteBatch);
            Main.spriteBatch.End();
        });
        HeatExclusionTarget.Request(Main.screenWidth, Main.screenHeight, 0, () =>
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, CullOnlyScreen, null, Main.GameViewMatrix.TransformationMatrix);

            while (ExclusionData.TryDequeue(out DrawData data))
                data.Draw(Main.spriteBatch);
            Main.spriteBatch.End();
        });

        if (!HeatDistortionTarget.TryGetTarget(0, out RenderTarget2D? distortionTarget) || distortionTarget is null)
            return;
        if (!HeatExclusionTarget.TryGetTarget(0, out RenderTarget2D? exclusionTarget) || exclusionTarget is null)
            return;

        Texture2D heatDistortionMap = ModContent.GetInstance<HeatDistortionMetaball>().LayerTargets[0];
        ManagedScreenFilter distortionShader = ShaderManager.GetFilter("NoxusBoss.ScreenDataHeatDistortionShader");
        distortionShader.TrySetParameter("maxDistortionOffset", 1f);
        distortionShader.SetTexture(distortionTarget, 1, SamplerState.LinearWrap);
        distortionShader.SetTexture(exclusionTarget, 2, SamplerState.LinearWrap);
        distortionShader.SetTexture(PerlinNoise, 3, SamplerState.LinearWrap);
        distortionShader.SetTexture(GennedAssets.Textures.Extra.PsychedelicWingTextureOffsetMap, 4, SamplerState.LinearWrap);
        distortionShader.Activate();
    }
}
