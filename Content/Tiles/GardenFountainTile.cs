using Microsoft.Xna.Framework;
using NoxusBoss.Content.Items.Placeable;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NoxusBoss.Content.Tiles;

public class GardenFountainTile : ModTile
{
    /// <summary>
    /// The tiled width of this door.
    /// </summary>
    public const int Width = 2;

    /// <summary>
    /// The tiled height of this door.
    /// </summary>
    public const int Height = 4;

    public override string Texture => GetAssetPath("Content/Tiles", Name);

    public override void SetStaticDefaults()
    {
        RegisterItemDrop(ModContent.ItemType<GardenFountain>());

        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileWaterDeath[Type] = false;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
        TileObjectData.addTile(Type);
        TileID.Sets.HasOutlines[Type] = true;

        TileObjectData.newTile.Width = Width;
        TileObjectData.newTile.Height = Height;
        TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, Height).ToArray();
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Origin = new Point16(0, Height - 1);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 2, 0);
        TileObjectData.newTile.StyleLineSkip = 2;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(28, 25, 44), Language.GetText("MapObject.WaterFountain"));
        AnimationFrameHeight = 72;
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        if (!Main.dedServ && Main.tile[i, j].TileFrameX >= 36)
            Main.SceneMetrics.ActiveFountainColor = ModContent.Find<ModWaterStyle>("NoxusBoss/EternalGardenWater").Slot;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Stone, 0f, 0f, 1, new Color(119, 102, 255), 1f);
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Water, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        return false;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override void AnimateTile(ref int frame, ref int frameCounter)
    {
        frameCounter++;
        if (frameCounter >= 6)
        {
            frame = (frame + 1) % 4;
            frameCounter = 0;
        }
    }

    public override void HitWire(int i, int j)
    {
        int x = i - Main.tile[i, j].TileFrameX / 18 % Width;
        int y = j - Main.tile[i, j].TileFrameY / 18 % Height;
        int widthX18 = Width * 18;
        for (int l = x; l < x + Width; l++)
        {
            for (int m = y; m < y + Height; m++)
            {
                if (Main.tile[l, m].HasTile && Main.tile[l, m].TileType == Type)
                {
                    if (Main.tile[l, m].TileFrameX < widthX18)
                        Main.tile[l, m].TileFrameX += (short)widthX18;
                    else
                        Main.tile[l, m].TileFrameX -= (short)widthX18;
                }
            }
        }

        if (Wiring.running)
        {
            for (int k = 0; k < Width; k++)
            {
                for (int l = 0; l < Height; l++)
                    Wiring.SkipWire(x + k, y + l);
            }
        }
    }

    public override bool RightClick(int i, int j)
    {
        HitWire(i, j);
        SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
        return true;
    }

    public override void MouseOver(int i, int j)
    {
        Main.LocalPlayer.noThrow = 2;
        Main.LocalPlayer.cursorItemIconEnabled = true;
        Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<GardenFountain>();
    }
}
