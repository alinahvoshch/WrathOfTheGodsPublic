using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Threading;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Tiles.GenesisComponents;

[Autoload(Side = ModSide.Client)]
public class GenesisGrassTargetManager : ModSystem
{
    /// <summary>
    /// The render target that contains all grass data.
    /// </summary>
    public static ManagedRenderTarget GrassTarget
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        GrassTarget = new(true, (width, height) => new RenderTarget2D(Main.instance.GraphicsDevice, width / 16, height / 16, true, SurfaceFormat.Color, DepthFormat.Depth24, 8, RenderTargetUsage.DiscardContents));
        RenderTargetManager.RenderTargetUpdateLoopEvent += RenderToGrassTargetWrapper;
    }

    private static void RenderToGrassTargetWrapper()
    {
        if (Main.gameMenu)
            return;

        GraphicsDevice gd = Main.instance.GraphicsDevice;
        gd.SetRenderTarget(GrassTarget);
        gd.Clear(new Color(0, 0, 255));

        Main.tileBatch.Begin(Main.GameViewMatrix.TransformationMatrix);
        RenderToGrassTarget();
        Main.tileBatch.End();
    }

    private static void RenderToGrassTarget()
    {
        int left = (int)(Main.screenPosition.X / 16f);
        int right = (int)(left + Main.screenWidth / 16f) + 1;
        int top = (int)(Main.screenPosition.Y / 16f);
        int bottom = (int)(top + Main.screenHeight / 16f) + 1;

        int grassID = ModContent.TileType<GenesisGrass>();

        // Determine where grass is in sections across the X axis.
        Dictionary<int, List<Vector2>> grassPositionsByXPosition = new Dictionary<int, List<Vector2>>();
        for (int i = left; i < right; i++)
        {
            for (int j = top; j < bottom; j++)
            {
                if (i < 0 || i >= Main.maxTilesX || j < 0 || j >= Main.maxTilesY)
                    continue;

                if (Main.tile[i, j].TileType != grassID)
                    continue;

                Vector2 point = new Vector2(i, j);

                if (!grassPositionsByXPosition.ContainsKey(i))
                    grassPositionsByXPosition[i] = [];
                grassPositionsByXPosition[i].Add(point);
            }
        }

        // If no grass was found, terminate this method immediately.
        if (grassPositionsByXPosition.Count <= 0)
            return;

        float[,] distanceMap = new float[right - left, bottom - top];
        FastParallel.For(left, right, (int from, int to, object context) =>
        {
            for (int i = from; i < to; i++)
            {
                if (!grassPositionsByXPosition.TryGetValue(i, out List<Vector2>? grassPositions) || grassPositions is null || grassPositions.Count <= 0)
                {
                    for (int j = top; j < bottom; j++)
                        distanceMap[i - left, j - top] = 9999f;
                }
                else
                {
                    for (int j = top; j < bottom; j++)
                    {
                        Vector2 point = new Vector2(i, j);
                        Vector2 nearestGrassPosition = grassPositions.OrderBy(p => p.Distance(point)).First();
                        distanceMap[i - left, j - top] = point.Distance(nearestGrassPosition);
                    }
                }
            }
        });

        byte CalculateDistanceBrightnessValue(int x, int y)
        {
            if (x < left || x >= right || y < top || y >= bottom)
                return 0;

            float brightnessInterpolant = InverseLerp(16f, 0f, distanceMap[x - left, y - top]);
            return (byte)(brightnessInterpolant * 255f);
        }
        byte CalculateLightBrightnessValue(int x, int y)
        {
            if (x < left || x >= right || y < top || y >= bottom)
                return 0;

            Color c = Lighting.GetColor(x, y);
            return (byte)((c.R + c.G + c.B) / 3f);
        }
        byte CalculateGrassInfluenceValue(int x, int y)
        {
            if (x < left || x >= right || y < top || y >= bottom)
                return 0;

            Tile tile = Framing.GetTileSafely(x, y);
            GenesisGrassMergeData mergeData = tile.Get<GenesisGrassMergeData>();
            return (byte)(255 - (mergeData.LeftConversionInterpolant + mergeData.RightConversionInterpolant) * 127.5f);
        }

        Texture2D pixel = WhitePixel;
        for (int i = left; i < right; i++)
        {
            for (int j = top; j < bottom; j++)
            {
                if (Main.tile[i, j].TileType != grassID)
                    continue;

                Vector2 drawPosition = (new Vector2(i, j).ToWorldCoordinates() - Main.screenPosition) / 16f;
                Color color = new Color(CalculateDistanceBrightnessValue(i, j), CalculateLightBrightnessValue(i, j), CalculateGrassInfluenceValue(i, j));
                VertexColors colors = new VertexColors(color);

                Main.tileBatch.Draw(WhitePixel, drawPosition, new Rectangle(0, 0, 1, 1), colors, Vector2.Zero, 1f, 0);
            }
        }
    }
}
