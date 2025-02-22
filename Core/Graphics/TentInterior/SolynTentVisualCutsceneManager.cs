using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.TentInterior;

[Autoload(Side = ModSide.Client)]
public class SolynTentVisualCutsceneManager : ModSystem
{
    /// <summary>
    /// The render target which contains render information about the current cutscene.
    /// </summary>
    public static InstancedRequestableTarget CutsceneTarget
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        CutsceneTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(CutsceneTarget);
    }

    public override void PreUpdateEntities()
    {
        List<SolynTentVisualCutscene> activeCutscenes = ModContent.GetContent<SolynTentVisualCutscene>().Where(c => c.IsActive).ToList();
        foreach (SolynTentVisualCutscene cutscene in activeCutscenes)
            cutscene.Time++;
    }

    private static void RenderBackground(float backgroundFadeout, int time)
    {
        float opacity = Sqrt(SmoothStep(0f, 1f, InverseLerp(0f, 180f, time))) * (1f - backgroundFadeout);

        ManagedShader backgroundShader = ShaderManager.GetShader("NoxusBoss.SolynCutsceneBackgroundShader");
        backgroundShader.TrySetParameter("pixelationFactor", Vector2.One * 1.74f / ViewportSize);
        backgroundShader.TrySetParameter("brightnessThresholding", 0.16f);
        backgroundShader.TrySetParameter("wavinessFactor", 0.012f);
        backgroundShader.TrySetParameter("flowSpeed", 0.0176f);
        backgroundShader.TrySetParameter("darkeningFactor", 0.81f);
        backgroundShader.TrySetParameter("twinkleSpeed", 0.51f);
        backgroundShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        backgroundShader.Apply();

        Main.spriteBatch.Draw(WhitePixel, ViewportArea, new Color(37, 2, 90) * opacity);
    }

    public override void PostDrawTiles()
    {
        List<SolynTentVisualCutscene> activeCutscenes = ModContent.GetContent<SolynTentVisualCutscene>().Where(c => c.IsActive).ToList();
        if (activeCutscenes.Count <= 0)
            return;

        Vector2 screenSize = ViewportSize;
        CutsceneTarget.Request((int)screenSize.X, (int)screenSize.Y, 0, () =>
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            RenderBackground(activeCutscenes.Max(c => c.BackgroundFadeOut), activeCutscenes.Max(c => c.Time));
            Main.spriteBatch.End();
            Main.spriteBatch.Begin();

            foreach (SolynTentVisualCutscene cutscene in activeCutscenes)
                cutscene.Render();

            Main.spriteBatch.End();
        });
    }
}
