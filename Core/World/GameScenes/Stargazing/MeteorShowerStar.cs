using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;

namespace NoxusBoss.Core.World.GameScenes.Stargazing;

public class MeteorShowerStar
{
    /// <summary>
    /// The scale of this star.
    /// </summary>
    public float Scale;

    /// <summary>
    /// The position of this star.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// The velocity of this star.
    /// </summary>
    public Vector2 Velocity;

    public MeteorShowerStar(float scale, Vector2 position, Vector2 velocity)
    {
        Scale = scale;
        Position = position;
        Velocity = velocity;
    }

    /// <summary>
    /// Updates this star.
    /// </summary>
    public void Update()
    {
        Scale -= 0.014f;
        Velocity = (Velocity * 1.04f).ClampLength(0f, 36f);
        Position += Velocity;
    }

    /// <summary>
    /// Renders this star.
    /// </summary>
    /// <param name="viewOffset">The viewing offset of the star.</param>
    public void Render(Vector2 viewOffset)
    {
        Texture2D beautifulStarTexture = GennedAssets.Textures.GreyscaleTextures.Star.Value;

        Color bloomColor = new Color(255, 255, 187, 0) * 0.95f;
        Color twinkleColor = Color.Orange with { A = 0 };
        for (int i = 13; i >= 0; i--)
        {
            float afterimageInterpolant = i / 13f;
            float afterimageScale = (1f - afterimageInterpolant) * Scale;
            Vector2 afterimageOffset = -Velocity * afterimageInterpolant * Scale * 4f;
            TwinkleParticle.DrawTwinkle(beautifulStarTexture, Position + viewOffset + afterimageOffset, 8, 0f, bloomColor, twinkleColor, Vector2.One * afterimageScale * 0.04f, 0.7f);
        }
    }
}
