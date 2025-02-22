using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.Particles;

public class BloomCircleGleamParticle : Particle
{
    /// <summary>
    /// The maximum scale of this bloom circle.
    /// </summary>
    public float MaxScale;

    public override BlendState BlendState => BlendState.Additive;

    public override string AtlasTextureName => "NoxusBoss.StrongBloom.png";

    public BloomCircleGleamParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale)
    {
        Position = position;
        Velocity = velocity;
        DrawColor = color;
        MaxScale = scale;
        Lifetime = lifetime;
    }

    public override void Update()
    {
        Scale = Vector2.One * InverseLerpBump(0f, 0.2f, 0.2f, 1f, LifetimeRatio) * MaxScale;
        Opacity = InverseLerp(1f, 0.3f, LifetimeRatio);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Texture, Position - Main.screenPosition, null, DrawColor * Opacity, Rotation, null, Scale, 0);
        spriteBatch.Draw(Texture, Position - Main.screenPosition, null, Color.Lerp(DrawColor, Color.White, 0.6f) * Opacity, Rotation, null, Scale * 0.56f, 0);
        spriteBatch.Draw(Texture, Position - Main.screenPosition, null, Color.White * Opacity, Rotation, null, Scale * 0.28f, 0);
    }
}
