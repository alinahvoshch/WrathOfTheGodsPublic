using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Items.Accessories.VanityEffects;
using Terraria;
using Terraria.ModLoader;
using static NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers.NamelessDeitySky;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;

[Autoload(Side = ModSide.Client)]
public class NamelessDeitySkyTargetManager : ModSystem
{
    /// <summary>
    /// The downscaled render target that contains the contents of Nameless' sky.
    /// </summary>
    public static ManagedRenderTarget SkyTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The downscale factor of the render target.
    /// </summary>
    public static float DownscaleFactor => 0.56f;

    public override void OnModLoad()
    {
        SkyTarget = new(true, (width, height) =>
        {
            return new(Main.instance.GraphicsDevice, (int)(width * DownscaleFactor), (int)(height * DownscaleFactor));
        });

        RenderTargetManager.RenderTargetUpdateLoopEvent += UpdateSkyTargetWrapper;
    }

    private void UpdateSkyTargetWrapper()
    {
        if (Intensity <= 0f && !DeificTouch.UsingEffect)
            return;

        GraphicsDevice gd = Main.instance.GraphicsDevice;
        gd.SetRenderTarget(SkyTarget);
        gd.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        UpdateSkyTarget();
        Main.spriteBatch.End();

        gd.SetRenderTarget(null);
    }

    private static void UpdateSkyTarget()
    {
        DrawStarDimension();

        // Draw the US flag if it's in use.
        Vector2 screenArea = ViewportSize;
        if (UnitedStatesFlagOpacity > 0f)
            DrawUnitedStatesFlag(screenArea);

        // Draw a black overlay if necessary.
        Main.spriteBatch.Draw(WhitePixel, screenArea * 0.5f, null, Color.Black * BlackOverlayInterpolant, 0f, WhitePixel.Size() * 0.5f, screenArea / WhitePixel.Size(), 0, 0f);

        // Draw the scary sky.
        NamelessDeityScarySkyManager.Draw();
    }
}
