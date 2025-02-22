using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.BackgroundManagement;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.World.GameScenes.AvatarAppearances;
using Terraria;

namespace NoxusBoss.Core.Graphics.UI.GraphicalUniverseImager;

public class GraphicalUniverseImagerSky : Background
{
    /// <summary>
    /// How much longer this sky has to wait before beginning to disappear.
    /// </summary>
    public int DisableDelay
    {
        get;
        set;
    }

    /// <summary>
    /// The settings that dictate the rendering of this sky.
    /// </summary>
    public static GraphicalUniverseImagerSettings? RenderSettings
    {
        get;
        set;
    }

    /// <summary>
    /// The secondary settings that dictate the rendering of the eclipse.
    /// </summary>
    public static GraphicalUniverseImagerSettings.EclipseSecondaryAmbienceSetting? EclipseConfigOption
    {
        get
        {
            if (RenderSettings is null)
                return null;

            if (RenderSettings.Option is not null && RenderSettings.Option.LocalizationKey == RiftEclipseSkyScene.RiftEclipseGUIOption.LocalizationKey)
                return RenderSettings.EclipseAmbienceSettings;

            return null;
        }
    }

    /// <summary>
    /// The render target that contains the render contents of this background.
    /// </summary>
    public static InstancedRequestableTarget BackgroundTarget
    {
        get;
        set;
    }

    /// <summary>
    /// The effective intensity of this sky.
    /// </summary>
    public float EffectiveIntensity => InverseLerp(0f, 0.85f, Opacity);

    public override float CloudOpacity => InverseLerp(1f, 0f, Opacity);

    public override float Priority => 10f;

    protected override Background CreateTemplateEntity() => new GraphicalUniverseImagerSky();

    public override void SetStaticDefaults()
    {
        BackgroundTarget = new();
        Main.ContentThatNeedsRenderTargets.Add(BackgroundTarget);
    }

    public override void Update()
    {
        DisableDelay = Utils.Clamp(DisableDelay + ShouldBeActive.ToDirectionInt(), 0, 10);
        if (DisableDelay <= 0 && !ShouldBeActive)
            Opacity = Saturate(Opacity - 0.06f);
        if (ShouldBeActive)
            Opacity = Saturate(Opacity + 0.06f);
    }

    public override void Render(Vector2 backgroundSize, float minDepth, float maxDepth)
    {
        bool drawOnlyToForeground = RenderSettings?.Option?.DrawOnlyToForeground ?? false;
        bool atForeground = minDepth <= float.MinValue;
        if (drawOnlyToForeground && !atForeground)
            return;

        float minDepthCopy = minDepth;
        float maxDepthCopy = maxDepth;
        int identifier = (int)(minDepth % 1000f) + (int)(maxDepth * 1000f) % 1000000;
        BackgroundTarget.Request((int)backgroundSize.X, (int)backgroundSize.Y, identifier, () =>
        {
            Main.spriteBatch.Begin();
            RenderSettings?.Option?.BackgroundRenderFunction(minDepthCopy, maxDepthCopy, RenderSettings);
            Main.spriteBatch.End();
        });

        if (BackgroundTarget.TryGetTarget(identifier, out RenderTarget2D? target) && target is not null)
        {
            SetSpriteSortMode(SpriteSortMode.Deferred, Matrix.Identity);
            Main.spriteBatch.Draw(target, Vector2.Zero, Color.White * EffectiveIntensity);
            SetSpriteSortMode(SpriteSortMode.Deferred);
        }
    }

    public override void ModifyLightColors(ref Color backgroundColor, ref Color tileLightColor)
    {
        RenderSettings?.Option?.TileColorFunction?.Invoke(ref tileLightColor, ref backgroundColor);
    }
}
