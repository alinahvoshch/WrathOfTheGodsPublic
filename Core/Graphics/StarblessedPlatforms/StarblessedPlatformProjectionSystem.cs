using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Tiles;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.World.TileDisabling;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NoxusBoss.Core.Graphics.StarblessedPlatforms;

public class StarblessedPlatformProjectionSystem : ModSystem
{
    public class PlatformProjection
    {
        public LineSegment Line;

        public List<Rectangle> Frames;

        public int Time;

        public void Update()
        {
            Time++;
        }
    }

    public static readonly List<PlatformProjection> Projections = [];

    /// <summary>
    /// How long platform projections take to fully materialize, in frames.
    /// </summary>
    public static int ProjectionCastTime => SecondsToFrames(2f);

    public override void OnModLoad()
    {
        On_TileObject.DrawPreview += DrawPotentialConnectionsWrapper;
        GlobalTileEventHandlers.NearbyEffectsEvent += DisableTilePlacementDuringProjection;
    }

    private void DisableTilePlacementDuringProjection(int x, int y, int type, bool closer)
    {
        if (InWayOfProjection(x, y))
            WorldGen.KillTile(x, y);
    }

    private static bool InWayOfProjection(int x, int y)
    {
        int platformID = ModContent.TileType<StarblessedPlatformTile>();
        int sourceID = ModContent.TileType<StarblessedPlatformSource>();
        int tileID = Framing.GetTileSafely(x, y).TileType;
        foreach (PlatformProjection projection in Projections)
        {
            if (tileID != platformID && tileID != sourceID && Collision.CheckAABBvLineCollision(new Vector2(x - 0.5f, y - 0.5f), Vector2.One, projection.Line.Start, projection.Line.End))
                return true;
        }

        return false;
    }

    public override void OnWorldLoad() => Projections.Clear();

    public override void OnWorldUnload() => Projections.Clear();

    private void DrawPotentialConnectionsWrapper(On_TileObject.orig_DrawPreview orig, SpriteBatch sb, TileObjectPreviewData previewData, Vector2 position)
    {
        if (previewData.Type == StarblessedPlatformSystem.PlatformSourceTileID)
        {
            TileObjectData tileData = TileObjectData.GetTileData(previewData.Type, previewData.Style, previewData.Alternate);
            Vector2 drawCenter = new Vector2(previewData.Coordinates.X * 16 - (int)(position.X - 8f) + tileData.DrawXOffset, previewData.Coordinates.Y * 16 - (int)position.Y + 8f);
            DrawPotentialConnections(new(previewData.Coordinates.X, previewData.Coordinates.Y), drawCenter);
        }

        orig(sb, previewData, position);
    }

    /// <summary>
    /// Draws all potential connections that can be made between platform source tiles.
    /// </summary>
    /// <param name="tilePlacementPoint">The tile placement position for the platform source.</param>
    /// <param name="drawCenter">The draw center.</param>
    public static void DrawPotentialConnections(Point tilePlacementPoint, Vector2 drawCenter)
    {
        List<Point> candidates = StarblessedPlatformSystem.FindPotentialConnectionCandidates(tilePlacementPoint);
        foreach (Point candidate in candidates)
        {
            Vector2 offset = candidate.ToVector2() - tilePlacementPoint.ToVector2();
            float distanceToCandidate = offset.Length();
            Vector2 lineStart = drawCenter + Main.screenPosition;
            Vector2 lineEnd = lineStart + offset.SafeNormalize(Vector2.Zero) * distanceToCandidate * 16f;
            DrawBasicPreviewLine(lineStart, lineEnd);
        }
    }

    /// <summary>
    /// Casts a basic colored projection line from one end to another for the purposes of previewing what connections would look like.
    /// </summary>
    /// <param name="lineStart">The starting position of the line, in world coordinates.</param>
    /// <param name="lineEnd">The ending position of the line, in world coordinates.</param>
    public static void DrawBasicPreviewLine(Vector2 lineStart, Vector2 lineEnd)
    {
        Utils.DrawLine(Main.spriteBatch, lineStart, lineEnd, new Color(212, 192, 139) * 0.4f, Color.Transparent, 4f);
    }

