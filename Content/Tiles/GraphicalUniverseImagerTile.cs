using Microsoft.Xna.Framework;
using NoxusBoss.Content.Tiles.TileEntities;
using NoxusBoss.Core.Graphics.UI.GraphicalUniverseImager;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NoxusBoss.Content.Tiles;

public class GraphicalUniverseImagerTile : ModTile
{
    /// <summary>
    /// The tiled width of this GUI.
    /// </summary>
    public const int Width = 2;

    /// <summary>
    /// The tiled height of this GUI.
    /// </summary>
    public const int Height = 3;

    /// <summary>
    /// How far out this GUI can affect things.
    /// </summary>
    public static float InfluenceRadius => 3000f;

    public override string Texture => GetAssetPath("Content/Tiles", Name);

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileObsidianKill[Type] = true;

        TileID.Sets.HasOutlines[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.Width = Width;
        TileObjectData.newTile.Height = Height;
        TileObjectData.newTile.Origin = new Point16(Width / 2, Height - 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height).ToArray();
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TEGraphicalUniverseImager>().Hook_AfterPlacement, -1, 0, true);

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(147, 102, 30));

        HitSound = SoundID.Tink;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        Tile t = Main.tile[i, j];
        int left = i - t.TileFrameX / 18;
        int top = j - t.TileFrameY / 18;

        TEGraphicalUniverseImager? tileEntity = FindTileEntity<TEGraphicalUniverseImager>(i, j, Width, Height);
        tileEntity?.Kill(left, top);
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override bool RightClick(int i, int j)
    {
        TEGraphicalUniverseImager? te = FindTileEntity<TEGraphicalUniverseImager>(i, j, Width, Height);
        if (te is null)
            return false;

        te.UIEnabled = !te.UIEnabled;
        if (te.UIEnabled)
        {
            ModContent.GetInstance<UniverseImagerUI>().VisibleTileEntity = te;
            if (te.Settings.Option is not null)
            {
                List<GraphicalUniverseImagerOption> options = GraphicalUniverseImagerOptionManager.options.Values.ToList();
                ModContent.GetInstance<UniverseImagerUI>().IdealMainPanelHorizontalScroll = ModContent.GetInstance<UniverseImagerUI>().MainPanelHorizontalScroll = options.IndexOf(te.Settings.Option) - 2;
            }
        }

        return true;
    }
}
