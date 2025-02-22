using Microsoft.Xna.Framework;
using NoxusBoss.Core.Graphics.StarblessedPlatforms;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NoxusBoss.Content.Tiles;

public class StarblessedPlatformTile : ModTile
{
    public override string Texture => GetAssetPath("Content/Tiles", Name);

    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = false;
        TileID.Sets.Platforms[Type] = true;
        TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height).ToArray();
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleMultiplier = 27;
        TileObjectData.newTile.StyleWrapLimit = 27;
        TileObjectData.newTile.UsesCustomCanPlace = false;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
        TileObjectData.addTile(Type);

        HitSound = null;

        AddMapEntry(new Color(255, 224, 96));
        AdjTiles = new int[] { TileID.Platforms };
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        // Check this platform to determine if there's anything unusual to its sides, such as air.
        int sourceID = ModContent.TileType<StarblessedPlatformSource>();
        int platformID = Type;
        bool hasValidNeighbors = true;
        foreach (Vector2 tileOffset in StarblessedPlatformSystem.PossibleConnectDirections)
        {
            Point checkPoint = (new Vector2(i, j) + tileOffset).ToPoint();
            Tile checkTile = Framing.GetTileSafely(checkPoint);
            if (checkTile.TileType != sourceID && checkTile.TileType != platformID)
            {
                hasValidNeighbors = false;
                break;
            }
        }

        // If no valid neighbor set was found, that means that this is a lone, functionally unbreakable tile and needs to be destroyed.
        if (!hasValidNeighbors)
        {
            // SPECIAL CASE: Breaking this tile while there's a (potentially filled) chest above would cause the game to explode because chests are silly.
            // If that's the case, wait for the chest to go away first before being destroyed.
            Tile above = Framing.GetTileSafely(i, j - 1);
            bool cantBeDestroyed = (TileID.Sets.IsAContainer[above.TileType] || above.TileType == TileID.Containers || above.TileType == TileID.Containers2) && above.HasTile;
            if (!cantBeDestroyed)
            {
                WorldGen.KillTile(i, j);
                for (int k = 0; k < 3; k++)
                {
                    Vector2 starSpawnPosition = new Point(i, j).ToWorldCoordinates() + Main.rand.NextVector2Circular(5f, 5f);
                    StarblessedPlatformSystem.CreateStar(starSpawnPosition);
                }
            }
        }
    }

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;

    public override bool CanReplace(int i, int j, int tileTypeBeingPlaced) => false;

    public override bool CanExplode(int i, int j) => false;

    public override bool CreateDust(int i, int j, ref int type) => false;

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 1f;
        g = 1f;
        b = 1f;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        Tile t = Main.tile[i, j];
        t.TileFrameX = (short)((i * 11 + j * 13) % 3 * 18);

        return false;
    }
}
