using Microsoft.Xna.Framework;
using NoxusBoss.Content.Tiles;
using NoxusBoss.Content.Tiles.TileEntities;
using NoxusBoss.Content.Walls;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace NoxusBoss.Core.World.WorldGeneration;

public static class EternalGardenWorldGen
{
    public const int DirtDepth = 64;

    public const int LakeWidth = 236;

    public const int LakeMaxDepth = 35;

    // Avoid using this in loops. It's more efficient to store it in a local variable and reference that instead of calling this getter property over and over.
    public static int SurfaceTilePoint => Main.maxTilesY - DirtDepth - LakeMaxDepth;

    // How much of a magnification is performed when calculating Perlin noise for height maps. The closer to 0 this value is, the more same-y they will seem in
    // terms of direction, size, etc.
    public const float SurfaceMapMagnification = 0.0025f;

    public const int MaxGroundHeight = 18;

    // This is how many tiles it takes for the flatness interpolant to go from its maximum to its minimum.
    public const int GroundFlatnessTaperZone = 45;

    public const int TotalFlatTilesAtCenter = 25;

    public const int TotalFlatTilesAtEdge = 12;

    public const float LakeDescentSharpnessMin = 0.56f;

    public const float LakeDescentSharpnessMax = 0.72f;

    public const int MaxLakeGroundHeight = 12;

    public const int MaxFlowerHeightDescent = 11;

    // Same idea as SurfaceMapMagnification, but with respect to the lakebed instead of the surface grass.
    public const float LakeSurfaceMapMagnification = 0.004f;

    public const float ClayNoiseMagnification = 0.0075f;

    public const float StoneNoiseMagnification = 0.0033f;

    public const int MinLakeClusters = 4;

    public const int MaxLakeClusters = 7;

    public const int MinLakeClusterRadius = 5;

    public const int MaxLakeClusterRadius = 10;

    public static void Generate()
    {
        // Set the base level of dirt. Its top serves as the bottom of the lakes, with no natural way of going further down.
        GenerateDirtBase();

        // Generate both lakes.
        Rectangle leftLakeArea = new Rectangle(0, SurfaceTilePoint + 1, LakeWidth + 1, LakeMaxDepth);
        Rectangle rightLakeArea = new Rectangle(Main.maxTilesX - LakeWidth - 1, SurfaceTilePoint + 1, LakeWidth, LakeMaxDepth);
        GenerateLake(leftLakeArea, false);
        GenerateLake(rightLakeArea, true);

        // Set spawn points.
        SetInitialPlayerSpawnPoint();

        // Calculate the topography of the ground and generate height accordingly.
        int[] topography = GenerateGroundTopography(leftLakeArea.Right, rightLakeArea.Left);

        // Replace the dirt with grass where necessary.
        ReplaceDirtWithGrass();

        // Smoothen everything.
        SmoothenWorld();

        // Ensure that lake slopes have water.
        MakeLakeSlopesWatery(leftLakeArea);
        MakeLakeSlopesWatery(rightLakeArea);

        // Generate plants atop the grass.
        GeneratePlants(topography, leftLakeArea.Right);

        // Generate flowery walls atop the grass.
        GenerateFlowerWalls(topography, leftLakeArea.Right);

        // Generate water decorations and grass.
        GenerateWaterGrass();
        GenerateWaterDecorations(leftLakeArea);
        GenerateWaterDecorations(rightLakeArea);

        // Save the surface Y offset.
        WorldSaveSystem.GardenTreeSurfaceYOffset = -topography[Main.maxTilesY / 2];
    }

