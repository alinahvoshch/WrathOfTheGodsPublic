using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Vector2SIMD = System.Numerics.Vector2;

namespace NoxusBoss.Core.Pathfinding;

internal class PathfindingInfo
{
    internal readonly Vector2SIMD End;

    internal readonly PriorityQueue<Vector2SIMD, float> OpenSet = new PriorityQueue<Vector2SIMD, float>(2048);

    // Using a separate hashset for determining if a point exists is a lot more efficient than using LINQ on the priority queue to determine if it contains the given type.
    internal readonly HashSet<Vector2SIMD> SeenPoints = new HashSet<Vector2SIMD>(2048);

    internal readonly Dictionary<Vector2SIMD, float> GScores = [];

    internal readonly Dictionary<Vector2SIMD, Vector2SIMD> PathConnectionMap = [];

    internal static readonly Vector2SIMD[] CardinalOffsets = new Vector2SIMD[4]
    {
        Vector2SIMD.UnitX,
        -Vector2SIMD.UnitX,
        Vector2SIMD.UnitY,
        -Vector2SIMD.UnitY
    };

    internal PathfindingInfo(Vector2SIMD start, Vector2SIMD end)
    {
        OpenSet.Enqueue(start, 0f);
        GScores[start] = 0f;
        End = end;
    }

    /// <summary>
    /// Attempts to evaluate a G cost score value at a given position, defaulting to a value of <see cref="float.PositiveInfinity"/>.
    /// </summary>
    /// <param name="position">The position to evaluate the G cost score value of.</param>
    internal float GetGScore(Vector2SIMD position)
    {
        if (GScores.TryGetValue(position, out float g))
            return g;

        return GScores[position] = float.PositiveInfinity;
    }

    /// <summary>
    /// Expands the open set outward, seeking new candidates to explore in the search space.
    /// </summary>
    /// <param name="current">The current node to update from.</param>
    /// <param name="costFunction">The cost function. Used to penalize characteristics of certain paths.</param>
    internal void UpdateFromNode(Vector2SIMD current, Func<Point, float> costFunction)
    {
        // Iterate through cardinal nodes.
        for (int i = 0; i < CardinalOffsets.Length; i++)
        {
            float tentativeScore = GetGScore(current) + 16f;

            // Discard a potential node if it goes directly into tiles.
            Vector2SIMD neighborPosition = current + CardinalOffsets[i] * 16f;
            Point neighborPositionTileCoords = new Point((int)(neighborPosition.X / 16f), (int)(neighborPosition.Y / 16f));
            Tile tile = Framing.GetTileSafely(neighborPositionTileCoords.X, neighborPositionTileCoords.Y);
            bool isModdedDoor = TileID.Sets.CloseDoorID[tile.TileType] != -1 && TileID.Sets.OpenDoorID[tile.TileType] != -1;
            bool isVanillaDoor = tile.TileType == TileID.ClosedDoor;
            if (WorldGen.SolidTile(tile) && !isModdedDoor && !isVanillaDoor)
                continue;

            Vector2 ground = FindGround(neighborPositionTileCoords, Vector2.UnitY).ToWorldCoordinates();
            float distanceToGround = Vector2SIMD.Distance(neighborPosition, Unsafe.As<Vector2, Vector2SIMD>(ref ground));
            tentativeScore += distanceToGround * 32f;

            // Evaluate whether the neighbor can lower the overall cost function of its parent.
            // If so, add it to the open set.
            float neighborGScore = GetGScore(neighborPosition);
            if (tentativeScore >= neighborGScore)
                continue;

            PathConnectionMap[neighborPosition] = current;
            GScores[neighborPosition] = tentativeScore;

            float neighborDistanceToEnd = Distance(End.X, neighborPosition.X) + Distance(End.Y, neighborPosition.Y);
            float fScore = GScores[neighborPosition] + neighborDistanceToEnd + costFunction(neighborPositionTileCoords);
            if (!SeenPoints.Contains(neighborPosition))
            {
                OpenSet.Enqueue(neighborPosition, fScore);
                SeenPoints.Add(neighborPosition);
            }
        }
    }

    /// <summary>
    /// Returns a path from a given end point to its starting point as a list of points.
    /// </summary>
    /// <param name="end">The point to go backwards from.</param>
    internal List<Vector2> GetPath(Vector2SIMD end)
    {
        Vector2 current = Unsafe.As<Vector2SIMD, Vector2>(ref end);
        List<Vector2> path = new List<Vector2>();
        while (PathConnectionMap.TryGetValue(Unsafe.As<Vector2, Vector2SIMD>(ref current), out Vector2SIMD pathPoint))
        {
            if (path.Count >= 1000)
                return [];

            path.Add(current);
            current = Unsafe.As<Vector2SIMD, Vector2>(ref pathPoint);
        }

        path.Reverse();
        return path;
    }
}
