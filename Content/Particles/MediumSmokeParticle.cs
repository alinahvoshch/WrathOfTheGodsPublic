using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.Particles;

public class MediumSmokeParticle : Particle
{
    /// <summary>
    /// The variant of this smoke.
    /// </summary>
    public int Variant;

    /// <summary>
    /// The amount by which the smoke's scale grows each frame.
    /// </summary>
    public float ScaleGrowRate;

    /// <summary>
    /// The rotation offset of this smoke.
    /// </summary>
    public float RotationOffset;

    public override int FrameCount => 8;

    public override string AtlasTextureName => "NoxusBoss.MediumSmokeParticle.png";

    public override BlendState BlendState => BlendState.NonPremultiplied;

    public MediumSmokeParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float scaleGrowRate)
    {
        Position = position;
        Velocity = velocity;
        DrawColor = color;
        Scale = Vector2.One * scale;
        Variant = Main.rand.Next(FrameCount);
        Lifetime = lifetime;
        ScaleGrowRate = scaleGrowRate;
        RotationOffset = -PiOver2 + Main.rand.NextFloatDirection() * 0.51f;
    }

    public override void Update()
    {
        Rotation = Velocity.ToRotation() + RotationOffset;
        Velocity *= 0.89f;
        Scale += Vector2.One * LifetimeRatio * ScaleGrowRate;

        int area = (int)(Scale.X * 50f);
        if (Collision.SolidCollision(Position - Vector2.One * area * 0.5f, area, area))
        {
            Time += 9;
            Velocity *= 0.75f;
        }

        DrawColor = Color.Lerp(DrawColor, Color.White, 0.055f);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        float opacity = InverseLerpBump(0f, 0.02f, 0.4f, 1f, LifetimeRatio) * 0.75f;
        int horizontalFrame = (int)Round(Lerp(0f, 2f, LifetimeRatio));
        int width = 28;
        int height = 28;
        Rectangle frame = new Rectangle(width * horizontalFrame, height * Variant, width, height);

        Vector2 origin = Vector2.Zero;
        float x = Clamp(frame.X + Texture.Frame.X, Texture.Frame.X, Texture.Frame.X + Texture.Frame.Width - width);
        float y = Clamp(frame.Y + Texture.Frame.Y, Texture.Frame.Y, Texture.Frame.Y + Texture.Frame.Height - height);
        Rectangle frameOnAtlas = new Rectangle((int)x, (int)y, (int)width, (int)height);

        spriteBatch.Draw(Texture.Atlas.Texture.Value, Position - Main.screenPosition, frameOnAtlas, DrawColor * opacity, Rotation, origin, Scale * 2f, 0, 0f);
    }
}
