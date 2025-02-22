using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Tiles.GenesisComponents.Seedling;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Metadata;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Tiles.GenesisComponents;

public class GenesisGrass : ModTile
{
    /// <summary>
    /// The radius at which grass can be converted to genesis grass when near a genesis instance.
    /// </summary>
    public static float ConversionRadius => 54f;

    public override string Texture => GetAssetPath("Content/Tiles/GenesisComponents", Name);

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileBrick[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Grass"]);

        // Merge with dirt and grass.
        Main.tileMerge[Type][TileID.Dirt] = true;
        Main.tileMerge[TileID.Dirt][Type] = true;
        Main.tileMerge[Type][TileID.Grass] = true;
        Main.tileMerge[TileID.Grass][Type] = true;

        DustType = TileID.Grass;
        RegisterItemDrop(ItemID.DirtBlock);

        AddMapEntry(new Color(174, 23, 51));

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.Conversion.Grass[Type] = true;

        // Grass framing.
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = TileID.Dirt;
        TileID.Sets.CanBeDugByShovel[Type] = true;

        GlobalTileEventHandlers.RandomUpdateEvent += ConvertGrassNearGenesis;
    }

    private void ConvertGrassNearGenesis(int x, int y, int type)
    {
        if (!TileID.Sets.Grass[type])
            return;

        Point point = new Point(x, y);
        Point? nearestGenesisPoint = GrowingGenesisRenderSystem.NearestGenesisPoint(point);
        bool leftOfGenesis = false;
        float distanceToGenesis = 10000f;
        if (nearestGenesisPoint is not null)
        {
            distanceToGenesis = point.ToVector2().Distance(nearestGenesisPoint.Value.ToVector2());
            leftOfGenesis = point.X < nearestGenesisPoint.Value.X;
        }

        bool withinConversionDistance = distanceToGenesis <= ConversionRadius;
        bool isGenesisGrass = type == Type;
        bool positionCanConvertToGenesisGrass = CanBeConvertedToGenesisGrass(point, leftOfGenesis, distanceToGenesis, out bool convertFromLeft, out bool convertFromRight);
        bool convertToGenesisGrass = !isGenesisGrass && withinConversionDistance && positionCanConvertToGenesisGrass;
        bool convertFromGenesisGrass = isGenesisGrass && !withinConversionDistance && Framing.GetTileSafely(point).Get<GenesisGrassMergeData>().Unconverted;

        if (convertToGenesisGrass)
        {
            Main.tile[x, y].TileType = Type;
            WorldGen.TileFrame(x, y);
            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendTileSquare(-1, x, y);
        }

        if (convertFromGenesisGrass)
        {
            Main.tile[x, y].TileType = TileID.Grass;
            WorldGen.TileFrame(x, y);
            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendTileSquare(-1, x, y);
        }

        // Handle spreading.
        if (isGenesisGrass)
        {
            ref GenesisGrassMergeData mergeData = ref Main.tile[x, y].Get<GenesisGrassMergeData>();
            mergeData.LeftConversionInterpolant = Saturate(mergeData.LeftConversionInterpolant + convertFromLeft.ToDirectionInt() * 0.045f);
            mergeData.RightConversionInterpolant = Saturate(mergeData.RightConversionInterpolant + convertFromRight.ToDirectionInt() * 0.045f);
        }
    }

