using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.Particles;

public class BloomPixelParticle : Particle
{
    /// <summary>
    /// The color of bloom behind the pixel.
    /// </summary>
    public Color BloomColor;

    /// <summary>
    /// The scale factor of the back-bloom.
    /// </summary>
    public Vector2 BloomScaleFactor;

    /// <summary>
    /// The bloom texture.
    /// </summary>
    public static AtlasTexture BloomTexture
    {
        get;
        private set;
    }

    public override string AtlasTextureName => "NoxusBoss.Pixel.png";

    public override BlendState BlendState => BlendState.Additive;

    public BloomPixelParticle(Vector2 position, Vector2 velocity, Color color, Color bloomColor, int lifetime, Vector2 scale, Vector2? bloomScaleFactor = null)
    {
        Position = position;
        Velocity = velocity;
        DrawColor = color;
        BloomColor = bloomColor;
        Scale = scale;
        Lifetime = lifetime;
        Opacity = 1f;
        Rotation = Main.rand.NextFloat(TwoPi);
        BloomScaleFactor = bloomScaleFactor ?? Vector2.One * 0.19f;
    }

    public override void Update()
    {
        if (Time >= Lifetime * 0.8f)
            Opacity *= 0.91f;

        Scale *= 0.967f;
        Velocity.X += Main.windSpeedCurrent * 0.051f;
        Velocity.Y *= 0.983f;
        Rotation = Velocity.ToRotation();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        BloomTexture ??= AtlasManager.GetTexture("NoxusBoss.BasicMetaballCircle.png");
        spriteBatch.Draw(BloomTexture, Position - Main.screenPosition, null, BloomColor * Opacity, Rotation, null, Scale * BloomScaleFactor, 0);
        base.Draw(spriteBatch);
    }
}
