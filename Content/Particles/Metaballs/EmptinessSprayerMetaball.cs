using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers.ParadiseReclaimed;

namespace NoxusBoss.Content.Particles.Metaballs;

public class EmptinessSprayerMetaball : MetaballType
{
    public override string MetaballAtlasTextureToUse => "NoxusBoss.BasicMetaballCircle.png";

    public override Color EdgeColor => Color.Transparent;

    public override bool ShouldRender => ActiveParticleCount >= 1;

    public override Func<Texture2D>[] LayerTextures => [() => ParadiseStaticTargetSystem.StaticTarget ?? InvisiblePixel.Value];

    public override void UpdateParticle(MetaballInstance particle)
    {
        particle.Velocity *= 0.99f;
        particle.Size *= 0.92f;
    }

    public override bool ShouldKillParticle(MetaballInstance particle) => particle.Size <= 4f;
}
