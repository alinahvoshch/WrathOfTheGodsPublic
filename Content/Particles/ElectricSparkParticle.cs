using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.Particles;

public class ElectricSparkParticle : Particle
{
    /// <summary>
    /// The color of bloom behind the pixel.
    /// </summary>
    public Color BloomColor;

    /// <summary>
    /// The bloom texture.
    /// </summary>
    public static AtlasTexture BloomTexture
    {
        get;
        private set;
    }

    public override int FrameCount => 4;

    public override string AtlasTextureName => "NoxusBoss.ElectricSparkParticle.png";

    public override BlendState BlendState => BlendState.Additive;

    public ElectricSparkParticle(Vector2 position, Vector2 velocity, Color color, Color bloomColor, int lifetime, Vector2 scale)
    {
        Position = position;
        Velocity = velocity;
        DrawColor = color;
        BloomColor = bloomColor;
        Scale = scale;
        Lifetime = lifetime;
        Frame = new(0, Main.rand.Next(FrameCount) * 125, 125, 125);
        Opacity = 1f;
        Rotation = Main.rand.NextFloat(TwoPi);
    }

    public override void Update()
    {
        if (Time >= Lifetime - 5)
            Opacity *= 0.84f;

        if (Time == Lifetime / 2 && Frame is not null)
        {
            int currentFrame = Frame.Value.Y / 125;
            int nextFrame = (currentFrame + 1) % FrameCount;
            Frame = new(0, nextFrame * 125, 125, 125);
        }

        Velocity *= 0.92f;
        Scale *= 1.09f;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        BloomTexture ??= AtlasManager.GetTexture("NoxusBoss.StrongBloom.png");
        spriteBatch.Draw(BloomTexture, Position - Main.screenPosition, null, BloomColor * Opacity, 0f, null, Scale * 2f, 0);
        base.Draw(spriteBatch);
    }
}
