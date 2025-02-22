using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.FormPresets;
using NoxusBoss.Content.Tiles.TileEntities;
using NoxusBoss.Core.Graphics.Blossoms;
using NoxusBoss.Core.World.Subworlds;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NoxusBoss.Content.Tiles;

// Deliberately renamed from TreeOfLife to ensure that old version trees of differing size get removed automatically.
public class GoodAppleTree : ModTile
{
    public static int TrunkWidth => 12;

    public static int TrunkHeight => 8;

    public override string Texture => GetAssetPath("Content/Tiles", Name);

    public override void SetStaticDefaults()
    {
        Main.tileAxe[Type] = true;
        Main.tileFrameImportant[Type] = true;
        TileObjectData.newTile.Width = TrunkWidth;
        TileObjectData.newTile.Height = TrunkHeight;
        TileObjectData.newTile.Origin = new Point16(TrunkWidth / 2, TrunkHeight - 1);
        TileObjectData.newTile.AnchorWall = true;
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height).ToArray();
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TEGoodAppleTree>().Hook_AfterPlacement, -1, 0, true);
        TileObjectData.addTile(Type);
        AddMapEntry(new Color(151, 107, 75));

        MineResist = 5f;

        On_TileDrawing.GetScreenDrawArea += IncreaseDrawAreaInGarden;
    }

    private void IncreaseDrawAreaInGarden(On_TileDrawing.orig_GetScreenDrawArea orig, TileDrawing self, Vector2 screenPosition, Vector2 offSet, out int firstTileX, out int lastTileX, out int firstTileY, out int lastTileY)
    {
        orig(self, screenPosition, offSet, out firstTileX, out lastTileX, out firstTileY, out lastTileY);
        if (EternalGardenUpdateSystem.WasInSubworldLastUpdateFrame)
        {
            int areaExtension = 16;
            firstTileX -= areaExtension;
            lastTileX += areaExtension;
            firstTileY -= areaExtension;
            lastTileY += areaExtension;
        }
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) => false;

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;

    public override bool CanExplode(int i, int j) => false;

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        // Only draw at the center.
        Tile t = Framing.GetTileSafely(i, j);
        short frameX = t.TileFrameX;
        short frameY = t.TileFrameY;
        if (frameX == 0 && frameY == 0)
        {
            if (t.IsActuated)
                DrawBrokenTree(i, j, spriteBatch);
            else
                DrawIntactTree(i, j, spriteBatch);
        }
        else
            return false;

        // Go no further if this isn't the top left subtitle of the tree, to ensure that only that subtile renders apples.
        if (frameX != 0 || frameY != 0)
            return false;

        // Attempt to access tree data for the purposes of rendering apples.
        TEGoodAppleTree? treeData = FindTileEntity<TEGoodAppleTree>(i, j, TrunkWidth, TrunkHeight);
        if (treeData is null)
            return false;

        foreach (TEGoodAppleTree.Apple apple in treeData.ApplesOnTree)
            apple.Render(new Vector2(i, j).ToWorldCoordinates());

        return false;
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        Tile t = Framing.GetTileSafely(i, j);
        short frameX = t.TileFrameX;
        short frameY = t.TileFrameY;
        if (frameX != 0 || frameY != 0)
            return;

        if (Main.gamePaused || !Main.rand.NextBool(15) || NamelessDeityBoss.Myself is not null)
            return;

        // Emit leaves randomly off the tree.
        Vector2 leafSize = new Vector2(4f, 4f) * Main.rand.NextFloat(0.9f, 1.6f);
        Color leafColor = Color.White;
        Vector2 leafSpawnPosition = new Vector2(i, j).ToWorldCoordinates() + new Vector2(Main.rand.NextFloat(-120f, 500f), -Main.rand.NextFloat(270f, 450f));
        if (Collision.SolidCollision(leafSpawnPosition, 20, 20))
            return;

        Vector2 leafVelocity = new Vector2(-Main.rand.NextFloat(1.1f), Main.rand.NextFloat(0.5f, 1.5f));
        LeafVisualsSystem.ParticleSystem.CreateNew(leafSpawnPosition, leafVelocity, leafSize, leafColor);
    }

    public static void DrawIntactTree(int i, int j, SpriteBatch spriteBatch)
    {
        // Draw the main tile texture.
        Texture2D treeTexture = GennedAssets.Textures.Tiles.GoodAppleTreeButReal.Value;
        if (NamelessDeityFormPresetRegistry.UsingLucillePreset)
            treeTexture = GennedAssets.Textures.Tiles.GoodAppleTreeButRealAutumnal.Value;

        Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        Vector2 drawPosition = new Vector2(i * 16 - Main.screenPosition.X + 120f, j * 16 - Main.screenPosition.Y + 150f) + drawOffset;
        spriteBatch.Draw(treeTexture, drawPosition, null, Color.White, 0f, treeTexture.Size() * new Vector2(0.5f, 1f), 1f, 0, 0f);
    }

    public static void DrawBrokenTree(int i, int j, SpriteBatch spriteBatch)
    {
        // Draw the trunk.
        Texture2D trunkTexture = GennedAssets.Textures.Tiles.GoodAppleTreeTrunk.Value;
        Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        Vector2 drawPosition = new Vector2(i * 16 - Main.screenPosition.X + 120f, j * 16 - Main.screenPosition.Y + 150f) + drawOffset;
        spriteBatch.Draw(trunkTexture, drawPosition, null, Color.White, 0f, trunkTexture.Size() * new Vector2(0.5f, 1f), 1f, 0, 0f);
    }
}
