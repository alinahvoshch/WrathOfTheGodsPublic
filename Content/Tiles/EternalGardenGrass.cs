using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.FormPresets;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Tiles;

public class EternalGardenGrass : ModTile
{
    public override string Texture => GetAssetPath("Content/Tiles", Name);

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

        AddMapEntry(new Color(43, 235, 129));

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.Conversion.Grass[Type] = true;

        // Grass framing.
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = TileID.Dirt;
        TileID.Sets.CanBeDugByShovel[Type] = true;
    }

    public override void NumDust(int i, int j, bool fail, ref int Type)
    {
        Type = fail ? 1 : 3;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (fail && !effectOnly)
            Main.tile[i, j].TileType = TileID.Dirt;
    }

    public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor)
    {
        sightColor = new(59, 179, 129);
        return true;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        Color lightColor = Lighting.GetColor(i, j);
        Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        Vector2 drawPosition = new Vector2(i, j).ToWorldCoordinates() - Main.screenPosition + drawOffset;

        Texture2D texture = NamelessDeityFormPresetRegistry.UsingLucillePreset ? GennedAssets.Textures.Tiles.EternalGardenGrassAutumnal.Value : TextureAssets.Tile[Type].Value;
        DrawTileWithSlope(i, j, texture, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), lightColor, drawOffset);

        bool liquidAbove = Framing.GetTileSafely(i, j - 1).LiquidAmount >= 1;
        if ((i + j * 7) % 2 != 0 || tile.Slope != SlopeType.Solid || liquidAbove)
            return false;

        int frameX = i * 16 % 48;
        int frameY = (j + 1485) * 16 % 32 + 16;

        Texture2D flowerTexture = GennedAssets.Textures.Tiles.EternalGardenGrassPattern.Value;
        Rectangle rectangleFrame = new Rectangle(frameX, frameY, 16, 16);
        drawPosition.Y -= 2f;

        Color color = Color.Lerp(lightColor, NamelessDeityFormPresetRegistry.UsingLucillePreset ? Color.Orange : Color.White, 0.67f);

        Main.spriteBatch.Draw(flowerTexture, drawPosition, rectangleFrame, color, 0f, rectangleFrame.Size() * 0.5f, 1f, 0, 0f);
        return false;
    }
}
