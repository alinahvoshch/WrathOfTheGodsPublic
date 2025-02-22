using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Terraria;

namespace NoxusBoss.Core.Graphics.ScreenShake;

public class ShakeData
{
    /// <summary>
    /// How long this shake has gone on for, in frames.
    /// </summary>
    public int Time;

    /// <summary>
    /// How far along this shake is to being completed.
    /// </summary>
    public float LifetimeCompletion => Saturate(Time / (float)Lifetime);

    /// <summary>
    /// The intensity of the screen shake incurred by this shake instance.
    /// </summary>
    public float ShakeIntensity
    {
        get
        {
            float shakeIntensityInterpolant = 1f - LifetimeCompletion;
            return ShakeDissipationCurve.Evaluate(ShakeDissipationEasingType, 0f, MaxShake, shakeIntensityInterpolant);
        }
    }

    /// <summary>
    /// The curve which dictates how shakes dissipate over time.
    /// </summary>
    public EasingCurves.Curve ShakeDissipationCurve = EasingCurves.Quadratic;

    /// <summary>
    /// The type of easing to apply to shake dissipation in relation to <see cref="ShakeDissipationCurve"/>.
    /// </summary>
    public EasingType ShakeDissipationEasingType = EasingType.InOut;

    /// <summary>
    /// How long this shake should go on for, in frames.
    /// </summary>
    public int Lifetime;

    /// <summary>
    /// The maximum amount of screen shake that should be performed as this shake's animation progresses.
    /// </summary>
    public float MaxShake;

    /// <summary>
    /// The amount of vectoral directional bias applied by this shake.
    /// </summary>
    public Vector2 DirectionalBias = Vector2.One;

    public void Update() => Time = Utils.Clamp(Time + 1, 0, Lifetime);

    public ShakeData WithDissipationCurve(EasingCurves.Curve curve)
    {
        ShakeDissipationCurve = curve;
        return this;
    }

    public ShakeData WithEasingType(EasingType easingType)
    {
        ShakeDissipationEasingType = easingType;
        return this;
    }

    public ShakeData WithDistanceFadeoff(Vector2 sourcePosition, float standardDistanceThreshold = 776f, float zeroDistanceThreshold = 1560f)
    {
        float distanceFromSource = Main.LocalPlayer.Distance(sourcePosition);
        MaxShake *= InverseLerp(zeroDistanceThreshold, standardDistanceThreshold, distanceFromSource);
        return this;
    }

    public ShakeData WithDirectionalBias(Vector2 bias)
    {
        DirectionalBias = bias;
        return this;
    }
}
