using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Utilities;

public static partial class Utilities
{
    /// <summary>
    /// Finds a given tile entity at a given tile position.
    /// </summary>
    /// <typeparam name="T">The type of tile entity to search for.</typeparam>
    /// <param name="i">The X tile position to search at.</param>
    /// <param name="j">The Y tile position to search at.</param>
    /// <param name="width">The width of the tile.</param>
    /// <param name="height">The height of the tile.</param>
    public static T? FindTileEntity<T>(int i, int j, int width, int height) where T : ModTileEntity
    {
        // Find the top left corner of the FrameImportant tile that the player clicked on in the world.
        Tile t = Main.tile[i, j];
        int left = i - t.TileFrameX % (width * 18) / 18;
        int top = j - t.TileFrameY % (height * 18) / 18;

        int tileEntityID = ModContent.GetInstance<T>().Type;
        bool exists = TileEntity.ByPosition.TryGetValue(new Point16(left, top), out TileEntity? te);
        return exists && te!.type == tileEntityID ? (T)te : null;
    }

    // Method in Calamity that's internal for some reason.
    public static void DrawTileWithSlope(int i, int j, Texture2D texture, Rectangle? sourceRectangle, Color drawColor, Vector2 positionOffset, bool overrideTileFrame = false)
    {
        Tile tile = Main.tile[i, j];

        int TileFrameX = tile.TileFrameX;
        int TileFrameY = tile.TileFrameY;

        if (overrideTileFrame)
        {
            TileFrameX = 0;
            TileFrameY = 0;
        }

        int width = 16;
        int height = 16;

        if (sourceRectangle != null)
        {
            TileFrameX = ((Rectangle)sourceRectangle).X;
            TileFrameY = ((Rectangle)sourceRectangle).Y;
        }

        int iX16 = i * 16;
        int jX16 = j * 16;
        Vector2 location = new Vector2(iX16, jX16);

        Vector2 offsets = positionOffset - Main.screenPosition;
        Vector2 drawCoordinates = location + offsets;
        if ((tile.Slope == 0 && !tile.IsHalfBlock) || (Main.tileSolid[tile.TileType] && Main.tileSolidTop[tile.TileType])) //second one should be for platforms
        {
            Main.spriteBatch.Draw(texture, drawCoordinates, new Rectangle(TileFrameX, TileFrameY, width, height), drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
        else if (tile.IsHalfBlock)
        {
            Main.spriteBatch.Draw(texture, new Vector2(drawCoordinates.X, drawCoordinates.Y + 8), new Rectangle(TileFrameX, TileFrameY, width, 8), drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
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

                    TileFrame = new Rectangle(TileFrameX + length, TileFrameY, 2, height2);
                    drawPos = new Vector2(iX16 + length, jX16 + aX2) + offsets;
                    Main.spriteBatch.Draw(texture, drawPos, TileFrame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                }

                TileFrame = new Rectangle(TileFrameX, TileFrameY + 14, 16, 2);
                drawPos = new Vector2(iX16, jX16 + 14) + offsets;
                Main.spriteBatch.Draw(texture, drawPos, TileFrame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
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

                    TileFrame = new Rectangle(TileFrameX + length, TileFrameY + 16 - height2, 2, height2);
                    drawPos = new Vector2(iX16 + length, jX16) + offsets;
                    Main.spriteBatch.Draw(texture, drawPos, TileFrame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                }

                drawPos = new Vector2(iX16, jX16) + offsets;
                TileFrame = new Rectangle(TileFrameX, TileFrameY, 16, 2);
                Main.spriteBatch.Draw(texture, drawPos, TileFrame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
        }
        // Contribuited by Vortex
    }
}
