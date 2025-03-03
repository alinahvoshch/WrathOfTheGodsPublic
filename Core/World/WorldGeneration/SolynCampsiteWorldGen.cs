using Microsoft.Xna.Framework;
using NoxusBoss.Content.Tiles.SolynCampsite;
using NoxusBoss.Content.Tiles.TileEntities;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace NoxusBoss.Core.World.WorldGeneration;

public class SolynCampsiteWorldGen : ModSystem
{
    private static bool generatingAlready;

    /// <summary>
    /// The position of the camp site, in world coordinates.
    /// </summary>
    public static Vector2 CampSitePosition
    {
        get;
        set;
    }

    /// <summary>
    /// The position of the tent, in world coordinates.
    /// </summary>
    public static Vector2 TentPosition
    {
        get;
        set;
    }

    /// <summary>
    /// The position of the telescope, in tile coordinates.
    /// </summary>
    public static Point TelescopePosition
    {
        get;
        set;
    }

    /// <summary>
    /// The width that the campsite generation checks when finding a spot to place.
    /// </summary>
    public static int CampsiteSurveyWidth => 65;

    /// <summary>
    /// The closest that meteors can generate to Solyn's campsite.
    /// </summary>
    public static int MinMeteorDistance => 120;

    /// <summary>
    /// The radius of unbreakability surrounding Solyn's campsite.
    /// </summary>
    public static float UnbreakableRadius => 736f;

    /// <summary>
    /// Attempts to find a position in the world where Solyn's camp site could be placed. Returns <see cref="Point.Zero"/> if none could be found.
    /// </summary>
    public static Point FindGenerationSpot()
    {
        for (int tries = 0; tries < 6400; tries++)
        {
            int x = (int)(WorldGen.genRand.NextFloat(0.054f, 0.46f) * Main.maxTilesX);
            if (WorldGen.genRand.NextBool())
                x = Main.maxTilesX - x;

            int y = FindGroundVertical(new Point(x, 375)).Y;

            // Reject positions that are too high up. That almost certainly means that a floating island was hit.
            if (y <= Main.maxTilesY * 0.185f)
                continue;

            // Reject positions inside of water.
            Point center = new Point(x, y);
            if (Main.tile[center].LiquidAmount >= 1 || Main.tile[center].IsHalfBlock || Main.tile[center].Slope != SlopeType.Solid)
                continue;

            List<int> topography = CalculateGroundTopography(center, CampsiteSurveyWidth);

            int previousTopography = topography[0];
            int totalHeightChanges = 0;
            float maximumLocalBumpiness = 0f;
            for (int i = 1; i < topography.Count - 1; i++)
            {
                // Calculate how bumpy the current point is relative to its two neighbors.
                int leftTopography = topography[i - 1];
                int centerTopography = topography[i];
                int rightTopography = topography[i + 1];
                float localBumpiness = MathF.Max(Abs(leftTopography), Abs(rightTopography)) - Abs(centerTopography);

                if (topography[i] != previousTopography)
                {
                    totalHeightChanges++;
                    previousTopography = topography[i];
                }

                maximumLocalBumpiness = MathF.Max(maximumLocalBumpiness, localBumpiness);
            }

            // Reject positions that are too bumpy.
            float averageBumpiness = topography.Average(t => Abs(t));
            float maximumDisrepancy = Abs(topography.Max() - topography.Min());
            float leniencyCoefficient = Lerp(1f, 4.2f, InverseLerp(0f, 4800f, tries).Squared());
            if (averageBumpiness >= leniencyCoefficient * 1.5f || maximumLocalBumpiness >= leniencyCoefficient * 3f || maximumDisrepancy >= leniencyCoefficient * 4f || totalHeightChanges >= leniencyCoefficient * 5f)
                continue;

            // Reject positions that contain tiles indictative of a hostile biome.
            if (AnyHostileBiomeTiles(center))
                continue;

            // Reject positions that would place a telescope in some awkward spot.
            if (PerformTelescopeCheck(center) && tries <= 900)
                continue;

            return center;
        }

        return Point.Zero;
    }

