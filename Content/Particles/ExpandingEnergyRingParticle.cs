using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.Particles;

public class ExpandingEnergyRingParticle : Particle
{
    /// <summary>
    /// The starting scale of this energy ring.
    /// </summary>
    public float StartingScale
    {
        get;
        set;
    }

    /// <summary>
    /// The ending scale of this energy ring.
    /// </summary>
    public float EndingScale
    {
        get;
        set;
    }

    /// <summary>
    /// The starting color of this energy ring.
    /// </summary>
    public Color StartingColor
    {
        get;
        set;
    }

    /// <summary>
    /// The ending color of this energy ring.
    /// </summary>
    public Color EndingColor
    {
        get;
        set;
    }

    /// <summary>
    /// The easing curve that dictates how the energy ring changes its scale over time.
    /// </summary>
    public EasingCurves.Curve ScaleEasingCurve
    {
        get;
        set;
    }

    /// <summary>
    /// The type of easing that should be applied in conjunction with the <see cref="ScaleEasingCurve"/>.
    /// </summary>
    public EasingType ScaleEasingType
    {
        get;
        set;
    }

    public override BlendState BlendState => BlendState.Additive;

    public override string AtlasTextureName => "NoxusBoss.ExpandingChromaticBurstParticle.png";

    public ExpandingEnergyRingParticle(Vector2 position, Color startingColor, Color endingColor, float startingScale, float endingScale, EasingCurves.Curve scaleEasingCurve, EasingType scaleEasingType, int lifeTime)
    {
        Position = position;
        Velocity = Vector2.Zero;
        DrawColor = startingColor;
        StartingColor = startingColor;
        EndingColor = endingColor;
        Scale = Vector2.One * startingScale;
        Lifetime = lifeTime;
        StartingScale = startingScale;
        EndingScale = endingScale;
        ScaleEasingCurve = scaleEasingCurve;
        ScaleEasingType = scaleEasingType;
        Rotation = Main.rand.NextFloat(TwoPi);
    }

    public override void Update()
    {
        Scale = Vector2.One * ScaleEasingCurve.Evaluate(ScaleEasingType, StartingScale, EndingScale, LifetimeRatio);
        DrawColor = Color.Lerp(StartingColor, EndingColor, LifetimeRatio);
    }
}
