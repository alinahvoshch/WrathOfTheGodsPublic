using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Core.Graphics.RenderTargets;

public class DownscaleOptimizedScreenTarget
{
    public readonly InstancedRequestableTarget DownscaledTarget;

    /// <summary>
    /// The amount by which the contents should be downscaled. Intended to be above 0 and below 1.
    /// </summary>
    /// 
    /// <remarks>
    /// The lower this value is, the greater the performance, but at the cost of degraded visual quality.
    /// </remarks>
    public readonly float DownscaleFactor;

    /// <summary>
    /// The action that governs what gets drawn to the render target.
    /// </summary>
    public readonly Action<int> RenderAction;

    public DownscaleOptimizedScreenTarget(float downscaleFactor, Action<int> renderAction)
    {
        DownscaleFactor = downscaleFactor;
        RenderAction = renderAction;

        DownscaledTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(DownscaledTarget);
    }

    /// <summary>
    /// Renders the downscaled target.
    /// </summary>
    /// <param name="color">The overlay color.</param>
    public void Render(Color color, int identifier = 0)
    {
        Rectangle screenArea = new Rectangle(0, 0, Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
        Render(color, screenArea, identifier);
    }

    /// <summary>
    /// Renders the downscaled target.
    /// </summary>
    /// <param name="color">The overlay color.</param>
    /// <param name="renderArea">The area upon which the target should be rendered.</param>
    public void Render(Color color, Rectangle renderArea, int identifier = 0)
    {
        int width = (int)(Main.instance.GraphicsDevice.Viewport.Width * DownscaleFactor);
        int height = (int)(Main.instance.GraphicsDevice.Viewport.Height * DownscaleFactor);
        int identifierCopy = identifier;
        DownscaledTarget.Request(width, height, identifier, () =>
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, CullOnlyScreen);
            RenderAction(identifierCopy);
            Main.spriteBatch.End();
        });

        if (!DownscaledTarget.TryGetTarget(identifier, out RenderTarget2D? target) || target is null)
            return;

        Main.spriteBatch.Draw(target, renderArea, color);
    }
}
