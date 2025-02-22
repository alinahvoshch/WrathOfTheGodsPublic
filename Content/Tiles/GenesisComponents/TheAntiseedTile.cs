using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.SoundSystems;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NoxusBoss.Content.Tiles.GenesisComponents;

public class TheAntiseedTile : ModTile, ICustomPlacementSound
{
    public SoundStyle PlaceSound => GennedAssets.Sounds.Genesis.AntiseedPlant;

    /// <summary>
    /// The tiled width of the seedling.
    /// </summary>
    public const int Width = 2;

    /// <summary>
    /// The tiled height of the seedling.
    /// </summary>
    public const int Height = 2;

    /// <summary>
    /// The height of this seedling's stem.
    /// </summary>
    public static float StemHeight => 276f;

    /// <summary>
    /// The branches texture of the seedling.
    /// </summary>
    public static LazyAsset<Texture2D> Branches
    {
        get;
        private set;
    }

    /// <summary>
    /// The bottom texture of the seedling.
    /// </summary>
    public static LazyAsset<Texture2D> BottomTexture
    {
        get;
        private set;
    }

    /// <summary>
    /// The top texture of the seedling.
    /// </summary>
    public static LazyAsset<Texture2D> TopTexture
    {
        get;
        private set;
    }

    public override string Texture => GetAssetPath("Content/Tiles/GenesisComponents", Name);

    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
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

        HitSound = null;
        AddMapEntry(Color.Black);

        if (Main.netMode != NetmodeID.Server)
        {
            Branches = LazyAsset<Texture2D>.FromPath($"{Texture}Branches");
            BottomTexture = LazyAsset<Texture2D>.FromPath($"{Texture}Bottom");
            TopTexture = LazyAsset<Texture2D>.FromPath($"{Texture}Top");
        }
    }

    public override bool CanDrop(int i, int j) => false;

    public override bool CreateDust(int i, int j, ref int type) => false;

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = -4f;
        g = -4f;
        b = -4f;
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        Tile t = Main.tile[i, j];
        if (t.TileFrameX == 18 && t.TileFrameY == (Height - 1) * 18)
            ModContent.GetInstance<AntiseedTileRenderSystem>().AddPoint(new(i, j));
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;
}