    public static void GenerateDirtBase()
    {
        // Self-explanatory. Just a simple rectangle of dirt that acts as the foundation for everything else.
        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int j = 0; j < DirtDepth; j++)
            {
                int y = Main.maxTilesY - j;

                Main.tile[x, y].TileType = TileID.Dirt;
                Main.tile[x, y].Get<TileWallWireStateData>().Slope = SlopeType.Solid;
                Main.tile[x, y].Get<TileWallWireStateData>().IsHalfBlock = false;
                Main.tile[x, y].Get<TileWallWireStateData>().HasTile = true;
            }
        }

        // Create a second mound of dirt in the center. This leaves the edges unchanged so that they can be occupied by the tranquil lakes.
        // In this first step the process is unnaturally rigid since there's no gradual descent into the lake or anything, but this will be addressed in
        // later generation steps.
        for (int x = LakeWidth + 1; x < Main.maxTilesX - LakeWidth; x++)
        {
            for (int j = 0; j < LakeMaxDepth; j++)
            {
                int y = Main.maxTilesY - DirtDepth - j;

                Main.tile[x, y].TileType = TileID.Dirt;
                Main.tile[x, y].Get<TileWallWireStateData>().Slope = SlopeType.Solid;
                Main.tile[x, y].Get<TileWallWireStateData>().IsHalfBlock = false;
                Main.tile[x, y].Get<TileWallWireStateData>().HasTile = true;
            }
        }
    }

    public static void GenerateLake(Rectangle area, bool right)
    {
        // Fill the currently empty rectangular box with water and flower walls.
        int flowerWallSeed = WorldGen.genRand.Next(999999999);
        for (int x = area.Left; x < area.Right; x++)
        {
            Vector2 heightMapInput = new Vector2(x, SurfaceTilePoint) * LakeSurfaceMapMagnification;
            float flowerHeightDescent = (int)(Abs(FractalBrownianMotion(heightMapInput.X, heightMapInput.Y, flowerWallSeed, 3)) * MaxFlowerHeightDescent);
            for (int y = area.Top; y < area.Bottom; y++)
            {
                if (y >= area.Top + flowerHeightDescent)
                    Main.tile[x, y].WallType = WallID.FlowerUnsafe;

                Main.tile[x, y].Get<LiquidData>().Amount = byte.MaxValue;
                Main.tile[x, y].Get<LiquidData>().LiquidType = LiquidID.Water;
                Main.tile[x, y].Get<TileWallWireStateData>().HasTile = false;
                WorldGen.SquareWallFrame(x, y);
            }
        }

        // Calculate the height topography of the lake. This will always create natural descents from the surface to the bottom of the lake.
        int heightMapSeed = WorldGen.genRand.Next(999999999);
        float descentSharpnessInterpolant = WorldGen.genRand.NextFloat(LakeDescentSharpnessMin, LakeDescentSharpnessMax);
        int[] topography = new int[area.Right - area.Left];
        for (int x = area.Left; x < area.Right; x++)
        {
            int heightIndex = x - area.Left;
            float distanceToStart = Distance(x, right ? area.Left : area.Right);
            float distanceToStartInterpolant = distanceToStart / (area.Right - area.Left);

            // Create the descent.
            int descentHeight = (int)(Pow(InverseLerp(descentSharpnessInterpolant, 0f, distanceToStartInterpolant), 0.49f) * LakeMaxDepth);

            // Apply noise to the topography. The effects of the noise diminish if the height is already very close to the surface.
            Vector2 heightMapInput = new Vector2(x, SurfaceTilePoint) * LakeSurfaceMapMagnification;
            float noiseAffectionInterpolant = InverseLerp(0.9f, 0.62f, descentHeight / (float)LakeMaxDepth, false);
            int noiseHeight = (int)(Abs(FractalBrownianMotion(heightMapInput.X, heightMapInput.Y, heightMapSeed, 3)) * MaxLakeGroundHeight * noiseAffectionInterpolant);

            // Use the height values to generate the topography.
            topography[heightIndex] = descentHeight + noiseHeight;
        }

        // Use the height map to generate the ground.
        for (int i = 0; i < topography.Length; i++)
        {
            int x = i + area.Left;
            int height = topography[i];

            for (int dy = 0; dy < height; dy++)
            {
                int y = area.Bottom - dy;
                Main.tile[x, y].TileType = TileID.Dirt;
                Main.tile[x, y].Get<TileWallWireStateData>().Slope = SlopeType.Solid;
                Main.tile[x, y].Get<TileWallWireStateData>().IsHalfBlock = false;
                Main.tile[x, y].Get<TileWallWireStateData>().HasTile = true;
                Main.tile[x, y].Get<LiquidData>().Amount = 0;
            }
        }

        // Spice up the tile variety in the lakes by replacing some of the dirt with clay and stone.
        int clayNoiseSeed = WorldGen.genRand.Next(999999999);
        int stoneNoiseSeed = WorldGen.genRand.Next(999999999);
        for (int x = area.Left; x < area.Right; x++)
        {
            for (int y = area.Top; y < area.Bottom; y++)
            {
                if (!Main.tile[x, y].HasTile)
                    continue;

                ushort newTileType = TileID.Dirt;
                float distanceToStart = Distance(x, right ? area.Left : area.Right);
                float clayNoiseInterpolant = FractalBrownianMotion(x * ClayNoiseMagnification, y * ClayNoiseMagnification, clayNoiseSeed, 3);
                float stoneNoiseInterpolant = FractalBrownianMotion(x * StoneNoiseMagnification, y * StoneNoiseMagnification, stoneNoiseSeed, 3);

                // Bias away from stone and clay if close to the shore.
                clayNoiseInterpolant *= InverseLerp(4f, 16f, distanceToStart);
                stoneNoiseInterpolant *= InverseLerp(6f, 19f, distanceToStart);

                // Choose a tile variant based on the interpolants.
                if (clayNoiseInterpolant >= 0.284f)
                    newTileType = TileID.ClayBlock;
                if (stoneNoiseInterpolant >= 0.3f)
                    newTileType = TileID.Stone;

                Main.tile[x, y].TileType = newTileType;
            }
        }

        // Create random clusters at the bottom of the lake.
        int lakeClusterCount = WorldGen.genRand.Next(MinLakeClusters, MaxLakeClusters);
        for (int i = 0; i < lakeClusterCount; i++)
        {
            int clusterRadius = WorldGen.genRand.Next(MinLakeClusterRadius, MaxLakeClusterRadius);
            int x = WorldGen.genRand.Next(area.Left + 20, area.Right - 20);
            int y = area.Bottom - topography[x - area.Left];
            ushort originalTileType = Framing.GetTileSafely(x, y).TileType;

            WorldUtils.Gen(new(x, y + clusterRadius / 2 + WorldGen.genRand.Next(-2, 3)), new Shapes.Circle(clusterRadius), Actions.Chain(new GenAction[]
            {
                new Modifiers.Blotches(),
                new Actions.PlaceTile(originalTileType),
                new Actions.PlaceWall(WallID.FlowerUnsafe),
                new Actions.SetFrames(),
                new Actions.SetLiquid(0, 0)
            }));
        }
    }

    public static int[] GenerateGroundTopography(int left, int right)
    {
        // Calculate the center point, width, and surface line.
        int width = right - left;
        int center = (left + right) / 2;
        int surfaceY = SurfaceTilePoint;

        // Use noise to determine the height topography of the ground.
        int heightMapSeed = WorldGen.genRand.Next(999999999);
        int[] topography = new int[right - left];
        for (int i = 0; i < topography.Length; i++)
        {
            // Use a flatness interpolant of 1 (meaning the result is unaffected) by default.
            float heightFlatnessInterpolant = 1f;

            // Make the height more flat at the center of the garden.
            int x = i + left;
            float distanceFromCenter = Distance(x, center);
            float distanceFromEdge = width * 0.5f - distanceFromCenter;
            if (distanceFromCenter <= TotalFlatTilesAtCenter + GroundFlatnessTaperZone)
                heightFlatnessInterpolant = InverseLerp(0f, GroundFlatnessTaperZone, distanceFromCenter - TotalFlatTilesAtCenter);

            // Make the height more flat at the edges of the garden, so that there's no awkward ledges at the lake.
            if (distanceFromEdge <= TotalFlatTilesAtEdge + GroundFlatnessTaperZone)
                heightFlatnessInterpolant = InverseLerp(0f, GroundFlatnessTaperZone, distanceFromEdge - TotalFlatTilesAtEdge);

            Vector2 heightMapInput = new Vector2(i, SurfaceTilePoint) * SurfaceMapMagnification;
            int height = (int)(Abs(FractalBrownianMotion(heightMapInput.X, heightMapInput.Y, heightMapSeed, 3)) * MaxGroundHeight * heightFlatnessInterpolant);
            topography[i] = height;
        }

        // Use the height map to generate the ground.
        for (int i = 0; i < topography.Length; i++)
        {
            int x = i + left;
            int height = topography[i];

            for (int dy = 0; dy < height; dy++)
            {
                int y = surfaceY - dy;
                Main.tile[x, y].TileType = TileID.Dirt;
                Main.tile[x, y].Get<TileWallWireStateData>().Slope = SlopeType.Solid;
                Main.tile[x, y].Get<TileWallWireStateData>().IsHalfBlock = false;
                Main.tile[x, y].Get<TileWallWireStateData>().HasTile = true;
            }
        }

        return topography;
    }

    public static void SetInitialPlayerSpawnPoint()
    {
        // Set the default spawn position for the player, right next to the left lake.
        // This is where the player appears when they've teleported with the Terminus, not where they respawn when killed by Nameless.
        // Once the player has properly entered the world and spawned already, this gets set again via the other method below.
        Main.spawnTileX = LakeWidth + 60;
        Main.spawnTileY = SurfaceTilePoint - 1;
    }

    public static void ReplaceDirtWithGrass()
    {
        int grassID = ModContent.TileType<EternalGardenGrass>();
        for (int x = LakeWidth - 5; x < Main.maxTilesX - LakeWidth + 4; x++)
        {
            for (int y = 10; y < Main.maxTilesY - 1; y++)
            {
                // The tile found is dirt. Now check if it has an exposed air or water pocket.
                if (Main.tile[x, y].TileType == TileID.Dirt)
                {
                    Tile left = Framing.GetTileSafely(x - 1, y);
                    Tile right = Framing.GetTileSafely(x + 1, y);
                    Tile top = Framing.GetTileSafely(x, y - 1);
                    Tile bottom = Framing.GetTileSafely(x, y + 1);
                    bool anyExposedAir = !left.HasTile || !right.HasTile || !top.HasTile || !bottom.HasTile;
                    if (anyExposedAir)
                        WorldGen.SpreadGrass(x, y, TileID.Dirt, grassID, false);
                }
            }
        }
    }

    public static void SmoothenWorld()
    {
        for (int x = 5; x < Main.maxTilesX - 5; x++)
        {
            for (int y = 5; y < Main.maxTilesY - 5; y++)
            {
                SlopeType oldSlope = Framing.GetTileSafely(x, y).Slope;
                Tile.SmoothSlope(x, y);
                Tile t = Framing.GetTileSafely(x, y);

                if (t.Slope != oldSlope)
                {
                    t.Get<LiquidData>().Amount = 255;

                    t = Framing.GetTileSafely(x, y - 1);
                    t.Get<LiquidData>().Amount = 255;

                    t = Framing.GetTileSafely(x, y + 1);
                    t.Get<LiquidData>().Amount = 255;
                }
            }
        }
    }

    public static void GeneratePlants(int[] topography, int left)
    {
        ushort flowerID = (ushort)ModContent.TileType<EternalFlower>();
        ushort tallFlowerID = (ushort)ModContent.TileType<EternalTallFlower>();
        WeightedRandom<ushort> plantSelector = new WeightedRandom<ushort>(WorldGen.genRand.Next());
        plantSelector.Add(flowerID, 0.3);
        plantSelector.Add(tallFlowerID, 0.2);
        plantSelector.Add((ushort)ModContent.TileType<PlantsCopy>(), 1.2);
        plantSelector.Add((ushort)ModContent.TileType<Plants2Copy>(), 0.45);
        plantSelector.Add(TileID.LargePiles2, 0.15);
        plantSelector.Add(TileID.FallenLog, 0.125);
        plantSelector.Add(TileID.DyePlants, 0.1);

        // Generate a special tree in the very center of the garden.
        PlaceGoodAppleTree(topography[Main.maxTilesX / 2]);

        // Separately place Starbearers based on how many times players have been killed by Nameless.
        // This has a hard limit so that nohitters don't litter the subworld with them.
        int starbearerCount = Utils.Clamp(WorldSaveSystem.NamelessDeityDeathCount, 0, 200);
        List<int> starbearerPositions = [];
        for (int i = 0; i < starbearerCount; i++)
        {
            int potentialX;
            do
                potentialX = WorldGen.genRand.Next(topography.Length - 16) + left + 8;
            while (starbearerPositions.Contains(potentialX));

            starbearerPositions.Add(potentialX);
        }

        // Loop through the mound's tiles and replace the dirt with grass if it's exposed to air.
        ushort previousPlantID = 0;
        int surfaceY = SurfaceTilePoint;
        for (int i = 0; i < topography.Length; i++)
        {
            int height = topography[i];
            int x = i + left;
            int y = surfaceY - height;
            bool veryCloseToCenter = Distance(x, Main.maxTilesX * 0.5f) <= 26f;
            ushort plantID = plantSelector.Get();

            // Don't place tiles at the very center, due to that being where the tree should be.
            if (veryCloseToCenter)
                continue;

            // Prevent placing special plants twice in succession.
            if (previousPlantID != flowerID && previousPlantID != tallFlowerID)
            {
                // Re-roll until the plant ID is different. This only happens 50 times at most, in case something goes wrong and would otherwise cause an infinite loop freeze.
                for (int j = 0; j < 50; j++)
                {
                    if (plantID != previousPlantID)
                        break;

                    plantID = plantSelector.Get();
                }
            }

            // Plant starbearers if in one of the locations they should appear.
            if (starbearerPositions.Contains(x))
            {
                WorldGen.KillTile(x, y);
                plantID = (ushort)ModContent.TileType<Starbearer>();
            }

            previousPlantID = plantID;

            // Certain tiles require manual selection of frames. Handle such cases.
            // The reason this is necessary is because the tile placement method will automatically determine the frame for some tiles on its own if the input is 0, but for others
            // it will just use the first frame variant universally. This inconsistent behavior is quite weird, but workable.
            int frameVariant = 0;
            switch (plantID)
            {
                // Uses the stylist strange plant variants.
                case TileID.DyePlants:
                    frameVariant = WorldGen.genRand.Next(8, 12);
                    break;

                // Uses living tree stump variants.
                case TileID.LargePiles2:
                    frameVariant = WorldGen.genRand.Next(47, 52);
                    break;
            }
            if (plantID == ModContent.TileType<EternalFlower>())
                frameVariant = WorldGen.genRand.Next(4);
            if (plantID == ModContent.TileType<EternalTallFlower>())
                frameVariant = WorldGen.genRand.Next(3);
            if (plantID == ModContent.TileType<PlantsCopy>())
                frameVariant = WorldGen.genRand.Next(44);
            if (plantID == ModContent.TileType<Plants2Copy>())
                frameVariant = WorldGen.genRand.Next(44);

            WorldGen.PlaceObject(x, y, plantID, true, frameVariant);
        }
    }

    public static void GenerateFlowerWalls(int[] topography, int left)
    {
        int surfaceY = SurfaceTilePoint;
        int noiseSeed = WorldGen.genRand.Next(1000000);
        ushort wallID = (ushort)ModContent.WallType<EternalGardenGrassWall>();
        for (int i = 0; i < topography.Length; i++)
        {
            int x = i + left;
            int y = surfaceY - topography[i];
            float edgeHeightFade = InverseLerpBump(0f, 10f, topography.Length - 11f, topography.Length - 1f, i);
            int grassHeight = (int)(Abs(FractalBrownianMotion(i * 0.008f, y * 0.02f, noiseSeed, 4) * 6f) * edgeHeightFade);

            for (int dy = 0; dy < grassHeight; dy++)
                Main.tile[x, y - dy].WallType = wallID;
        }
    }

    public static void PlaceGoodAppleTree(int surfaceHeight)
    {
        int surfaceY = SurfaceTilePoint;
        ushort wallID = (ushort)ModContent.WallType<EternalGardenGrassWall>();
        Point treePosition = new Point(Main.maxTilesX / 2, surfaceY - surfaceHeight);
        while (!Framing.GetTileSafely(treePosition.X, treePosition.Y + 1).HasTile)
            treePosition.Y++;

        // Generate grass walls behind the tree.
        for (int dx = -15; dx <= GoodAppleTree.TrunkWidth + 15; dx++)
        {
            int bottom = treePosition.Y + 2;
            int top = (int)(bottom - Convert01To010(InverseLerp(-15f, GoodAppleTree.TrunkWidth + 15f, dx)) * WorldGen.genRand.NextFloat(4f, 5.5f) - 1f);
            for (int y = top; y <= bottom; y++)
                WorldGen.PlaceWall(treePosition.X + dx - GoodAppleTree.TrunkHeight / 2, y, wallID);
        }

        // Generate the tree itself.
        for (int dx = 0; dx <= GoodAppleTree.TrunkWidth; dx++)
        {
            for (int dy = 0; dy <= GoodAppleTree.TrunkHeight; dy++)
            {
                Point localTreePosition = new Point(treePosition.X - GoodAppleTree.TrunkHeight / 2 + dx, treePosition.Y - GoodAppleTree.TrunkHeight + dy);
                Main.tile[localTreePosition].TileType = (ushort)ModContent.TileType<GoodAppleTree>();
                Main.tile[localTreePosition].Get<TileWallWireStateData>().HasTile = true;
                Main.tile[localTreePosition].Get<TileWallWireStateData>().TileFrameX = (short)(dx * 16);
                Main.tile[localTreePosition].Get<TileWallWireStateData>().TileFrameY = (short)(dy * 16);

                if (dx == 0 && dy == 0)
                    TileEntity.PlaceEntityNet(localTreePosition.X, localTreePosition.Y, ModContent.TileEntityType<TEGoodAppleTree>());
            }
        }
    }

    public static void GenerateWaterGrass()
    {
        ushort grassID = (ushort)ModContent.TileType<EternalGardenGrass>();
        for (int x = 1; x < Main.maxTilesX - 1; x++)
        {
            for (int y = 10; y < Main.maxTilesY - 10; y++)
            {
                Tile t = Framing.GetTileSafely(x, y);
                Tile above = Framing.GetTileSafely(x, y - 1);
                if (t.HasTile && t.TileType == TileID.Dirt && !above.HasTile && above.LiquidAmount >= 127)
                    Main.tile[x, y].TileType = grassID;
            }
        }
    }

    public static void GenerateWaterDecorations(Rectangle area)
    {
        // Adorn the lake with cattails and lily pads.
        int cattailCount = (area.Right - area.Left) / 7;
        int lilyPadCount = (area.Right - area.Left) / 3;
        for (int i = 0; i < cattailCount; i++)
        {
            int x = WorldGen.genRand.Next(area.Left + 9, area.Right - 9);
            int y = area.Bottom + 4;
            while (Framing.GetTileSafely(x, y).HasTile)
                y--;

            int cattailHeight = y - area.Top + WorldGen.genRand.NextBool().ToInt() + 1;
            Main.tile[x, y].TileType = TileID.Cattail;
            Main.tile[x, y].Get<TileWallWireStateData>().HasTile = true;

            for (int j = 0; j < cattailHeight; j++)
                WorldGen.GrowCatTail(x, y);
        }
        for (int i = 0; i < lilyPadCount; i++)
        {
            int x = WorldGen.genRand.Next(area.Left + 9, area.Right - 9);
            int y = area.Top + 1;
            WorldGen.PlaceTile(x, y, TileID.LilyPad);
        }
    }

    public static void MakeLakeSlopesWatery(Rectangle area)
    {
        for (int x = area.Left; x < area.Right; x++)
        {
            for (int y = area.Top - 10; y < area.Bottom + 10; y++)
            {
                Tile t = Framing.GetTileSafely(x, y);
                if (!t.HasTile)
                    continue;

                if (t.Slope == SlopeType.Solid && !t.IsHalfBlock)
                    continue;

                Tile left = Framing.GetTileSafely(x - 1, y);
                Tile right = Framing.GetTileSafely(x + 1, y);
                Tile top = Framing.GetTileSafely(x, y - 1);
                Tile bottom = Framing.GetTileSafely(x, y + 1);
                bool neighborHasWater = left.LiquidAmount >= 127 || right.LiquidAmount >= 127 || top.LiquidAmount >= 127 || bottom.LiquidAmount >= 127;
                if (neighborHasWater)
                    t.Get<LiquidData>().Amount = 255;
            }
        }
    }
}
