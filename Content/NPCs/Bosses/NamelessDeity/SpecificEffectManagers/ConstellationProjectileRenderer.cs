using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Core.World.Subworlds;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;

[Autoload(Side = ModSide.Client)]
public class ConstellationProjectileRenderer : ModSystem
{
    internal struct StarData
    {
        public float Scale;

        public float Opacity;

        public Vector2 Position;
    }

    /// <summary>
    /// The amount of stars currently active.
    /// </summary>
    internal static int ActiveStarCount;

    /// <summary>
    /// The vertex buffer that contains all star information.
    /// </summary>
    internal static VertexBuffer StarVertexBuffer;

    /// <summary>
    /// The index buffer that contains all vertex pointers for <see cref="StarVertexBuffer"/>.
    /// </summary>
    internal static IndexBuffer StarIndexBuffer;

    public static int MaxStars => 8192;

    public override void OnModLoad()
    {
        Main.QueueMainThreadAction(() =>
        {
            StarVertexBuffer = new VertexBuffer(Main.instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, MaxStars * 4, BufferUsage.WriteOnly);
            StarIndexBuffer = new(Main.instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, MaxStars * 6, BufferUsage.WriteOnly);

            // Generate index data.
            int[] indices = new int[MaxStars * 6];
            for (int i = 0; i < MaxStars; i++)
            {
                int bufferIndex = i * 6;
                int vertexIndex = i * 4;
                indices[bufferIndex] = vertexIndex;
                indices[bufferIndex + 1] = vertexIndex + 1;
                indices[bufferIndex + 2] = vertexIndex + 2;
                indices[bufferIndex + 3] = vertexIndex + 2;
                indices[bufferIndex + 4] = vertexIndex + 1;
                indices[bufferIndex + 5] = vertexIndex + 3;
            }

            StarIndexBuffer.SetData(indices);
        });

        On_Main.DrawProjectiles += RenderStarsWrapper;
    }

    private static void RenderStarsWrapper(On_Main.orig_DrawProjectiles orig, Main self)
    {
        UpdateVertexBuffer();
        RenderStars();
        orig(self);
    }

    public override void OnModUnload()
    {
        Main.QueueMainThreadAction(() =>
        {
            StarVertexBuffer?.Dispose();
            StarIndexBuffer?.Dispose();
        });
    }

    private static void UpdateVertexBuffer()
    {
        List<StarData> starPoints = new List<StarData>();
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.ModProjectile is ConstellationProjectile constellationProjectile)
            {
                float scaleFactor = constellationProjectile.StarScaleFactor;
                int shapePointCount = constellationProjectile.ConstellationShape.ShapePoints.Count;
                for (int i = 0; i < shapePointCount; i++)
                {
                    Vector2 position = constellationProjectile.GetStarPosition(i);
                    starPoints.Add(new StarData()
                    {
                        Position = position,
                        Scale = scaleFactor,
                        Opacity = projectile.Opacity
                    });
                }
            }
        }

        ActiveStarCount = Math.Min(starPoints.Count, MaxStars);
        if (ActiveStarCount <= 0)
            return;

        // Generate vertex data.
        ulong seed = 2015uL;
        VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[MaxStars * 4];
        for (int i = 0; i < ActiveStarCount; i++)
        {
            float twinkle = Lerp(1f, 1.5f, Cos01(Main.GlobalTimeWrappedHourly * 1.5f + i));

            StarData star = starPoints[i];
            Vector2 starRadius = Vector2.One * star.Scale * twinkle * 6f;
            Vector2 starCenter = star.Position;
            float hueInterpolant = Utils.RandomFloat(ref seed);
            Color starColor = EternalGardenSkyStarRenderer.StarPalette.SampleColor(hueInterpolant);
            starColor = Color.Lerp(starColor, Color.Wheat, 0.3f) * star.Opacity;
            starColor.A = 0;

            // Generate vertex positions.
            Vector3 topLeftPosition = new Vector3(starCenter - starRadius, 0f);
            Vector3 topRightPosition = new Vector3(starCenter + new Vector2(1f, -1f) * starRadius, 0f);
            Vector3 bottomLeftPosition = new Vector3(starCenter + new Vector2(-1f, 1f) * starRadius, 0f);
            Vector3 bottomRightPosition = new Vector3(starCenter + starRadius, 0f);

            int bufferIndex = i * 4;
            vertices[bufferIndex] = new VertexPositionColorTexture(topLeftPosition, starColor, Vector2.Zero);
            vertices[bufferIndex + 1] = new VertexPositionColorTexture(topRightPosition, starColor, Vector2.UnitX);
            vertices[bufferIndex + 2] = new VertexPositionColorTexture(bottomLeftPosition, starColor, Vector2.UnitY);
            vertices[bufferIndex + 3] = new VertexPositionColorTexture(bottomRightPosition, starColor, Vector2.One);
        }

        // Send the vertices to the buffer.
        StarVertexBuffer.SetData(vertices);
    }

    private static void RenderStars()
    {
        if (ActiveStarCount <= 0)
            return;

        Vector2 screenSize = ViewportSize;

        ManagedShader starShader = ShaderManager.GetShader("NoxusBoss.NamelessConstellationStarShader");
        starShader.TrySetParameter("projection", Main.GameViewMatrix.TransformationMatrix * Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f));
        starShader.TrySetParameter("screenSize", screenSize);
        starShader.SetTexture(BloomFlare, 1, SamplerState.LinearWrap);
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