    private static bool CanBeConvertedToGenesisGrass(Point point, bool leftOfGenesis, float distanceToGenesis, out bool convertFromLeft, out bool convertFromRight)
    {
        convertFromLeft = false;
        convertFromRight = false;

        int grassID = ModContent.TileType<GenesisGrass>();
        int genesisID = ModContent.TileType<GenesisTile>();
        Tile left = Framing.GetTileSafely(point.X - 1, point.Y);
        Tile right = Framing.GetTileSafely(point.X + 1, point.Y);

        // If a Genesis instance is directly above, grass can be converted directly on both sides.
        Tile top = Framing.GetTileSafely(point.X, point.Y - 1);
        if (top.HasTile && top.TileType == genesisID)
        {
            convertFromLeft = true;
            convertFromRight = true;
            return true;
        }

        // If the left tile has converted on its right side, it is ready to convert.
        bool convert = false;
        if (left.HasTile && left.TileType == grassID && left.Get<GenesisGrassMergeData>().RightConversionInterpolant >= 0.7f)
        {
            convertFromLeft = true;
            convert = true;
        }

        // If the right tile has converted on its left side, it is ready to convert.
        if (right.HasTile && right.TileType == grassID && right.Get<GenesisGrassMergeData>().LeftConversionInterpolant >= 0.7f)
        {
            convertFromRight = true;
            convert = true;
        }

        // Convert evenly on both sides if the top or bottom tiles are grass.
        Tile bottom = Framing.GetTileSafely(point.X, point.Y + 1);
        if ((top.TileType == grassID && top.HasTile) || (bottom.TileType == grassID && bottom.HasTile))
        {
            convertFromLeft = true;
            convertFromRight = true;
            convert = true;
        }

        // Continue converting if one side has already converted completely.
        GenesisGrassMergeData mergeData = Framing.GetTileSafely(point.X, point.Y).Get<GenesisGrassMergeData>();
        if (mergeData.LeftConversionInterpolant >= 1f)
        {
            convertFromRight = true;
            convert = true;
        }
        if (mergeData.RightConversionInterpolant >= 1f)
        {
            convertFromLeft = true;
            convert = true;
        }

        // Ensure that tiles at the very end of the Genesis' influence zone fade out smoothly, rather than creating an abrupt grass seam.
        if (distanceToGenesis >= ConversionRadius - 1f && distanceToGenesis <= ConversionRadius)
        {
            if (leftOfGenesis)
                convertFromLeft = false;
            else
                convertFromRight = false;
        }

        return convert;
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        Point point = new Point(i, j);
        Point? nearestGenesisPoint = GrowingGenesisRenderSystem.NearestGenesisPoint(point);
        float distanceToGenesis = 10000f;
        if (nearestGenesisPoint is not null)
            distanceToGenesis = point.ToVector2().Distance(nearestGenesisPoint.Value.ToVector2());

        bool shouldVanish = distanceToGenesis > ConversionRadius;
        if (shouldVanish)
        {
            ref GenesisGrassMergeData mergeData = ref Main.tile[i, j].Get<GenesisGrassMergeData>();
            mergeData.LeftConversionInterpolant = Saturate(mergeData.LeftConversionInterpolant - 0.003f);
            mergeData.RightConversionInterpolant = Saturate(mergeData.RightConversionInterpolant - 0.003f);

            if (mergeData.LeftConversionInterpolant <= 0f && mergeData.RightConversionInterpolant <= 0f)
            {
                Main.tile[i, j].TileType = TileID.Grass;
                WorldGen.TileFrame(i, j);
                if (Main.netMode != NetmodeID.SinglePlayer)
                    NetMessage.SendTileSquare(-1, i, j);
            }
        }
    }

    public override void RandomUpdate(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        Tile up = Main.tile[i, j - 1];
        Tile up2 = Main.tile[i, j - 2];
        if (WorldGen.genRand.NextBool(60) && !up.HasTile && !up2.HasTile && !(up.LiquidAmount > 0 && up2.LiquidAmount > 0) && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
        {
            int variant = Main.rand.Next(GenesisGrassBlades.VariantCount);
            WorldGen.PlaceObject(i, j - 1, ModContent.TileType<GenesisGrassBlades>(), true, 0, 0, -1, Main.rand.NextFromList(-1, 1));
            for (int k = 1; k <= 2; k++)
                Main.tile[i, j - k].TileFrameX = (short)(variant * 18);
        }
    }

    public override void NumDust(int i, int j, bool fail, ref int Type) => Type = fail ? 1 : 3;

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (fail && !effectOnly)
            Main.tile[i, j].TileType = TileID.Dirt;
        if (!effectOnly)
            Main.tile[i, j].Get<GenesisGrassMergeData>().Clear();
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        GenesisGrassMergeData mergeData = tile.Get<GenesisGrassMergeData>();
        Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

        Color baseColor = Lighting.GetColor(i, j);
        Color leftColor = baseColor * mergeData.LeftConversionInterpolant;
        Color rightColor = baseColor * mergeData.RightConversionInterpolant;

        Texture2D regularGrassTexture = TextureAssets.Tile[TileID.Grass].Value;
        Texture2D texture = TextureAssets.Tile[Type].Value;

        Rectangle frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);

