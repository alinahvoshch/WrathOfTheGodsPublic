using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.Particles;

public class EmberParticle : Particle
{
    /// <summary>
    /// The base scale of this ember.
    /// </summary>
    public Vector2 BaseScale
    {
        get;
        set;
    }

    public override string AtlasTextureName => "NoxusBoss.Pixel.png";

    public EmberParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifeTime)
    {
        Position = position;
        Velocity = velocity;
        DrawColor = color;
        Scale = Vector2.Zero;
        BaseScale = Vector2.One * scale;
        Lifetime = lifeTime;
        Direction = 1;
    }

    public override void Update()
    {
        Scale = BaseScale * InverseLerpBump(0f, 0.05f, 0.6f, 1f, LifetimeRatio);

        float idealYSpeed = InverseLerp(0.56f, 0f, LifetimeRatio) * -2f + AperiodicSin(Position.X * 0.02f) * 0.3f;
        Velocity.X = Lerp(Velocity.X, Main.windSpeedCurrent * 1.4f, 0.015f);
        Velocity.Y = Lerp(Velocity.Y, idealYSpeed, 0.075f);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        AtlasTexture bloomTexture = AtlasManager.GetTexture("NoxusBoss.StrongBloom.png");

        Color color = Color.Lerp(Color.White, DrawColor, InverseLerp(0.02f, 0.24f, LifetimeRatio) * 0.5f + 0.5f);
        Color backglowColor = Main.hslToRgb(Main.rgbToHsl(color) + new Vector3(LifetimeRatio * -0.1f, 0f, 0.1f));
        backglowColor.A = 0;

        spriteBatch.Draw(bloomTexture, Position - Main.screenPosition, Frame, backglowColor * Opacity * 0.2f, Rotation, null, Scale / 25f, Direction.ToSpriteDirection());
        spriteBatch.Draw(Texture, Position - Main.screenPosition, Frame, color * Opacity, Rotation, null, Scale / 128f, Direction.ToSpriteDirection());
    }
}
