using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Particles.Metaballs;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.SpecificEffectManagers;

[Autoload(Side = ModSide.Client)]
public class TileDistortionSystem : ModSystem
{
    /// <summary>
    /// Whether the distortion effect is currently in use or not.
    /// </summary>
    public static bool EffectInUse => ModContent.GetInstance<TileDistortionMetaball>().ActiveParticleCount >= 1;

    public override void PostUpdateEverything()
    {
        ManagedScreenFilter distortionShader = ShaderManager.GetFilter("NoxusBoss.TileDistortionShader");

        if (!distortionShader.IsActive && EffectInUse)
            ApplyDistortionParameters(distortionShader);
    }

    private static void ApplyDistortionParameters(ManagedScreenFilter distortionShader)
    {
        distortionShader.TrySetParameter("maxDistortionOffset", 0.026f);
        distortionShader.TrySetParameter("projection", Matrix.Invert(Main.GameViewMatrix.TransformationMatrix));
        distortionShader.SetTexture(ModContent.GetInstance<TileDistortionMetaball>().LayerTargets[0], 1, SamplerState.LinearClamp);
        distortionShader.SetTexture(TileTargetManagers.TileTarget, 2, SamplerState.PointWrap);
        distortionShader.Activate();
    }
}
