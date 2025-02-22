using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Items.GenesisComponents;
using NoxusBoss.Content.Tiles.GenesisComponents.Seedling;
using NoxusBoss.Core.SoundSystems;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NoxusBoss.Content.Tiles.GenesisComponents;

public class GenesisTile : ModTile, ICustomPlacementSound
{
    public SoundStyle PlaceSound => ModContent.GetInstance<FrostblessedSeedling>().PlaceSound;

    /// <summary>
    /// The tiled width of the Genesis.
    /// </summary>
    public const int Width = 2;

    /// <summary>
    /// The tiled height of the Genesis.
    /// </summary>
    public const int Height = 3;

    public override string Texture => GetAssetPath("Content/Tiles/GenesisComponents", Name);

    public override void SetStaticDefaults()
    {
        RegisterItemDrop(ModContent.ItemType<FrostblessedSeedlingItem>());

        Main.tileFrameImportant[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.Width = Width;
        TileObjectData.newTile.Height = Height;
        TileObjectData.newTile.Origin = new Point16(Width / 2, Height - 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height).ToArray();
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(112, 58, 95));
    }

    public override bool RightClick(int i, int j)
    {
        return false;
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        Tile t = Main.tile[i, j];
        if (t.TileFrameX == Width * 9 && t.TileFrameY == (Height - 1) * 18)
            GrowingGenesisRenderSystem.AddGenesisPoint(new(i, j));
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;

    public override bool CanPlace(int i, int j)
    {
        Tile below = Framing.GetTileSafely(i, j + 1);
        return TileID.Sets.Grass[below.TileType];
    }
}
