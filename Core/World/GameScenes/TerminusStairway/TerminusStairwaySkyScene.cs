using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Autoloaders;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Graphics.Shaders.Screen;
using NoxusBoss.Core.Graphics.UI.GraphicalUniverseImager;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static NoxusBoss.Core.World.GameScenes.TerminusStairway.TerminusStairwaySky;

namespace NoxusBoss.Core.World.GameScenes.TerminusStairway;

public class TerminusStairwaySkyScene : ModSceneEffect
{
    /// <summary>
    /// The render target responsible for holding visual data about the gradient for the GUI.
    /// </summary>
    public static DownscaleOptimizedScreenTarget GradientTarget
    {
        get;
        private set;
    }

    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/ascent");

    public override bool IsSceneEffectActive(Player player) => TerminusStairwaySystem.Enabled;

    public override void SpecialVisuals(Player player, bool isActive)
    {
        if (isActive)
            SkyManager.Instance.Activate(ScreenShaderKey);

        player.ManageSpecialBiomeVisuals(ScreenShaderKey, isActive);
    }

    public override void Load()
    {
        GradientTarget = new(0.5f, DrawGradient);
        SkyManager.Instance[ScreenShaderKey] = new TerminusStairwaySky();
        Filters.Scene[ScreenShaderKey] = new Filter(new GenericScreenShaderData("FilterMiniTower").UseColor(Color.Transparent).UseOpacity(0f), EffectPriority.VeryHigh);

        string contentPath = GetAssetPath("Content/Items/Placeable/MusicBoxes", "AscentMusicBox");
        string musicPath = "Assets/Sounds/Music/ascent";
        MusicBoxAutoloader.Create(Mod, contentPath, musicPath, out _, out _);
    }

    internal static void LoadGUIOption()
    {
        GraphicalUniverseImagerOptionManager.RegisterNew(new GraphicalUniverseImagerOption("Mods.NoxusBoss.UI.GraphicalUniverseImager.TerminusStairway", true,
            GennedAssets.Textures.GraphicalUniverseImager.ShaderSource_Garden, RenderGUIPortrait, RenderGUIBackground));
    }

    private static void DrawGradient(int identifier)
    {
        GetGradientColors(0f, 0f, out Color top, out Color bottom);

        ManagedShader backgroundShader = ShaderManager.GetShader("NoxusBoss.TerminusBackgroundShader");
        backgroundShader.TrySetParameter("bottomThreshold", 0.9f);
        backgroundShader.TrySetParameter("gradientTop", top.ToVector4());
        backgroundShader.TrySetParameter("gradientBottom", bottom.ToVector4());
        backgroundShader.TrySetParameter("screenPosition", Vector2.Zero);
        backgroundShader.TrySetParameter("additiveTopOfWorldColor", Vector4.Zero);
        backgroundShader.Apply();

        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea / WhitePixel.Size();
        Main.spriteBatch.Draw(WhitePixel, screenArea * 0.5f, null, Color.White, 0f, WhitePixel.Size() * 0.5f, textureArea, 0, 0f);
    }

    private static void RenderGUI(int identifier, Vector2 screenPosition)
    {
        GradientTarget.Render(Color.White, identifier);

        RenderClouds(false, CompletionInterpolant, 0f, 0f);
        RenderBackgroundElements(screenPosition, CompletionInterpolant);
        RenderClouds(true, CompletionInterpolant, 0f, 0f);

        // Draw the mist at the bottom of the screen.
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        DrawBottomMist(0f);
    }

    private static void RenderGUIPortrait(GraphicalUniverseImagerSettings settings)
    {
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        RenderGUI(1, new Vector2(0f, 7600f));
        Main.spriteBatch.End();
    }

    private static void RenderGUIBackground(float minDepth, float maxDepth, GraphicalUniverseImagerSettings settings)
    {
        RenderGUI(2, Main.screenPosition);
    }
}
