using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Buffs;
using NoxusBoss.Core.Data;
using NoxusBoss.Core.World.GameScenes.EndCredits;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NoxusBoss.Content.Tiles;

public class LotusOfCreationTile : ModTile
{
    private static LazyAsset<Texture2D> lotusTexture;

    /// <summary>
    /// The tiled width of this lotus.
    /// </summary>
    public const int Width = 7;

    /// <summary>
    /// The tiled height of this lotus.
    /// </summary>
    public const int Height = 5;

    /// <summary>
    /// The palette used used by this lotus.
    /// </summary>
    public static Vector3[] ShaderPalette
    {
        get
        {
            string paletteFilePath = $"{ModContent.GetInstance<LotusOfCreationTile>().GetModRelativeDirectory()}Palette.json";
            Vector3[] palette = LocalDataManager.Read<Vector3[]>(paletteFilePath)["Standard"];
            return palette;
        }
    }

    /// <summary>
    /// How long the invincibility buff lasts when this lotus is right clicked.
    /// </summary>
    public static int InvincibilityDuration => MinutesToFrames(14400f);

    /// <summary>
    /// The sound that this lotus plays upon being used by the player.
    /// </summary>
    public static readonly SoundStyle UseSound = GennedAssets.Sounds.Item.LotusOfCreationUse with { Volume = 1.2f, MaxInstances = 0 };

    public override string Texture => GetAssetPath("Content/Tiles", Name);

    public override void SetStaticDefaults()
    {
        if (Main.netMode != NetmodeID.Server)
            lotusTexture = LazyAsset<Texture2D>.FromPath($"{Texture}Real");

        Main.tileFrameImportant[Type] = true;

        // Prepare necessary setups to ensure that this tile is treated like grass.
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

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

        // All of the special plants in Nameless' garden glow slightly.
        Main.tileLighted[Type] = true;

        // Use plant destruction visuals and sounds.
        HitSound = SoundID.Grass;
        DustType = DustID.WhiteTorch;

        AddMapEntry(new Color(244, 196, 207));
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Framing.GetTileSafely(i, j);
        if (tile.TileFrameX != 72 || tile.TileFrameY != 72)
            return false;

        if (Main.drawToScreen)
            spriteBatch.PrepareForShaders();
        else
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        }

        float appearanceInterpolant = ModContent.GetInstance<EndCreditsScene>().LotusAppearanceInterpolant;
        if (!ModContent.GetInstance<EndCreditsScene>().IsActive)
            appearanceInterpolant = 1f;

        Vector3[] palette = ShaderPalette;

        ManagedShader lotusShader = ShaderManager.GetShader("NoxusBoss.LotusOfCreationShader");
        lotusShader.TrySetParameter("appearanceInterpolant", appearanceInterpolant);
        lotusShader.TrySetParameter("gradient", palette);
        lotusShader.TrySetParameter("gradientCount", palette.Length);
        lotusShader.Apply();

        // Draw the main tile texture.
        Texture2D mainTexture = lotusTexture.Value;
        Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        drawOffset.Y += 18f;

        float pulse = (Main.GlobalTimeWrappedHourly * 0.74f + i * 0.13f + j * 0.23f) % 1f;
        Vector2 drawPosition = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y + 2f) + drawOffset;
        Color lightColor = Color.Lerp(Lighting.GetColor(i, j), Color.White, 0.75f);
        SpriteEffects direction = (i * 3 + j * 717) % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        spriteBatch.Draw(mainTexture, drawPosition, null, lightColor, 0f, new Vector2(0.5f, 1f) * mainTexture.Size(), appearanceInterpolant * 0.5f, direction, 0f);

        spriteBatch.Draw(mainTexture, drawPosition, null, lightColor * (1f - pulse).Cubed() * InverseLerp(0.05f, 0.3f, pulse), 0f, new Vector2(0.5f, 1f) * mainTexture.Size(), appearanceInterpolant * (0.5f + pulse * 0.1f), direction, 0f);

        if (Main.drawToScreen)
            spriteBatch.ResetToDefault();
        else
        {
            spriteBatch.End();
            spriteBatch.Begin();
        }

        return false;
    }

    public override bool RightClick(int i, int j)
    {
        SoundEngine.PlaySound(UseSound);
        Main.LocalPlayer.AddBuff(ModContent.BuffType<InvincibilityBuff>(), InvincibilityDuration);
        ScreenShakeSystem.StartShake(2f);
        return true;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.5f;
        g = 0.3f;
        b = 0.7f;
    }
}
