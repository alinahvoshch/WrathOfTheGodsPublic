using Microsoft.Xna.Framework;

namespace NoxusBoss.Core.Physics;

public class MotionCharacteristicSolver
{
    // In the context of this system, y is analogous to the resulting position after modifying it with the differential equation that imbues a certain motion characteristic.
    // On the other hand, x is analogous to the base position, without any changes in motion.

    // In this simplest case, we have y = x, meaning no motion characteristic is applied at all.

    // For the purposes of this system, the (differential) equation to solve is as follows:
    // y + k1 * dy/dt + k2 * d^2y/dt^2 = x + k3 * dx/dt

    // k1, k2, and k3 are the parameters that control the simulation's motion.
    // The constructor provides less opaque which is translated into these parameters.

    private Vector2 y;

    private Vector2 velocity;

    private Vector2 previousTarget;

    private float k1;

    private float k2;

    private float k3;

    /// <summary>
    /// The speed at which the system responds to changes in input. Greater values equate to a sharper tendency for y to approach x.
    /// </summary>
    public float Frequency
    {
        get => 1f / (TwoPi * Sqrt(k2));
        set => ResetStateVariables(value, DampingCoefficient, InitialEaseResponse);
    }

    /// <summary>
    /// The coefficient which determines how much y exponentially decays to x, versus moving in an oscillating manner.
    /// </summary>
    public float DampingCoefficient
    {
        get => k1 / (Sqrt(k2) * 2f);
        set => ResetStateVariables(Frequency, value, InitialEaseResponse);
    }

    /// <summary>
    /// For 0<x<1, immediate motion occurs, similar to out easing. For values x > 1, overshoot occurs. For values x < 0, anticipation occurs.
    /// </summary>
    public float InitialEaseResponse
    {
        get => k3 / k1 * 2f;
        set => ResetStateVariables(Frequency, DampingCoefficient, value);
    }

    /// <summary>
    /// The amount of time between each frame in the simulation.
    /// </summary>
    public static float DeltaTime => 1f / 90f;

    /// <summary>
    /// Initiates a motion characteristic solver.
    /// </summary>
    /// <param name="position">The starting position of the system.</param>
    /// <param name="frequency">The speed at which the system responds to changes in input. Greater values equate to a sharper tendency for y to approach x.</param>
    /// <param name="dampingCoefficient">The coefficient which determines how much y exponentially decays to x, versus moving in an oscillating manner.</param>
    /// <param name="initialEaseResponse">For 0<x<1, immediate motion occurs, similar to out easing. For values x > 1, overshoot occurs. For values x < 0, anticipation occurs.</param>
    public MotionCharacteristicSolver(Vector2 position, float frequency, float dampingCoefficient, float initialEaseResponse)
    {
        y = position;
        ResetStateVariables(frequency, dampingCoefficient, initialEaseResponse);
    }

    private void ResetStateVariables(float frequency, float dampingCoefficient, float initialEaseResponse)
    {
        k1 = dampingCoefficient / (Pi * frequency);
        k2 = 1f / (TwoPi * frequency).Squared();
        k3 = initialEaseResponse * dampingCoefficient / (TwoPi * frequency);
    }

    /// <summary>
    /// Updates the overall system towards a given target position.
    /// </summary>
    /// <returns>The position</returns>
    public Vector2 Update(Vector2 target)
    {
        if (previousTarget == Vector2.Zero)
            previousTarget = target;

        // y = x + k3 * dx/dt - k1 * dy/dt - k2 * d^2y/dt^2
        // dy/dt = (x + k3 * dx/dt - y - k2 * d^2y/dt^2) / k1

        y += velocity * DeltaTime;

        float k2Stable = MathF.Max(MathF.Max(k2, DeltaTime.Squared() * 0.5f + DeltaTime * k1 * 0.5f), DeltaTime * k1);

        Vector2 xDerivative = (target - previousTarget) / DeltaTime;
        Vector2 newVelocity = (target + k3 * xDerivative - y - k1 * velocity) * DeltaTime / k2Stable;

        previousTarget = target;
        velocity = newVelocity.ClampLength(0f, 4000f);

        return y;
    }
}
