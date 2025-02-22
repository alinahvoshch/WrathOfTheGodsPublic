using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Particles.Metaballs;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;

[Autoload(Side = ModSide.Client)]
public class CodeBackgroundManager : ModSystem
{
    /// <summary>
    /// The render target that contains the code background.
    /// </summary>
    public static ManagedRenderTarget CodeBackgroundTarget
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        CodeBackgroundTarget = new(true, ManagedRenderTarget.CreateScreenSizedTarget);
        RenderTargetManager.RenderTargetUpdateLoopEvent += UpdateCodeTarget;
    }

    public override void PostSetupContent()
    {
        Main.QueueMainThreadAction(() =>
        {
            CodeBackgroundTarget.Target.Disposing += WhatTheFuck;
        });
    }

    private void WhatTheFuck(object? sender, EventArgs e)
    {
        CodeBackgroundTarget.Recreate(Main.screenWidth, Main.screenHeight);
    }

    private void UpdateCodeTarget()
    {
        if (!ModContent.GetInstance<CodeMetaball>().ShouldRender)
            return;

        GraphicsDevice gd = Main.instance.GraphicsDevice;

        gd.SetRenderTarget(CodeBackgroundTarget);
        gd.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

        Texture2D binary = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/Code").Value;
        ManagedShader backgroundShader = ShaderManager.GetShader("NoxusBoss.CodeBackgroundShader");
        backgroundShader.TrySetParameter("frameSize", binary.Size() * new Vector2(1f, 1f));
        backgroundShader.TrySetParameter("viewSize", new Vector2(gd.Viewport.Width, gd.Viewport.Height));
        backgroundShader.TrySetParameter("binaryColor", new Color(255, 255, 255).ToVector3());
        backgroundShader.TrySetParameter("digitShiftSpeed", 2f);
        backgroundShader.TrySetParameter("opacityScrollSpeed", 2.3f);
        backgroundShader.TrySetParameter("downwardScrollSpeed", 0.75f);
        backgroundShader.TrySetParameter("totalFrames", 8f);
        backgroundShader.SetTexture(binary, 1, SamplerState.PointWrap);
        backgroundShader.Apply();

        Main.spriteBatch.Draw(WhitePixel, new Rectangle(0, 0, gd.Viewport.Width, gd.Viewport.Height), new(0, 4, 10));

        Main.spriteBatch.End();

        gd.SetRenderTarget(null);
    }
}
