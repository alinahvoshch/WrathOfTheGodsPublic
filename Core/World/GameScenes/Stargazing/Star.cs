using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.DataStructures;

namespace NoxusBoss.Core.World.GameScenes.Stargazing;

public readonly struct Star
{
    /// <summary>
    /// The orientation of this star in 3D space.
    /// </summary>
    public Vector3 Orientation { get; }

    /// <summary>
    /// The radius of this star, in pixels.
    /// </summary>
    public float Radius { get; }

    /// <summary>
    /// The color of this star.
    /// </summary>
    public Color Color { get; }

    public Star(float latitude, float longitude, Color color, float radius)
    {
        float latitudeCosine = Cos(latitude);
        float latitudeSine = Sin(latitude);
        float longitudeCosine = Cos(longitude);
        float longitudeSine = Sin(longitude);

        Orientation = new(latitudeCosine * longitudeCosine, latitudeCosine * longitudeSine, latitudeSine);
        Color = color;
        Radius = radius;
    }

    internal Quad<VertexPositionColorTexture> GenerateVertices(float scale)
    {
        // Calculate screen-relative position values.
        Vector2 screenSize = ViewportSize;
        Vector3 radiusVector = new Vector3(1f / screenSize.X, 1f / screenSize.Y, 0f) * Radius * scale;
        Vector3 screenPosition = Orientation;

        // Generate vertex positions.
        Vector3 topLeftPosition = screenPosition - radiusVector;
        Vector3 topRightPosition = screenPosition + new Vector3(1f, -1f, 0f) * radiusVector;
        Vector3 bottomLeftPosition = screenPosition + new Vector3(-1f, 1f, 0f) * radiusVector;
        Vector3 bottomRightPosition = screenPosition + radiusVector;

        // Generate vertices.
        VertexPositionColorTexture topLeft = new VertexPositionColorTexture(topLeftPosition, Color, Vector2.Zero);
        VertexPositionColorTexture topRight = new VertexPositionColorTexture(topRightPosition, Color, Vector2.UnitX);
        VertexPositionColorTexture bottomLeft = new VertexPositionColorTexture(bottomLeftPosition, Color, Vector2.UnitY);
        VertexPositionColorTexture bottomRight = new VertexPositionColorTexture(bottomRightPosition, Color, Vector2.One);
        return new(topLeft, topRight, bottomLeft, bottomRight);
    }
}
