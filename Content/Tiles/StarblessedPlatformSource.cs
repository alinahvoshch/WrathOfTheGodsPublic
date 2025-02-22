using Microsoft.Xna.Framework;
using NoxusBoss.Content.Items.Placeable;
using NoxusBoss.Core.Graphics.StarblessedPlatforms;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NoxusBoss.Content.Tiles;

public class StarblessedPlatformSource : ModTile
{
    public override string Texture => GetAssetPath("Content/Tiles", Name);

    public override void SetStaticDefaults()
    {
        RegisterItemDrop(ModContent.ItemType<StarblessedPlatform>());

        Main.tileLighted[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileSolidTop[Type] = true;

        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(145, 130, 81));

        DustType = DustID.AncientLight;

        On_TileObject.CanPlace += AllowMidairPlacement;
        On_Player.IsTilePoundable += DisableHammering;
    }

    private bool DisableHammering(On_Player.orig_IsTilePoundable orig, Player self, Tile targetTile)
    {
        if (targetTile.TileType == Type)
            return false;

        return orig(self, targetTile);
    }

    private bool AllowMidairPlacement(On_TileObject.orig_CanPlace orig, int x, int y, int type, int style, int dir, out TileObject objectData, bool onlyCheck, int? forcedRandom, bool checkStay)
    {
        bool result = orig(x, y, type, style, dir, out objectData, onlyCheck, forcedRandom, checkStay);
        if (type == Type)
            return true;

        return result;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 1f;
        g = 1f;
        b = 1f;
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        Tile t = Main.tile[i, j];
        t.Get<TileWallWireStateData>().Slope = SlopeType.Solid;
        t.Get<TileWallWireStateData>().IsHalfBlock = false;
    }

    public static bool CurrentlyUnbreakable(int i, int j)
    {
        Vector2 p = new Vector2(i, j);
        return StarblessedPlatformProjectionSystem.Projections.Any(projection => projection.Line.End == p || projection.Line.Start == p);
    }

    public override bool CanExplode(int i, int j) => !CurrentlyUnbreakable(i, j);

    public override bool CanReplace(int i, int j, int tileTypeBeingPlaced) => !CurrentlyUnbreakable(i, j);

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => !CurrentlyUnbreakable(i, j);
}
