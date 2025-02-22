using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles.Metaballs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.Mist;

public class MistRenderingSystem : ModSystem
{
    /// <summary>
    /// The render target responsible for rendering mist.
    /// </summary>
    public static ManagedRenderTarget MistTarget
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        MistTarget = new(true, (width, height) =>
        {
            return new(Main.instance.GraphicsDevice, width / 2, height / 2);
        });

        RenderTargetManager.RenderTargetUpdateLoopEvent += UpdateMistTargetWrapper;

        Main.OnPostDraw += DrawMistTargetToScreen;
    }

    public override void OnModUnload()
    {
        Main.OnPostDraw -= DrawMistTargetToScreen;
    }

    private void UpdateMistTargetWrapper()
    {
        if (Main.gameMenu || !ModContent.GetInstance<MistMetaball>().ShouldRender)
            return;

        GraphicsDevice graphicsDevice = Main.instance.GraphicsDevice;
        graphicsDevice.SetRenderTarget(MistTarget);
        graphicsDevice.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        UpdateMistTarget();
        Main.spriteBatch.End();
    }

    private static void UpdateMistTarget()
    {
        Vector2 lightPosition = Main.dayTime ? SunMoonPositionRecorder.SunPosition : SunMoonPositionRecorder.MoonPosition;
        Vector2 lightPositionUV = lightPosition / Main.ScreenSize.ToVector2();
        lightPositionUV = (lightPositionUV - Vector2.One * 0.5f) / Main.GameViewMatrix.Zoom + Vector2.One * 0.5f;

        Rectangle area = new Rectangle(0, 0, Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
        ManagedShader mistShader = ShaderManager.GetShader("NoxusBoss.MistRaymarchShader");
        mistShader.TrySetParameter("minDropletSize", 370f);
        mistShader.TrySetParameter("maxDropletSize", 1485f);
        mistShader.TrySetParameter("stepIncrement", 0.05f);
        mistShader.TrySetParameter("brightnessAccentuationBiasExponent", 2f);
        mistShader.TrySetParameter("brightnessAccentuationBiasFactor", 3f);
        mistShader.TrySetParameter("saturationAccentuationBiasExponent", 2f);
        mistShader.TrySetParameter("saturationAccentuationBiasFactor", 5f);
        mistShader.TrySetParameter("lightPosition", new Vector3(lightPositionUV, 0f));
        mistShader.TrySetParameter("lightWavelengths", new Vector3(670f, 540f, 440f));
        mistShader.SetTexture(BubblyNoise, 1, SamplerState.LinearWrap);
        mistShader.SetTexture(PerlinNoise, 2, SamplerState.LinearWrap);
        mistShader.SetTexture(ModContent.GetInstance<MistMetaball>().LayerTargets[0], 3, SamplerState.LinearWrap);
        mistShader.SetTexture(GennedAssets.Textures.Extra.Psychedelic, 4, SamplerState.LinearWrap);
        mistShader.Apply();

        Main.spriteBatch.Draw(WhitePixel, area, Color.White);
    }

    private void DrawMistTargetToScreen(GameTime obj)
    {
        if (Main.gameMenu || !ModContent.GetInstance<MistMetaball>().ShouldRender)
            return;

        Rectangle area = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
        Main.spriteBatch.ResetToDefault(false);
        Main.spriteBatch.Draw(MistTarget, area, Color.White);
        Main.spriteBatch.End();
    }
}
