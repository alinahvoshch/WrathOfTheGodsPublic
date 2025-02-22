using Luminance.Common.Easings;
using Terraria;
using static NoxusBoss.Content.NPCs.Bosses.NamelessDeity.NamelessDeityBoss;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering;

public class NamelessDeityWingSet
{
    /// <summary>
    /// The current rotation of the wings.
    /// </summary>
    public float Rotation
    {
        get;
        set;
    }

    /// <summary>
    /// The previous rotation of the wings.
    /// </summary>
    public float PreviousRotation
    {
        get;
        set;
    }

    /// <summary>
    /// The amount of squish to apply to wings.
    /// </summary>
    public float Squish
    {
        get;
        set;
    }

    /// <summary>
    /// Updates the wings.
    /// </summary>
    /// <param name="motionState">The motion that should be used when updating.</param>
    /// <param name="animationCompletion">The 0-1 interpolant for the animation completion.</param>
    public void Update(WingMotionState motionState, float animationCompletion)
    {
        // Cache the current wing rotation as the previous one.
        PreviousRotation = Rotation;

        // Positive rotations correspond to upward flaps.
        // Negative rotations correspond to downward flaps.
        PiecewiseCurve flap = new PiecewiseCurve().
            Add(EasingCurves.Cubic, EasingType.Out, 0.67f, 0.25f). // Upward rise.
            Add(EasingCurves.Quadratic, EasingType.In, -1.98f, 0.44f). // Flap.
            Add(EasingCurves.MakePoly(1.5f), EasingType.In, 0f, 1f); // Recovery.

        PiecewiseCurve squish = new PiecewiseCurve().
            Add(EasingCurves.Cubic, EasingType.Out, 0.15f, 0.25f). // Upward rise.
            Add(EasingCurves.Quintic, EasingType.In, 0.7f, 0.44f). // Flap.
            Add(EasingCurves.MakePoly(1.3f), EasingType.InOut, 0f, 1f); // Recovery.

        // It's easing curve time!
        switch (motionState)
        {
            case WingMotionState.RiseUpward:
                Squish = 0f;
                Rotation = (-0.6f).AngleLerp(0.36f, animationCompletion);
                break;
            case WingMotionState.Flap:
                Squish = squish.Evaluate(animationCompletion % 1f);
                Rotation = flap.Evaluate(animationCompletion % 1f);
                break;
        }
    }
}
