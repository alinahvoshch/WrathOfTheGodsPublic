using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.World.Subworlds;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;
using SpecialStar = NoxusBoss.Core.World.GameScenes.Stargazing.Star;

namespace NoxusBoss.Core.World.GameScenes.Stargazing;

[Autoload(Side = ModSide.Client)]
public class StarGazingStarsRenderer : ModSystem
{
    /// <summary>
    /// The set of all stars in the sky.
    /// </summary>
    internal static SpecialStar[] Stars;

    /// <summary>
    /// The vertex buffer that contains all star information.
    /// </summary>
    internal static VertexBuffer StarVertexBuffer;

    /// <summary>
    /// The index buffer that contains all vertex pointers for <see cref="StarVertexBuffer"/>.
    /// </summary>
    internal static IndexBuffer StarIndexBuffer;

    /// <summary>
    /// The minimum brightness that a star can be at as a result of twinkling.
    /// </summary>
    public const float MinTwinkleBrightness = 0.2f;

    /// <summary>
    /// The maximum brightness that a star can be at as a result of twinkling.
    /// </summary>
    public const float MaxTwinkleBrightness = 3.37f;

    public override void OnModLoad()
    {
        // Generate stars.
        GenerateStars();
    }

    internal static void GenerateStars()
    {
        int starCount = 80000;
        UnifiedRandom rng = new UnifiedRandom(74);

        Stars = new SpecialStar[starCount];
        for (int i = 0; i < Stars.Length; i++)
        {
            Color color = EternalGardenSkyStarRenderer.StarPalette.SampleColor(rng.NextFloat().Squared());
            color = Color.Lerp(color, Color.White, 0.45f);
            color.A = 0;

            float latitude = rng.NextFloat(-PiOver2, PiOver2) * Sqrt(rng.NextFloat());
            float longitude = rng.NextFloat(-Pi, Pi);
            float radius = Lerp(1.25f, 2.25f, Pow(rng.NextFloat(), 11f));
            Stars[i] = new(latitude, longitude, color * Pow(radius / 2.25f, 1.5f), radius);
        }

        Main.QueueMainThreadAction(RegenerateBuffers);
    }

    internal static void RegenerateBuffers()
    {
        RegenerateVertexBuffer();
        RegenerateIndexBuffer();
    }

    internal static void RegenerateVertexBuffer()
    {
        // Initialize the star buffer if necessary.
        StarVertexBuffer?.Dispose();
        StarVertexBuffer = new VertexBuffer(Main.instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, Stars.Length * 4, BufferUsage.WriteOnly);

        // Generate vertex data.
        VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[Stars.Length * 4];
        for (int i = 0; i < Stars.Length; i++)
        {
            // Acquire vertices for the star.
            Quad<VertexPositionColorTexture> quad = Stars[i].GenerateVertices(1f);

            int bufferIndex = i * 4;
            vertices[bufferIndex] = quad.TopLeft;
            vertices[bufferIndex + 1] = quad.TopRight;
            vertices[bufferIndex + 2] = quad.BottomRight;
            vertices[bufferIndex + 3] = quad.BottomLeft;
        }

        // Send the vertices to the buffer.
        StarVertexBuffer.SetData(vertices);
    }

    internal static void RegenerateIndexBuffer()
    {
        // Initialize the star buffer if necessary.
        StarIndexBuffer?.Dispose();
        StarIndexBuffer = new(Main.instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, Stars.Length * 6, BufferUsage.WriteOnly);

        // Generate index data.
        int[] indices = new int[Stars.Length * 6];
        for (int i = 0; i < Stars.Length; i++)
        {
            int bufferIndex = i * 6;
            int vertexIndex = i * 4;
            indices[bufferIndex] = vertexIndex;
            indices[bufferIndex + 1] = vertexIndex + 1;
            indices[bufferIndex + 2] = vertexIndex + 2;
            indices[bufferIndex + 3] = vertexIndex + 2;
            indices[bufferIndex + 4] = vertexIndex + 3;
            indices[bufferIndex + 5] = vertexIndex;
        }

        StarIndexBuffer.SetData(indices);
    }

    internal static Matrix CalculateProjectionMatrix()
    {
        // Project the stars onto the screen.
        float height = Main.instance.GraphicsDevice.Viewport.Height / (float)Main.instance.GraphicsDevice.Viewport.Width;
        Matrix projection = Matrix.CreateOrthographicOffCenter(-1f, 1f, height, -height, -1f, 0f);

        // Zoom in slightly on the stars, so that the sphere does not abruptly end at the bounds of the screen.
        Matrix screenStretch = Matrix.CreateScale(6f, height * 4f, 1f);

        // Combine matrices together.
        return projection * screenStretch;
    }

    public static void Render(float opacity, Matrix backgroundMatrix)
    {
        Vector2 screenSize = ViewportSize;

        ManagedShader starShader = ShaderManager.GetShader("NoxusBoss.StarPrimitiveShader");
        starShader.TrySetParameter("opacity", opacity);
        starShader.TrySetParameter("projection", backgroundMatrix * CalculateProjectionMatrix());
        starShader.TrySetParameter("sunPosition", Vector2.One * 50000f);
        starShader.TrySetParameter("minTwinkleBrightness", MinTwinkleBrightness);
        starShader.TrySetParameter("maxTwinkleBrightness", MaxTwinkleBrightness);
        starShader.TrySetParameter("distanceFadeoff", 1f);
        starShader.TrySetParameter("screenSize", screenSize);
        starShader.SetTexture(BloomCircleSmall, 1, SamplerState.LinearWrap);
        starShader.Apply();

        Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        // Render the stars.
        Main.instance.GraphicsDevice.Indices = StarIndexBuffer;
        Main.instance.GraphicsDevice.SetVertexBuffer(StarVertexBuffer);
        Main.instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, StarVertexBuffer.VertexCount, 0, StarIndexBuffer.IndexCount / 3);
        Main.instance.GraphicsDevice.SetVertexBuffer(null);
        Main.instance.GraphicsDevice.Indices = null;
    }
}
