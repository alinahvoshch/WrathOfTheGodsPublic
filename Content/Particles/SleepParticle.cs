using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.Particles;

public class SleepParticle : Particle
{
    public override BlendState BlendState => BlendState.NonPremultiplied;

    public override string AtlasTextureName => "NoxusBoss.SleepParticle.png";

    public SleepParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifeTime)
    {
        Position = position;
        Velocity = velocity;
        DrawColor = color;
        Scale = Vector2.One * scale;
        Lifetime = lifeTime;
        Direction = 1;
        Rotation = Cos(Main.GlobalTimeWrappedHourly * 2f) * 0.4f;
    }

    public override void Update()
    {
        Opacity = InverseLerp(1f, 0.7f, LifetimeRatio);
        Scale = Vector2.One * Convert01To010(Sqrt(LifetimeRatio) + 0.001f) * 0.33f;
        Velocity.X *= 0.9967f;
        Velocity.Y *= 0.93f;
        Velocity.Y -= Cos01(Position.X * 0.012f) * 0.04f;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Color outlineColor = new Color(Vector3.One - DrawColor.ToVector3());
        outlineColor = Color.Lerp(outlineColor, Color.Purple, outlineColor.G / 255f);
        for (int i = 0; i < 12; i++)
        {
            Vector2 drawOffset = (TwoPi * i / 12f).ToRotationVector2() * 4f;
            spriteBatch.Draw(Texture, Position - Main.screenPosition + drawOffset, Frame, outlineColor * Opacity, Rotation, null, Scale, Direction.ToSpriteDirection());
        }

        spriteBatch.Draw(Texture, Position - Main.screenPosition, Frame, DrawColor * Opacity, Rotation, null, Scale, Direction.ToSpriteDirection());
    }
}
