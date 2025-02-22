using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.Particles;

public class HighDefinitionSmokeParticle : Particle
{
    /// <summary>
    /// The variant of this smoke.
    /// </summary>
    public int Variant;

    /// <summary>
    /// The amount by which the smoke's scale grows each frame.
    /// </summary>
    public float ScaleGrowRate;

    public override int FrameCount => 5;

    public override string AtlasTextureName => "NoxusBoss.HighDefinitionSmoke.png";

    public override BlendState BlendState => BlendState.Additive;

    public HighDefinitionSmokeParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float scaleGrowRate)
    {
        Position = position;
        Velocity = velocity;
        DrawColor = color;
        Scale = Vector2.One * scale;
        Variant = Main.rand.Next(FrameCount);
        Lifetime = lifetime;
        ScaleGrowRate = scaleGrowRate;
    }

    public override void Update()
    {
        Rotation += Velocity.Length() * Velocity.X.NonZeroSign() * 0.004f;
        Velocity.X = Lerp(Velocity.X, Main.windSpeedCurrent * 1.4f, 0.015f);
        Scale += Vector2.One * LifetimeRatio * ScaleGrowRate;

        if (LifetimeRatio >= 0.8f)
            Opacity *= 0.97f;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        float opacity = InverseLerpBump(0f, 0.07f, 0.1f, 1f, LifetimeRatio) * 0.75f;
        int verticalFrame = (int)Round(Lerp(0f, 4f, LifetimeRatio.Squared()));
        int width = 100;
        int height = 100;
        Rectangle frame = new Rectangle(width * Variant, height * verticalFrame, width, height);

        float x = Clamp(frame.X + Texture.Frame.X, Texture.Frame.X, Texture.Frame.X + Texture.Frame.Width - width);
        float y = Clamp(frame.Y + Texture.Frame.Y, Texture.Frame.Y, Texture.Frame.Y + Texture.Frame.Height - height);
        Rectangle frameOnAtlas = new Rectangle((int)x, (int)y, width, height);
        Vector2 origin = frameOnAtlas.Size() * 0.5f;

        spriteBatch.Draw(Texture.Atlas.Texture.Value, Position - Main.screenPosition, frameOnAtlas, DrawColor * opacity, Rotation, origin, Scale * 2f, 0, 0f);
    }
}
