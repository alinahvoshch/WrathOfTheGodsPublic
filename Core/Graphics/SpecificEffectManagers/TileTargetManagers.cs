using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.SpecificEffectManagers;

[Autoload(Side = ModSide.Client)]
public class TileTargetManagers : ModSystem
{
    /// <summary>
    /// The render target that holds all tile data on the screen, both for solid and non-solid tiles.
    /// </summary>
    public static ManagedRenderTarget TileTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The render target that holds all (with the exception of slopes) liquid data on the screen, such as water and lava.
    /// </summary>
    public static ManagedRenderTarget LiquidTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The render target that holds all liquid slopes on the screen.
    /// </summary>
    public static ManagedRenderTarget LiquidSlopesTarget
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        TileTarget = new(true, ManagedRenderTarget.CreateScreenSizedTarget);
        LiquidTarget = new(true, ManagedRenderTarget.CreateScreenSizedTarget);
        LiquidSlopesTarget = new(true, ManagedRenderTarget.CreateScreenSizedTarget);
        RenderTargetManager.RenderTargetUpdateLoopEvent += RecordTiles;
        RenderTargetManager.RenderTargetUpdateLoopEvent += RecordLiquids;
    }

    private void RecordTiles()
    {
        // HUH????
        // RETRO, WHYYYYY
        if (Main.instance.blackTarget.IsDisposed || Main.instance.tileTarget.IsDisposed || Main.instance.tile2Target.IsDisposed)
            return;

        if (Main.gameMenu)
            return;

        GraphicsDevice gd = Main.instance.GraphicsDevice;
        gd.SetRenderTarget(TileTarget);
        gd.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        Vector2 drawPosition = Main.sceneTilePos - Main.screenPosition;
        Main.spriteBatch.Draw(Main.instance.blackTarget, drawPosition, Color.White);
        Main.spriteBatch.Draw(Main.instance.tileTarget, drawPosition, Color.White);

        drawPosition = Main.sceneTile2Pos - Main.screenPosition;
        Main.spriteBatch.Draw(Main.instance.tile2Target, drawPosition, Color.White);
        Main.spriteBatch.End();

        gd.SetRenderTarget(null);
    }

    private void RecordLiquids()
    {
        // ???????????????
        if (Main.waterTarget.IsDisposed || Main.instance.backWaterTarget.IsDisposed)
            return;

        if (Main.gameMenu)
            return;

        // Front layer.
        GraphicsDevice gd = Main.instance.GraphicsDevice;
        gd.SetRenderTarget(LiquidTarget);
        gd.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        Vector2 drawPosition = Main.sceneWaterPos - Main.screenPosition;
        Main.spriteBatch.Draw(Main.waterTarget, drawPosition, Color.White);
        Main.spriteBatch.End();

        // Back layer.
        if (ShaderManager.GetFilter("NoxusBoss.CosmicWaterShader").Opacity > 0f)
        {
            gd.SetRenderTarget(LiquidSlopesTarget);
            gd.Clear(Color.Transparent);

            Main.tileBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            int firstTileX = (int)(Main.screenPosition.X - 100f) / 16;
            int lastTileX = (int)(Main.screenPosition.X + Main.screenWidth + 100f) / 16;
            int firstTileY = (int)(Main.screenPosition.Y - 100f) / 16;
            int lastTileY = (int)(Main.screenPosition.Y + Main.screenHeight + 100f) / 16;
            for (int y = firstTileY; y < lastTileY + 4; y++)
            {
                for (int x = firstTileX - 2; x < lastTileX + 2; x++)
                {
                    Tile tile = Framing.GetTileSafely(x, y);
                    bool waterNearby = tile.LiquidAmount >= 1 || Framing.GetTileSafely(x, y - 1).LiquidAmount >= 1;
                    if ((tile.Slope != SlopeType.Solid || tile.IsHalfBlock) && waterNearby)
                    {
                        drawPosition = new Vector2(x, y).ToWorldCoordinates(0f, 0f) - Main.screenPosition;
                        Rectangle area = new Rectangle(0, 0, 16, 16);
                        VertexColors color = new VertexColors(Color.White);
                        DrawPartialLiquid(false, tile, ref drawPosition, ref area, Main.waterStyle, ref color);
                    }
                }
            }

            Main.tileBatch.End();
        }

        gd.SetRenderTarget(null);
    }

    private static void DrawPartialLiquid(bool behindBlocks, Tile tileCache, ref Vector2 position, ref Rectangle liquidSize, int liquidType, ref VertexColors colors)
    {
        int slope = (int)tileCache.Slope;
        bool flag = !TileID.Sets.BlocksWaterDrawingBehindSelf[tileCache.TileType];
        if (!behindBlocks)
            flag = false;
        if (flag || slope == 0)
        {
            Main.tileBatch.Draw(TextureAssets.Liquid[liquidType].Value, position, liquidSize, colors, default(Vector2), 1f, SpriteEffects.None);
            return;
        }
        liquidSize.X += 18 * (slope - 1);
        switch (slope)
        {
            case 1:
                Main.tileBatch.Draw(TextureAssets.LiquidSlope[liquidType].Value, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
                break;
            case 2:
                Main.tileBatch.Draw(TextureAssets.LiquidSlope[liquidType].Value, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
                break;
            case 3:
                Main.tileBatch.Draw(TextureAssets.LiquidSlope[liquidType].Value, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
                break;
            case 4:
                Main.tileBatch.Draw(TextureAssets.LiquidSlope[liquidType].Value, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
                break;
        }
    }
}
