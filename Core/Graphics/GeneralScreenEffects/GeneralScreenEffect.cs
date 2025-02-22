using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;

namespace NoxusBoss.Core.Graphics.GeneralScreenEffects;

public class GeneralScreenEffect
{
    /// <summary>
    /// Whether this screen effect is in use or not.
    /// </summary>
    public bool Active => GeneralScreenEffectSystem.activeEffects.Contains(this);

    /// <summary>
    /// How long this effect has existed for.
    /// </summary>
    public int Timer
    {
        get;
        internal set;
    }

    /// <summary>
    /// How long this screen effect should last before it goes inactive.
    /// </summary>
    public int Duration
    {
        get;
        internal set;
    }

    /// <summary>
    /// An intensity factor which dictates how powerful this screen effect should be overall.
    /// </summary>
    public float IntensityFactor
    {
        get;
        internal set;
    }

    /// <summary>
    /// The source position at which this screen effect should happen relative to, in world coordinates.
    /// </summary>
    public Vector2 SourcePosition
    {
        get;
        internal set;
    }

    /// <summary>
    /// The identifier for the <see cref="ManagedScreenFilter"/> that this screen effect manages.
    /// </summary>
    public readonly string ShaderKey;

    /// <summary>
    /// The action which dictates how this screen effect functions.
    /// </summary>
    public readonly PreparationFunction PreparationAction;

    public delegate void PreparationFunction(ManagedScreenFilter filter, Vector2 source, float intensity);

    public GeneralScreenEffect(string shaderKey, PreparationFunction preparationAction)
    {
        ShaderKey = shaderKey;
        PreparationAction = preparationAction;
    }

    /// <summary>
    /// Starts this screen effect.
    /// </summary>
    /// <param name="sourcePosition">The world position at which this screen effect should be started.</param>
    /// <param name="intensityFactor">The general intensity factor of this screen effect.</param>
    /// <param name="duration">The duration of this screen effect.</param>
    public void Start(Vector2 sourcePosition, float intensityFactor, int duration)
    {
        if (GeneralScreenEffectSystem.activeEffects.Contains(this))
            return;

        SourcePosition = sourcePosition;
        Timer = 1;
        Duration = duration;
        IntensityFactor = intensityFactor;
        GeneralScreenEffectSystem.activeEffects.Add(this);
    }
}
