using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.Items.Placeable;
using NoxusBoss.Content.Particles;
using NoxusBoss.Content.Tiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.StarblessedPlatforms;

public class StarblessedPlatformSystem : ModSystem
{
    /// <summary>
    /// The tile ID of the source platforms that should be recorded.
    /// </summary>
    public static int PlatformSourceTileID
    {
        get;
        private set;
    }

    /// <summary>
    /// The set of all unit vector directions that platform sources can extend out in in the hopes of connecting with another platform source.
    /// </summary>
    public static readonly Vector2[] PossibleConnectDirections = new Vector2[]
    {
        -Vector2.UnitX,
        Vector2.UnitX,
    };

    public override void OnModLoad()
    {
        On_WorldGen.PlaceTile += RecordPlatformPoints;
        On_TileObject.Place += RecordPlatformPoints2;
        On_WorldGen.KillTile += KillPlatformPoints;
    }

    public override void PostSetupContent() => PlatformSourceTileID = ModContent.TileType<StarblessedPlatformSource>();

    /// <summary>
    /// Finds and returns all potential candidates in which a hypothetical starblessed platform could connect.
    /// </summary>
    /// <param name="tilePlacementPoint"></param>
    /// <returns></returns>
    public static List<Point> FindPotentialConnectionCandidates(Point tilePlacementPoint)
    {
        int platformID = ModContent.TileType<StarblessedPlatformTile>();

        List<Point> candidates = new List<Point>(PossibleConnectDirections.Length);
        foreach (Vector2 direction in PossibleConnectDirections)
        {
            for (int i = 1; i < StarblessedPlatform.MaximumReach; i++)
            {
                Point checkPoint = (tilePlacementPoint.ToVector2() + direction * i).ToPoint();
                Tile checkTile = Framing.GetTileSafely(checkPoint);

                if (checkTile.HasUnactuatedTile)
                {
                    if (checkTile.TileType == PlatformSourceTileID)
                        candidates.Add(checkPoint);

                    bool hitObstruction = !Main.tileCut[checkTile.TileType];
                    if (checkTile.TileType != platformID && hitObstruction)
                        break;
                }
            }
        }

        return candidates;
    }

    /// <summary>
    /// Performs a tile loop action from a given point to all possible connection candidates.
    /// </summary>
    /// <param name="point">The starting point from which candidates should be searched for.</param>
    /// <param name="tileAction">The action to perform across each tile in the connection.</param>
    public static void PerformConnectionLoopAction(Point point, Action<Point, Point> tileAction)
    {
        List<Point> candidates = FindPotentialConnectionCandidates(point);
        foreach (Point candidate in candidates)
        {
            Vector2 offset = candidate.ToVector2() - point.ToVector2();
            Vector2 direction = offset.SafeNormalize(Vector2.Zero);
            float distanceToCandidate = offset.Length();
            for (int i = 0; i < distanceToCandidate; i++)
            {
                Point platformPoint = (point.ToVector2() + direction * i).ToPoint();
                tileAction(platformPoint, candidate);
            }
        }
    }

    /// <summary>
    /// Attemptes to establish a connection between a given source tile and sufficiently close nearby source tiles.
    /// </summary>
    /// <param name="point">The tile position to try and establish connections from.</param>
    public static void AttemptToEstablishConnection(Point point)
    {
        int platformID = ModContent.TileType<StarblessedPlatformTile>();
        PerformConnectionLoopAction(point, (platformPoint, candidate) =>
        {
            if (StarblessedPlatformProjectionSystem.Projections.Any(p => p.Line.Start == point.ToVector2()))
                return;

            // Apply imaginary framing across the projection to determine what tile frames should be used.
            LineSegment line = new LineSegment(point.ToVector2(), candidate.ToVector2());
            Vector2 offset = line.End - line.Start;
            Vector2 direction = offset.SafeNormalize(Vector2.Zero);
            float distanceToCandidate = offset.Length();
            List<Rectangle> frames = new List<Rectangle>((int)Ceiling(distanceToCandidate));
            for (int i = 1; i < distanceToCandidate; i++)
            {
                Point localPlatformPoint = (line.Start + direction * i).ToPoint();
                WorldGen.PlaceObject(localPlatformPoint.X, localPlatformPoint.Y, (ushort)platformID, true);

                frames.Add(new(Main.tile[localPlatformPoint].TileFrameX, Main.tile[localPlatformPoint].TileFrameY, 18, 18));
            }

            // Undo the tile framing.
            for (int i = 1; i < distanceToCandidate; i++)
            {
                Point localPlatformPoint = (line.Start + direction * i).ToPoint();
                Main.tile[localPlatformPoint].Get<TileWallWireStateData>().HasTile = false;
            }

            SoundEngine.PlaySound(GennedAssets.Sounds.Item.StarblessedPlatformActivate, point.ToWorldCoordinates());
            StarblessedPlatformProjectionSystem.Projections.Add(new StarblessedPlatformProjectionSystem.PlatformProjection()
            {
                Line = line,
                Frames = frames
            });
        });
    }

