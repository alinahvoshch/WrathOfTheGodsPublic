using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Tiles.TileEntities;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Tiles.SolynCampsite;

[Autoload(Side = ModSide.Client)]
public class SolynTelescopeUIRenderer : ModSystem
{
    internal static LazyAsset<Texture2D> UIBackground;

    internal static LazyAsset<Texture2D> UIArrow;

    public override void OnModLoad()
    {
        UIBackground = LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/Tiles/SolynCampsite/TelescopeRepairUIBackground");
        UIArrow = LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/Tiles/SolynCampsite/TelescopeRepairUIArrow");
    }

    public override void PostDrawTiles()
    {
        Rectangle checkArea = Utils.CenteredRectangle(Main.LocalPlayer.Center.ToTileCoordinates().ToVector2(), Vector2.One * 24f);
        if (!SolynTelescopeTile.AnyoneCanRepairTelescope)
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, DefaultRasterizerScreenCull, null, Main.GameViewMatrix.TransformationMatrix);

        int telescopeID = ModContent.TileType<SolynTelescopeTile>();
        for (int x = checkArea.Left; x <= checkArea.Right; x++)
        {
            for (int y = checkArea.Top; y <= checkArea.Bottom; y++)
            {
                Tile t = Framing.GetTileSafely(x, y);
                if (t.TileType != telescopeID || !t.HasTile)
                    continue;

                int frameX = t.TileFrameX;
                int frameY = t.TileFrameY;
                if (frameX != 18 || frameY != 0)
                    continue;

                TESolynTelescope? telescope = FindTileEntity<TESolynTelescope>(x, y, SolynTelescopeTile.Width, SolynTelescopeTile.Height);
                if (telescope is null || telescope.UIAppearanceInterpolant <= 0f)
                    continue;

                telescope.RenderUI();
            }
        }

        Main.spriteBatch.End();
    }
}
