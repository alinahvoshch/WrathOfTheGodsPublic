using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;

namespace NoxusBoss.Content.Particles.Metaballs;

public abstract class ColoredDarkGasMetaball : MetaballType
{
    public override string MetaballAtlasTextureToUse => "NoxusBoss.BasicMetaballCircle.png";

    public override bool ShouldRender => ActiveParticleCount >= 1;

    public override Func<Texture2D>[] LayerTextures => new Func<Texture2D>[]
    {
        () => GennedAssets.Textures.Particles.PitchBlackLayer
    };

    public override bool ShouldKillParticle(MetaballInstance particle) => particle.Size <= 2f;

    public override void UpdateParticle(MetaballInstance particle)
    {
        particle.Velocity.Y += Sin(particle.Center.X * 0.0055f + Main.windSpeedCurrent * 250f) * 0.16f - 0.09f;
        particle.Size *= 0.996f;
        particle.Center += particle.Velocity * particle.Size / 17f;
    }
}

public class CyanDarkGasMetaball : ColoredDarkGasMetaball
{
    public override Color EdgeColor => new(70, 229, 247);

    public override Func<Texture2D>[] LayerTextures => new Func<Texture2D>[]
    {
        () => GennedAssets.Textures.Particles.WhiteLayer
    };
}

public class RedDarkGasMetaball : ColoredDarkGasMetaball
{
    public override Color EdgeColor => new(208, 18, 23);

    public override Func<Texture2D>[] LayerTextures => new Func<Texture2D>[]
    {
        () => GennedAssets.Textures.Particles.PitchBlackLayer
    };
}

public class VioletDarkGasMetaball : ColoredDarkGasMetaball
{
    public override Color EdgeColor => new(31, 7, 71);

    public override Func<Texture2D>[] LayerTextures => new Func<Texture2D>[]
    {
        () => GennedAssets.Textures.Particles.PitchBlackLayer
    };
}

public class TanDarkGasMetaball : ColoredDarkGasMetaball
{
    public override Color EdgeColor => new(180, 140, 137);

    public override Func<Texture2D>[] LayerTextures => new Func<Texture2D>[]
    {
        () => GennedAssets.Textures.Particles.WhiteLayer
    };
}
