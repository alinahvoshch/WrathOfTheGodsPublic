using Luminance.Core.Graphics;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Particles.Metaballs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.HeatDistortion;

public class MetaballHeatDistortionSystem : ModSystem
{
    public override void PostUpdateEverything()
    {
        if (Main.netMode == NetmodeID.Server || !ModContent.GetInstance<HeatDistortionMetaball>().ShouldRender)
            return;

        Texture2D heatDistortionMap = ModContent.GetInstance<HeatDistortionMetaball>().LayerTargets[0];
        ManagedScreenFilter distortionShader = ShaderManager.GetFilter("NoxusBoss.HeatDistortionShader");
        distortionShader.TrySetParameter("maxDistortionOffset", 0.0023f);
        distortionShader.SetTexture(heatDistortionMap, 1, SamplerState.LinearWrap);
        distortionShader.SetTexture(PerlinNoise, 2, SamplerState.LinearWrap);
        distortionShader.Activate();
    }
}
