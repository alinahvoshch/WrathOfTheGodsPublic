using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NoxusBoss.Content.Tiles;

public class AvatarTrophyTile : ModTile
{
    public override string Texture => GetAssetPath("Content/Tiles", Name);

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.FramesOnKillWall[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
        TileObjectData.addTile(Type);

        AddMapEntry(new(120, 85, 60), Language.GetText("MapObject.Trophy"));
        DustType = 7;
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        Point p = new Point(i, j);
        if (!AvatarTrophyRenderSystem.TrophyRenderPositions.Contains(p))
            AvatarTrophyRenderSystem.TrophyRenderPositions.Add(p);
    }
}
