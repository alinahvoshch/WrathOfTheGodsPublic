using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Tiles;

[Autoload(Side = ModSide.Client)]
public class AvatarTrophyRenderSystem : ModSystem
{
    /// <summary>
    /// The set of all render positions associated with the Avatar's trophies.
    /// </summary>
    internal static readonly List<Point> TrophyRenderPositions = [];

    /// <summary>
    /// The render target that contains render information for all of the Avatar trophies.
    /// </summary>
    public static ManagedRenderTarget TrophyMaskTarget
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        RenderTargetManager.RenderTargetUpdateLoopEvent += RenderStatic;
        TrophyMaskTarget = new ManagedRenderTarget(true, ManagedRenderTarget.CreateScreenSizedTarget);
    }

    private void RenderStatic()
    {
        if (TrophyRenderPositions.Count <= 0)
            return;

        PrepareStaticTarget();

        ManagedScreenFilter staticShader = ShaderManager.GetFilter("NoxusBoss.AvatarTrophyStaticShader");
        staticShader.TrySetParameter("oldScreenPosition", Main.screenPosition);
        staticShader.SetTexture(TrophyMaskTarget, 1, SamplerState.PointWrap);
        staticShader.Activate();

        // Clear the position cache.
        TrophyRenderPositions.RemoveAll(p => !p.ToWorldCoordinates().WithinRange(Main.screenPosition, 4000f) || !Framing.GetTileSafely(p).HasTile);
    }

    private static void PrepareStaticTarget()
    {
        Main.instance.GraphicsDevice.SetRenderTarget(TrophyMaskTarget);

        Main.spriteBatch.ResetToDefault(false);
        foreach (Point point in TrophyRenderPositions)
        {
            Tile t = Framing.GetTileSafely(point);
            Vector2 drawPosition = point.ToWorldCoordinates(0f, 0f) - Main.screenPosition;
            Rectangle frame = new Rectangle(t.TileFrameX, t.TileFrameY, 16, 16);

            Main.spriteBatch.Draw(GennedAssets.Textures.Tiles.AvatarTrophyTile, drawPosition, frame, Color.White, 0f, Vector2.Zero, 1f, 0, 0f);
        }

        Main.spriteBatch.End();
    }
}
