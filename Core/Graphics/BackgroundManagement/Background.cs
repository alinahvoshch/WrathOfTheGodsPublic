using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.BackgroundManagement;

public abstract class Background : ModType<Background>
{
    /// <summary>
    /// Whether this background should be active or not.
    /// </summary>
    public bool ShouldBeActive
    {
        get;
        set;
    }

    /// <summary>
    /// Whether this background is active currently or not.
    /// </summary>
    public bool IsActive => Opacity > 0f;

    /// <summary>
    /// The opacity of this background. Approaches 0 if the background is inactive, approaches 1 if the background is active.
    /// </summary>
    public float Opacity
    {
        get;
        set;
    }

    /// <summary>
    /// The opacity of clouds when this background is active.
    /// </summary>
    public virtual float CloudOpacity => 1f;

    /// <summary>
    /// The layering priority of this background. Higher values correspond to a tendency to be drawn atop other backgrounds.
    /// </summary>
    public abstract float Priority
    {
        get;
    }

    protected sealed override void Register() => ModTypeLookup<Background>.Register(this);

    /// <summary>
    /// Updates this background's opacity based on whether it's active.
    /// </summary>
    public virtual void Update()
    {
        Opacity = Opacity.StepTowards(ShouldBeActive.ToInt(), 0.02f);
    }

    /// <summary>
    /// An easy shorthand that allows for the swapping of sprite sort mode in the context of background rendering.
    /// </summary>
    public static void SetSpriteSortMode(SpriteSortMode spriteSortMode, Matrix? matrix = null)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(spriteSortMode, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, CullOnlyScreen, null, matrix ?? GetCustomSkyBackgroundMatrix());
    }

    /// <summary>
    /// Renders this background.
    /// </summary>
    public abstract void Render(Vector2 backgroundSize, float minDepth, float maxDepth);

    /// <summary>
    /// Modifies the background and tile light colors of the background.
    /// </summary>
    public virtual void ModifyLightColors(ref Color backgroundColor, ref Color tileLightColor) { }
}
