using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.Particles;

public class PartyHatParticle : Particle
{
    public int FrameY;

    public override int FrameCount => 4;

    public override string AtlasTextureName => "NoxusBoss.PartyHatParticle.png";

    public PartyHatParticle(Vector2 position, int lifetime, int direction)
    {
        Position = position;
        Lifetime = lifetime;
        Direction = direction;
        Scale = Vector2.One;
    }

    public override void Update()
    {
        // Fade in and out based on the lifetime of the hat.
        Opacity = InverseLerp(1f, 0.67f, LifetimeRatio);

        // Fall down after a cartoonish delay.
        if (Time >= 60)
            Velocity.Y += 0.3f;

        // Collide with tiles.
        Velocity = Collision.TileCollision(Position - new Vector2(17f, 28f), Velocity, 34, 24);

        // Crumple on the ground.
        if (Velocity.Y == 0f && Time % 4 == 3 && Time >= 60)
            FrameY = Utils.Clamp(FrameY + 1, 0, FrameCount - 1);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Color color = Lighting.GetColor(Position.ToTileCoordinates());
        Rectangle frame = Texture.Frame.Subdivide(1, FrameCount, 0, FrameY);
        frame.Y += frame.Y % 2;

        SpriteEffects visualDirection = Direction.ToSpriteDirection();
        spriteBatch.Draw(Texture, Position - Main.screenPosition - Vector2.UnitY * Scale * 12f, frame, color * Opacity, Rotation, null, Scale, visualDirection);
    }
}
