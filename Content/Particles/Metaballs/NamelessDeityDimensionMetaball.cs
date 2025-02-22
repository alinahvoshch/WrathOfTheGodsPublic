using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using Terraria;
using Terraria.GameContent;

namespace NoxusBoss.Content.Particles.Metaballs;

public class NamelessDeityDimensionMetaball : MetaballType
{
    public override string MetaballAtlasTextureToUse => "NoxusBoss.BasicMetaballCircle.png";

    public override bool ShouldRender => ActiveParticleCount >= 1;

    public override Color EdgeColor => Color.White;

    public override Func<Texture2D>[] LayerTextures => [() => Main.gameMenu ? TextureAssets.MagicPixel.Value : NamelessDeitySkyTargetManager.SkyTarget];

    public override bool LayerIsFixedToScreen(int layerIndex) => true;

    public override void UpdateParticle(MetaballInstance particle)
    {
        particle.Velocity *= 0.984f;
        particle.Size *= 0.89f;
    }

    public override bool ShouldKillParticle(MetaballInstance particle) => particle.Size <= 2f;
}