    /// <summary>
    /// Determines whether a given prospective area is fit for placement based on where a telescope might be placed, rejecting positions that would place it in a hole.
    /// </summary>
    /// <param name="center"></param>
    /// <returns></returns>
    public static bool PerformTelescopeCheck(Point center)
    {
        for (int dx = 34; dx < 85; dx++)
        {
            Point telescopePosition = FindGroundVertical(new(center.X - dx, center.Y));
            Vector2 checkStart = telescopePosition.ToWorldCoordinates(8f, -32f);
            Vector2 checkEndHorizontal = checkStart + new Vector2(-300f, -16f);
            Vector2 checkEndDiagonal = checkStart + new Vector2(-300f, -300f);
            if (!Collision.CanHitLine(checkStart, 16, 16, checkEndHorizontal, 16, 16) || !Collision.CanHitLine(checkStart, 16, 16, checkEndDiagonal, 16, 16))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Calculates the topography of the ground at a given point, returning a set of listed offsets.
    /// </summary>
    /// <param name="start">The center point to perform topography calculations relative to.</param>
    /// <param name="width">The width to survery.</param>
    public static List<int> CalculateGroundTopography(Point start, int width)
    {
        List<int> topography = new List<int>(width);
        for (int i = width / -2; i < width / 2; i++)
        {
            Point samplePoint = new Point(start.X + i, start.Y);
            Point groundPosition = FindGroundVertical(samplePoint);
            topography.Add(groundPosition.Y - start.Y);
        }

        return topography;
    }

    /// <summary>
    /// Determines whether there are any crimson, corruption, or dungeon tiles in a given area by sparsely sampling tiles in the area.
    /// </summary>
    /// <param name="samplePoint">The base place to search.</param>
    public static bool AnyHostileBiomeTiles(Point samplePoint)
    {
        for (int tries = 0; tries < 150; tries++)
        {
            Tile sampleTile = Main.tile[samplePoint.X + WorldGen.genRand.Next(-CampsiteSurveyWidth / 2, CampsiteSurveyWidth / 2), samplePoint.Y + WorldGen.genRand.Next(10)];
            if (sampleTile.HasUnactuatedTile && (TileID.Sets.Crimson[sampleTile.TileType] || TileID.Sets.Corrupt[sampleTile.TileType] || TileID.Sets.DungeonBiome[sampleTile.TileType] >= 1))
                return true;

            // Reject tiles that are player-made, such as doors.
            if (TileID.Sets.HousingWalls[sampleTile.TileType])
                return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to place a given tile at a given position.
    /// </summary>
    /// <param name="position">The place at which the tile should be placed.</param>
    /// <param name="tileID">The tile ID.</param>
    /// <param name="style">The tile's style frame.</param>
    /// <param name="direction">The tile's direction.</param>
    /// <param name="clearWater">Whether nearby water should be cleared.</param>
    public static bool TryToPlaceTile(Point position, int tileID, int style = 0, int direction = -1, bool clearWater = false)
    {
        WorldGen.PlaceObject(position.X, position.Y, tileID, false, style, 0, -1, direction);
        bool successful = Main.tile[position].TileType == tileID;

        if (clearWater && Main.tile[position].LiquidAmount >= 1)
        {
            for (int dx = -56; dx < 56; dx++)
            {
                for (int dy = -1; dy < 6; dy++)
                    Main.tile[new Point(position.X + dx, position.Y - dy)].LiquidAmount = 0;
            }

            Liquid.worldGenTilesIgnoreWater(true);
            Liquid.QuickWater(3);
            WorldGen.WaterCheck();

            Liquid.quickSettle = true;

            for (int i = 0; i < 10; i++)
            {
                int maxLiquid = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;
                int m = maxLiquid * 5;
                double maxLiquidDifferencePercentage = 0D;
                while (Liquid.numLiquid > 0)
                {
                    m--;
                    if (m < 0)
                        break;

                    double liquidDifferencePercentage = (maxLiquid - Liquid.numLiquid - LiquidBuffer.numLiquidBuffer) / (double)maxLiquid;
                    if (Liquid.numLiquid + LiquidBuffer.numLiquidBuffer > maxLiquid)
                        maxLiquid = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;

                    if (liquidDifferencePercentage > maxLiquidDifferencePercentage)
                        maxLiquidDifferencePercentage = liquidDifferencePercentage;

                    Liquid.UpdateLiquid();
                }
                WorldGen.WaterCheck();
            }
            Liquid.quickSettle = false;
            Liquid.worldGenTilesIgnoreWater(false);
        }

        return successful;
    }

    /// <summary>
    /// Places a foundation for Solyn's campsite.
    /// </summary>
    public static Point GenerateGround(Point center)
    {
        center.Y -= 2;

        // Determine which tile should be placed based on surrounding tiles.
        int tileToPlace = TileID.Dirt;
        for (int dx = -12; dx < 12; dx++)
        {
            Tile tile = Main.tile[center.X + dx, center.Y + WorldGen.genRand.Next(3, 10)];
            if (tile.TileType == TileID.SnowBlock || tile.TileType == TileID.IceBlock)
            {
                tileToPlace = TileID.SnowBlock;
                break;
            }
            if (tile.TileType == TileID.Sand)
            {
                tileToPlace = TileID.Sand;
                break;
            }
            if (tile.TileType == TileID.Mud || tile.TileType == TileID.JungleGrass)
            {
                tileToPlace = TileID.Mud;
                break;
            }
        }

        for (int dx = -40; dx < 40; dx++)
        {
            Main.tile[center.X + dx, center.Y].Get<TileWallWireStateData>().Slope = SlopeType.Solid;

            int y = center.Y + 1;
            Tile tile = Main.tile[center.X + dx, y];

            while (!tile.HasTile || !Main.tileSolid[tile.TileType])
            {
                WorldGen.KillTile(center.X + dx, y, noItem: true);
                WorldGen.PlaceTile(center.X + dx, y, tileToPlace);

                y++;
                tile = Main.tile[center.X + dx, y];
                tile.Get<TileWallWireStateData>().Slope = SlopeType.Solid;
                tile.Get<TileWallWireStateData>().IsHalfBlock = false;

                WorldGen.TileFrame(center.X + dx, y);
            }
        }

        return center;
    }

    /// <summary>
    /// Attempts to place Solyn's campfire at a given position.
    /// </summary>
    /// <param name="center">The center of Solyn's camp site.</param>
    public static void PlaceCampfire(Point center)
    {
        for (int dx = -8; dx < 8; dx++)
        {
            Point campfirePosition = FindGroundVertical(new(center.X + dx, center.Y));

            // Destroy piles and trees if they're in the way.
            if (Main.tile[new Point(campfirePosition.X, campfirePosition.Y - 1)].TileType == TileID.SmallPiles)
                WorldGen.KillTile(campfirePosition.X, campfirePosition.Y - 1, noItem: true);
            for (int dy = 0; dy < 8; dy++)
            {
                Point treeCheckPosition = new Point(campfirePosition.X, campfirePosition.Y + dy);
                if ((Main.tile[treeCheckPosition].TileType == TileID.Trees || Main.tile[treeCheckPosition].TileType == TileID.Cactus || Main.tile[treeCheckPosition].TileType == TileID.PalmTree) && Abs(dx) <= 2f)
                    WorldGen.KillTile(treeCheckPosition.X, treeCheckPosition.Y, noItem: true);
            }

            Point leftChairPosition = FindGroundVertical(new(campfirePosition.X - 4, campfirePosition.Y));
            Point rightChairPosition = FindGroundVertical(new(campfirePosition.X + 4, campfirePosition.Y));
            if ((int)(Round(leftChairPosition.Y + rightChairPosition.Y + campfirePosition.Y) / 3f) != campfirePosition.Y)
                continue;

            if (TryToPlaceTile(campfirePosition, ModContent.TileType<StarlitCampfireTile>(), 0, -1, true))
            {
                for (int i = 2; i <= 4; i++)
                {
                    WorldGen.KillTile(campfirePosition.X - i, campfirePosition.Y, noItem: true);
                    WorldGen.KillTile(campfirePosition.X + i, campfirePosition.Y, noItem: true);
                }

                WorldGen.KillTile(leftChairPosition.X, leftChairPosition.Y, noItem: true);
                WorldGen.KillTile(rightChairPosition.X, rightChairPosition.Y, noItem: true);

                TryToPlaceTile(leftChairPosition, TileID.Chairs, 27, 1, true);
                TryToPlaceTile(rightChairPosition, TileID.Chairs, 27, -1, true);
                break;
            }
        }
    }

    /// <summary>
    /// Attempts to place Solyn's flag at a given position.
    /// </summary>
    /// <param name="center">The center of Solyn's camp site.</param>
    public static void PlaceFlag(Point center)
    {
        int flagID = ModContent.TileType<SolynFlagTile>();
        for (int dx = 14; dx < 45; dx++)
        {
            Point flagPosition = FindGroundVertical(new(center.X + dx, center.Y));
            if (TryToPlaceTile(flagPosition, flagID, 0, -1, true))
            {
                TileEntity.PlaceEntityNet(flagPosition.X, flagPosition.Y - SolynFlagTile.Height + 1, ModContent.TileEntityType<TESolynFlag>());
                break;
            }
        }
    }

    private static void PlaceTent_ClearTilesIfNecessary(Point point)
    {
        // Destroy piles and trees if they're in the way.
        if (Main.tile[new Point(point.X, point.Y - 1)].TileType == TileID.SmallPiles)
            WorldGen.KillTile(point.X, point.Y - 1, noItem: true);
        for (int dy = 0; dy < 8; dy++)
        {
            if (Main.tile[point].TileType == TileID.Trees || Main.tile[point].TileType == TileID.Cactus || Main.tile[point].TileType == TileID.PalmTree)
                WorldGen.KillTile(point.X, point.Y, noItem: true);
            point.Y++;
        }
    }

    /// <summary>
    /// Attempts to place Solyn's tent at a given position.
    /// </summary>
    /// <param name="center">The center of Solyn's camp site.</param>
    public static void PlaceTent(Point center)
    {
        for (int dx = 4; dx < 56; dx++)
        {
            PlaceTent_ClearTilesIfNecessary(new(center.X - dx, center.Y));
            PlaceTent_ClearTilesIfNecessary(new(center.X + dx, center.Y));
        }

        int tentID = ModContent.TileType<SolynTent>();
        for (int dx = 5; dx < 50; dx++)
        {
            Point tentPosition = FindGroundVertical(new(center.X + dx, center.Y));
            if (TryToPlaceTile(tentPosition, tentID, 0, -1, true))
            {
                TentPosition = tentPosition.ToWorldCoordinates(8f, -8f);
                TileObjectData.CallPostPlacementPlayerHook(tentPosition.X, tentPosition.Y, tentID, 0, -1, 0, default);
                break;
            }

            tentPosition = FindGroundVertical(new(center.X - dx, center.Y));
            if (TryToPlaceTile(tentPosition, tentID, 0, -1, true))
            {
                TentPosition = tentPosition.ToWorldCoordinates(8f, -8f);
                TileObjectData.CallPostPlacementPlayerHook(tentPosition.X, tentPosition.Y, tentID, 0, -1, 0, default);
                break;
            }
        }
    }

    /// <summary>
    /// Attempts to place Solyn's telescope at a given position.
    /// </summary>
    /// <param name="center">The center of Solyn's camp site.</param>
    public static void PlaceTelescope(Point center)
    {
        int telescopeID = ModContent.TileType<SolynTelescopeTile>();
        for (int dx = 34; dx < 85; dx++)
        {
            Point telescopePosition = FindGroundVertical(new(center.X - dx, center.Y));
            if (TryToPlaceTile(telescopePosition, telescopeID, 0, -1, true))
            {
                TelescopePosition = telescopePosition;

                TileObjectData.CallPostPlacementPlayerHook(telescopePosition.X, telescopePosition.Y, telescopeID, 0, -1, 0, default);
                return;
            }
        }

        // Desperation.
        for (int dx = 10; dx < 34; dx++)
        {
            Point telescopePosition = FindGroundVertical(new(center.X - dx, center.Y));
            if (TryToPlaceTile(telescopePosition, telescopeID, 0, -1, true))
            {
                TelescopePosition = telescopePosition;

                TileObjectData.CallPostPlacementPlayerHook(telescopePosition.X, telescopePosition.Y, telescopeID, 0, -1, 0, default);
                return;
            }
        }
    }

    /// <summary>
    /// Attempts to generate Solyn's camp site somewhere in the outer parts of the world.
    /// </summary>
    private static void Generate()
    {
        if (generatingAlready)
            return;

        generatingAlready = true;

        Point center = FindGenerationSpot();

        // Should never happen, but just in case??
        if (center == Point.Zero)
            return;

        center = GenerateGround(center);

        CampSitePosition = center.ToWorldCoordinates();

        PlaceCampfire(center);
        PlaceFlag(center);
        PlaceTent(center);
        PlaceTelescope(center);

        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            NetMessage.SendTileSquare(-1, center.X - 40, center.Y - 40, 80, 80);
            NetMessage.SendData(MessageID.WorldData);
        }

        generatingAlready = false;
    }

    /// <summary>
    /// Attempts to generate Solyn's camp site somewhere in the outer parts of the world on a new thread.
    /// </summary>
    public static void GenerateOnNewThread() => new Thread(Generate).Start();

    public override void OnModLoad()
    {
        On_WorldGen.meteor += DisallowMeteorsDestroyingTheCampsite;
        On_Player.PlaceThing_Tiles_PlaceIt += DisableBlockPlacement;
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += DisableGrassBreakage;
        On_WorldGen.SpawnFallingBlockProjectile += PreventSandFromFalling;
        GlobalTileEventHandlers.IsTileUnbreakableEvent += MakeCampsiteUnbreakable;
        GlobalNPCEventHandlers.EditSpawnRateEvent += DisableSpawnsNearCampsite;
        GlobalTileEventHandlers.NearbyEffectsEvent += MakeTombsDisappearInProtectedSpot;
    }

    private bool PreventSandFromFalling(On_WorldGen.orig_SpawnFallingBlockProjectile orig, int i, int j, Tile tileCache, Tile tileTopCache, Tile tileBottomCache, int type)
    {
        if (generatingAlready)
            return false;

        return orig(i, j, tileCache, tileTopCache, tileBottomCache, type);
    }

    private TileObject DisableBlockPlacement(On_Player.orig_PlaceThing_Tiles_PlaceIt orig, Player self, bool newObjectType, TileObject data, int tileToCreate)
    {
        Point point = new Point(Player.tileTargetX, Player.tileTargetY);
        if (MakeCampsiteUnbreakable(point.X, point.Y, tileToCreate))
            return data;

        return orig(self, newObjectType, data, tileToCreate);
    }

    private static void DisableGrassBreakage(On_Player.orig_ItemCheck_UseMiningTools_ActuallyUseMiningTool orig, Player self, Item sItem, out bool canHitWalls, int x, int y)
    {
        canHitWalls = false;
        if (MakeCampsiteUnbreakable(x, y, Framing.GetTileSafely(x, y).TileType))
            return;

        orig(self, sItem, out canHitWalls, x, y);
    }

    private static bool MakeCampsiteUnbreakable(int x, int y, int type)
    {
        Vector2 worldPosition = new Point(x, y).ToWorldCoordinates();
        bool inProtectionRadius = worldPosition.WithinRange(CampSitePosition, UnbreakableRadius);
        if (!inProtectionRadius)
            return false;

        int[] protectedTileIDs = new int[]
        {
            ModContent.TileType<SolynTent>(),
            ModContent.TileType<SolynFlagTile>(),
            ModContent.TileType<StarlitCampfireTile>(),
            TileID.Chairs,
        };
        if (protectedTileIDs.Contains(type))
            return true;

        return y >= CampSitePosition.Y / 16f - 1f;
    }

    private static void DisableSpawnsNearCampsite(Player player, ref int spawnRate, ref int maxSpawns)
    {
        if (CampSitePosition != Vector2.Zero && player.WithinRange(CampSitePosition, UnbreakableRadius * 1.2f))
        {
            spawnRate = int.MaxValue;
            maxSpawns = 0;
        }
    }

    private void MakeTombsDisappearInProtectedSpot(int x, int y, int type, bool closer)
    {
        if (type == TileID.Tombstones && MakeCampsiteUnbreakable(x, y, type))
            Main.tile[x, y].Get<TileWallWireStateData>().HasTile = false;
    }

    public override void OnWorldLoad()
    {
        CampSitePosition = Vector2.Zero;
        TelescopePosition = Point.Zero;
        TentPosition = Vector2.Zero;
    }

    public override void OnWorldUnload()
    {
        CampSitePosition = Vector2.Zero;
        TelescopePosition = Point.Zero;
        TentPosition = Vector2.Zero;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["CampSitePositionX"] = CampSitePosition.X;
        tag["CampSitePositionY"] = CampSitePosition.Y;
        tag["TelescopePositionX"] = TelescopePosition.X;
        tag["TelescopePositionY"] = TelescopePosition.Y;
        tag["TentPositionX"] = TentPosition.X;
        tag["TentPositionY"] = TentPosition.Y;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        Vector2 campPosition = Vector2.Zero;
        if (tag.TryGet("CampSitePositionX", out float campX))
            campPosition.X = campX;
        if (tag.TryGet("CampSitePositionY", out float campY))
            campPosition.Y = campY;
        CampSitePosition = campPosition;

        Point telescopePosition = Point.Zero;
        if (tag.TryGet("TelescopePositionX", out int telescopeX))
            telescopePosition.X = telescopeX;
        if (tag.TryGet("TelescopePositionY", out int telescopeY))
            telescopePosition.Y = telescopeY;
        TelescopePosition = telescopePosition;

        Vector2 tentPosition = Vector2.Zero;
        if (tag.TryGet("TentPositionX", out float tentX))
            tentPosition.X = tentX;
        if (tag.TryGet("TentPositionY", out float tentY))
            tentPosition.Y = tentY;
        TentPosition = tentPosition;
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.WriteVector2(CampSitePosition);
        writer.WriteVector2(TelescopePosition.ToVector2());
        writer.WriteVector2(TentPosition);
    }

    public override void NetReceive(BinaryReader reader)
    {
        CampSitePosition = reader.ReadVector2();
        TelescopePosition = reader.ReadVector2().ToPoint();
        TentPosition = reader.ReadVector2();
    }

    private bool DisallowMeteorsDestroyingTheCampsite(On_WorldGen.orig_meteor orig, int i, int j, bool ignorePlayers)
    {
        if (CampSitePosition.X != 0f && Distance(i, CampSitePosition.X / 16f) <= MinMeteorDistance)
            return false;

        return orig(i, j, ignorePlayers);
    }
}