    /// <summary>
    /// Places a single starblessed platform down.
    /// </summary>
    /// <param name="platformPoint">The placement position of the platform.</param>
    public static void PlacePlatform(Point platformPoint)
    {
        int platformID = ModContent.TileType<StarblessedPlatformTile>();
        WorldGen.PlaceObject(platformPoint.X, platformPoint.Y, platformID, true);

        if (Main.netMode != NetmodeID.SinglePlayer)
            NetMessage.SendTileSquare(-1, platformPoint.X, platformPoint.Y);
    }

    /// <summary>
    /// Creates particles that indicate a platform materialization effect.
    /// </summary>
    /// <param name="particleSpawnPosition">The particle's spawn position.</param>
    public static void CreateMaterializationParticles(Vector2 particleSpawnPosition)
    {
        ParticleOrchestrator.SpawnParticlesDirect(ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings()
        {
            PositionInWorld = particleSpawnPosition,
            MovementVector = Main.rand.NextVector2Circular(2f, 2f)
        });
        StarblessedPlatformSystem.CreateStar(particleSpawnPosition);
    }

    public override void PostDrawTiles()
    {
        if (Projections.Count <= 0 || TileDisablingSystem.TilesAreUninteractable)
            return;

        Main.spriteBatch.ResetToDefault(false);

        int platformID = ModContent.TileType<StarblessedPlatformTile>();
        Texture2D platformTexture = TextureAssets.Tile[platformID].Value;

        foreach (PlatformProjection projection in Projections)
        {
            Vector2 offset = projection.Line.End - projection.Line.Start;
            Vector2 direction = offset.SafeNormalize(Vector2.Zero);
            float distanceToCandidate = offset.Length();
            float materializeInterpolant = InverseLerp(0f, ProjectionCastTime - 5f, projection.Time);
            float platformDisappearStart = Lerp(0.7f, 0.99f, materializeInterpolant.Squared());
            float totalTilesToMaterialize = distanceToCandidate * EasingCurves.Cubic.Evaluate(EasingType.InOut, materializeInterpolant);

            DrawBasicPreviewLine(projection.Line.Start.ToWorldCoordinates(), projection.Line.End.ToWorldCoordinates());

            for (int i = 1; i < distanceToCandidate; i++)
            {
                Rectangle frame = projection.Frames[i - 1];
                if (frame.X == 18 || frame.X == 36 || frame.X == 90)
                    frame.X = 0;

                // Place platforms as the projection materializes.
                // The Distance(i, totalTilesToMaterialize) term ensures that the placement old happens near the end of the line.
                Point platformPoint = (projection.Line.Start + direction * i).ToPoint();
                if (Distance(i, totalTilesToMaterialize) <= 6f && i < totalTilesToMaterialize)
                    PlacePlatform(platformPoint);

                Vector2 drawPosition = platformPoint.ToWorldCoordinates(8f, 10f) - Main.screenPosition;
                if (i >= totalTilesToMaterialize)
                {
                    frame.Width = (int)(frame.Width * totalTilesToMaterialize.Modulo(1f));
                    if (offset.X < 0f)
                        drawPosition.X += 16 - frame.Width;

                    CreateMaterializationParticles(drawPosition + Main.screenPosition);
                }

                if (i >= totalTilesToMaterialize + 1)
                    break;

                Color platformColor = Color.White * InverseLerp(1f, platformDisappearStart, i / totalTilesToMaterialize);
                Main.spriteBatch.Draw(platformTexture, drawPosition, frame, platformColor, 0f, new Vector2(8f, 8f), 1f, 0, 0f);
            }
        }

        Main.spriteBatch.End();
    }

    public override void PostUpdateEverything()
    {
        int platformID = ModContent.TileType<StarblessedPlatformTile>();

        foreach (PlatformProjection projection in Projections)
            projection.Time++;

        Projections.RemoveAll(p => p.Time >= ProjectionCastTime);
    }
}
