using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.Particles;

public class CartoonAngerParticle : Particle
{
    public override BlendState BlendState => BlendState.Additive;

    public override string AtlasTextureName => "NoxusBoss.CartoonAngerParticle.png";

    public CartoonAngerParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifeTime)
    {
        Position = position;
        Velocity = velocity;
        DrawColor = color;
        Scale = Vector2.One * scale;
        Lifetime = lifeTime;
        Rotation = Main.rand.NextFloatDirection() * 0.4f;
    }

    public override void Update()
    {
        Opacity = InverseLerp(1f, 0.4f, LifetimeRatio);
        Velocity *= 0.95f;
        Scale = Vector2.One * Convert01To010(LifetimeRatio) * 0.5f;
    }
}