        VertexColors vertexColors = new VertexColors(leftColor, rightColor, rightColor, leftColor);
        DrawSlopedTile(i, j, regularGrassTexture, frame, new(baseColor), drawOffset);
        DrawSlopedTile(i, j, texture, frame, vertexColors, drawOffset);

        return false;
    }

    public static void DrawSlopedTile(int i, int j, Texture2D texture, Rectangle? sourceRectangle, VertexColors drawColor, Vector2 positionOffset)
    {
        Tile tile = Main.tile[i, j];

        int tileFrameX = tile.TileFrameX;
        int tileFrameY = tile.TileFrameY;

        int width = 16;
        int height = 16;

        if (sourceRectangle != null)
        {
            tileFrameX = sourceRectangle.Value.X;
            tileFrameY = sourceRectangle.Value.Y;
        }

        int iX16 = i * 16;
        int jX16 = j * 16;
        Vector2 location = new Vector2(iX16, jX16);

        Vector2 offsets = positionOffset - Main.screenPosition;
        Vector2 drawCoordinates = location + offsets;
        if ((tile.Slope == 0 && !tile.IsHalfBlock) || (Main.tileSolid[tile.TileType] && Main.tileSolidTop[tile.TileType])) //second one should be for platforms
        {
            Main.tileBatch.Draw(texture, drawCoordinates, new Rectangle(tileFrameX, tileFrameY, width, height), drawColor, Vector2.Zero, 1f, SpriteEffects.None);
        }
        else if (tile.IsHalfBlock)
        {
            Main.tileBatch.Draw(texture, new Vector2(drawCoordinates.X, drawCoordinates.Y + 8), new Rectangle(tileFrameX, tileFrameY, width, 8), drawColor, Vector2.Zero, 1f, SpriteEffects.None);
        }
        else
        {
            byte b = (byte)tile.Slope;
            Rectangle TileFrame;
            Vector2 drawPos;
            if (b == 1 || b == 2)
            {
                int length;
                int height2;
                for (int a = 0; a < 8; ++a)
                {
                    int aX2 = a * 2;
                    if (b == 2)
                    {
                        length = 16 - aX2 - 2;
                        height2 = 14 - aX2;
                    }
                    else
                    {
                        length = aX2;
                        height2 = 14 - length;
                    }

                    TileFrame = new Rectangle(tileFrameX + length, tileFrameY, 2, height2);
                    drawPos = new Vector2(iX16 + length, jX16 + aX2) + offsets;
                    Main.tileBatch.Draw(texture, drawPos, TileFrame, drawColor, Vector2.Zero, 1f, SpriteEffects.None);
                }

                TileFrame = new Rectangle(tileFrameX, tileFrameY + 14, 16, 2);
                drawPos = new Vector2(iX16, jX16 + 14) + offsets;
                Main.tileBatch.Draw(texture, drawPos, TileFrame, drawColor, Vector2.Zero, 1f, SpriteEffects.None);
            }
            else
            {
                int length;
                int height2;
                for (int a = 0; a < 8; ++a)
                {
                    int aX2 = a * 2;
                    if (b == 3)
                    {
                        length = aX2;
                        height2 = 16 - length;
                    }
                    else
                    {
                        length = 16 - aX2 - 2;
                        height2 = 16 - aX2;
                    }

                    TileFrame = new Rectangle(tileFrameX + length, tileFrameY + 16 - height2, 2, height2);
                    drawPos = new Vector2(iX16 + length, jX16) + offsets;
                    Main.tileBatch.Draw(texture, drawPos, TileFrame, drawColor, Vector2.Zero, 1f, SpriteEffects.None);
                }

                drawPos = new Vector2(iX16, jX16) + offsets;
                TileFrame = new Rectangle(tileFrameX, tileFrameY, 16, 2);
                Main.tileBatch.Draw(texture, drawPos, TileFrame, drawColor, Vector2.Zero, 1f, SpriteEffects.None);
            }
        }
        // Contribuited by Vortex
    }
}
