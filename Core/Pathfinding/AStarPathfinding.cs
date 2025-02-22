using Microsoft.Xna.Framework;
using Terraria;
using Vector2SIMD = System.Numerics.Vector2;

namespace NoxusBoss.Core.Pathfinding;

public static class AStarPathfinding
{
    /// <summary>
    /// Performs pathfinding from one point to another, representing the results as a set of points.
    /// </summary>
    /// <param name="start">The starting point to pathfind from.</param>
    /// <param name="end">The destination point to try and pathfind to.</param>
    /// <param name="costFunction">The cost function. Used to penalize characteristics of certain paths.</param>
    /// <param name="maxIterations">The maximum amount of iterations that should be considered before a path is considered inaccessible.</param>
    /// <returns>The list of points on the tile map that compose the path. This list is empty if no valid path could be found.</returns>
    public static List<Vector2> PathfindThroughTiles(Vector2 start, Vector2 end, Func<Point, float> costFunction, int maxIterations = 10000)
    {
        Vector2SIMD startSimd = new Vector2SIMD((int)(start.X / 16f) - 0.5f, (int)(start.Y / 16f)) * 16f;
        Vector2SIMD endSimd = new Vector2SIMD(end.X, end.Y);

        int iterations = 0;
        PathfindingInfo info = new PathfindingInfo(startSimd, endSimd);
        Vector2SIMD endInConnectionMap = Vector2SIMD.Zero;

        while (info.OpenSet.Count >= 1)
        {
            iterations++;
            if (iterations >= maxIterations)
                return [];

            Vector2SIMD current = info.OpenSet.Dequeue();

            // Check if the end point has been reached. If it has, proceed to the construct the path that lead to it.
            if (Vector2SIMD.DistanceSquared(current, endSimd) <= 256.01f)
            {
                endInConnectionMap = current;
                break;
            }

            info.UpdateFromNode(current, costFunction);
        }

        return info.GetPath(endInConnectionMap);
    }

    /// <summary>
    /// Casts dust along a path, for debug display purposes.
    /// </summary>
    /// <param name="path">The list of points that compose the path.</param>
    public static void DebugDrawPath(List<Vector2> path)
    {
        for (int i = 0; i < path.Count; i++)
            Dust.QuickDust(path[i], Color.Yellow);
    }
}
