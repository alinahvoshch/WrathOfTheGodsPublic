using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Particles.Metaballs;

public class DarkGasMetaball : MetaballType
{
    public override string MetaballAtlasTextureToUse => "NoxusBoss.BasicMetaballCircle.png";

    public override Color EdgeColor => new(255, 8, 36);

    public override bool ShouldRender => ActiveParticleCount >= 1;

    public override bool DrawnManually => true;

    public override Func<Texture2D>[] LayerTextures => [() => GennedAssets.Textures.Particles.PitchBlackLayer];

    public override void Load()
    {
        On_Main.DrawProjectiles += DrawGasMetaballs;
        base.Load();
    }

    private static void DrawGasMetaballs(On_Main.orig_DrawProjectiles orig, Main self)
    {
        DarkGasMetaball metaball = ModContent.GetInstance<DarkGasMetaball>();
        if (metaball.ShouldRender)
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            metaball.RenderLayerWithShader();
            Main.spriteBatch.End();
        }

        orig(self);
    }

    public override void UpdateParticle(MetaballInstance particle)
    {
        particle.Velocity *= 0.99f;
        particle.Velocity = Collision.TileCollision(particle.Center, particle.Velocity, 1, 1);
        if (particle.Velocity.Y == 0f)
        {
            particle.Velocity.X *= 0.5f;
            particle.Size *= 0.93f;
        }

        particle.Size *= 0.93f;
    }

    public override bool ShouldKillParticle(MetaballInstance particle) => particle.Size <= 2f;
}
