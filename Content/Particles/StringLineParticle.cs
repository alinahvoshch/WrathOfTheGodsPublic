using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;

namespace NoxusBoss.Content.Particles;

public class StringLineParticle : Particle
{
    public float Spin;

    public override string AtlasTextureName => "NoxusBoss.Pixel.png";

    public StringLineParticle(Vector2 position, Vector2 velocity, Color color, Vector2 scale, float rotation, int lifetime)
    {
        Position = position;
        Velocity = velocity;
        Scale = scale;
        DrawColor = color;
        Opacity = 1f;
        Lifetime = lifetime;
        Rotation = rotation;
        Spin = Main.rand.NextFloat(0.1f, 0.25f) * Main.rand.NextFromList(-1f, 1f);
    }

    public override void Update()
    {
        Spin *= 0.98f;
        Rotation += Spin;
        Opacity = InverseLerp(1f, 0.25f, LifetimeRatio);

        Velocity.X *= 0.97f;
        Velocity.Y = Lerp(Velocity.Y, -0.4f, 0.028f);
    }
}