    /// <summary>
    /// Attemptes to clear a connection between a given source tile and otherwise connected source tiles.
    /// </summary>
    /// <param name="point">The tile position to try and remove connections from.</param>
    public static void AttemptToClearConnection(Point point)
    {
        int platformID = ModContent.TileType<StarblessedPlatformTile>();
        PerformConnectionLoopAction(point, (platformPoint, candidate) =>
        {
            if (Framing.GetTileSafely(platformPoint).TileType != platformID)
                return;

            SoundEngine.PlaySound(GennedAssets.Sounds.Item.StarblessedPlatformDeactivate, point.ToWorldCoordinates());
            WorldGen.KillTile(platformPoint.X, platformPoint.Y);
            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendTileSquare(-1, platformPoint.X, platformPoint.Y);

            for (int i = 0; i < 3; i++)
            {
                Vector2 starSpawnPosition = platformPoint.ToWorldCoordinates() + Main.rand.NextVector2Circular(5f, 5f);
                CreateStar(starSpawnPosition);
            }
        });
    }

    /// <summary>
    /// Crates a single star particle for a platform.
    /// </summary>
    /// <param name="starSpawnPosition">The spawn position of the star.</param>
    public static void CreateStar(Vector2 starSpawnPosition)
    {
        int starPoints = Main.rand.Next(3, 9);
        float starScaleInterpolant = Main.rand.NextFloat();
        int starLifetime = (int)Lerp(20f, 54f, starScaleInterpolant);
        float starScale = Lerp(0.2f, 0.4f, starScaleInterpolant);
        Color starColor = Color.Lerp(Color.Yellow, Color.Wheat, 0.4f) * 0.85f;
        Vector2 starVelocity = Main.rand.NextVector2Circular(3f, 4f) - Vector2.UnitY * 2.9f;
        TwinkleParticle star = new TwinkleParticle(starSpawnPosition, starVelocity, starColor, starLifetime, starPoints, new Vector2(Main.rand.NextFloat(0.4f, 1.6f), 1f) * starScale, starColor * 0.5f);
        star.Spawn();
    }

    private bool RecordPlatformPoints(On_WorldGen.orig_PlaceTile orig, int i, int j, int Type, bool mute, bool forced, int plr, int style)
    {
        bool successfulPlacementExceptNotReally = orig(i, j, Type, mute, forced, plr, style);
        if (successfulPlacementExceptNotReally && Type == PlatformSourceTileID)
            AttemptToEstablishConnection(new(i, j));

        return successfulPlacementExceptNotReally;
    }

    private bool RecordPlatformPoints2(On_TileObject.orig_Place orig, TileObject toBePlaced)
    {
        bool successfulPlacement = orig(toBePlaced);
        if (successfulPlacement && toBePlaced.type == PlatformSourceTileID)
            AttemptToEstablishConnection(new(toBePlaced.xCoord, toBePlaced.yCoord));

        return successfulPlacement;
    }

    private void KillPlatformPoints(On_WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem)
    {
        int tileID = Framing.GetTileSafely(i, j).TileType;
        orig(i, j, fail, effectOnly, noItem);

        if (!fail && !effectOnly)
        {
            if (tileID == PlatformSourceTileID)
                AttemptToClearConnection(new(i, j));
        }
    }
}
