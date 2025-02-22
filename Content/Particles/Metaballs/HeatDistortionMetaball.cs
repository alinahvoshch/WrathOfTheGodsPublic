using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.Particles.Metaballs;

public class HeatDistortionMetaball : MetaballType
{
    public override string MetaballAtlasTextureToUse => "NoxusBoss.StrongBloom.png";

    public override Color EdgeColor => Color.Transparent;

    public override bool ShouldRender => ActiveParticleCount >= 1;

    public override bool DrawnManually => true;

    public override Func<Texture2D>[] LayerTextures => [() => WhitePixel];

    public override void UpdateParticle(MetaballInstance particle)
    {
        particle.ExtraInfo[0]++;
        particle.Velocity *= 0.942f;
        if (particle.ExtraInfo[0] <= 12f)
            return;

        particle.Size *= 0.97f;
    }

    public override bool PerformCustomSpritebatchBegin(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        return true;
    }

    public override bool ShouldKillParticle(MetaballInstance particle) => particle.Size <= 2f;
}
