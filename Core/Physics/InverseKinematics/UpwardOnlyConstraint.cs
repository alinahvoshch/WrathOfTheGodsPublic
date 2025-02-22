using Microsoft.Xna.Framework;
using Terraria;

namespace NoxusBoss.Core.Physics.InverseKinematics;

public class UpwardOnlyConstraint : IJointConstraint
{
    private Vector2 upDirection;

    public UpwardOnlyConstraint(Vector2? upDirection = null)
    {
        this.upDirection = upDirection ?? -Vector2.UnitY;
    }

    public double ApplyPenaltyLoss(Joint owner, float gradientDescentCompletion)
    {
        if (owner.previousJoint is null)
            return 0f;

        Vector2 jointDirection = owner.Offset.SafeNormalize(Vector2.UnitY);

        // Relax penalties the further into the gradient descent iterations the process is.
        // This allows for angles to properly unfold initially, and once they are unfolded, a solution can be reached based on that initial configuration, rather than having
        // angle corrections get in the way of the ultimate goal of having the end effector reach a desired point.
        float penaltyFactor = InverseLerp(0.4f, 0f, gradientDescentCompletion) * 1500f;
        float penalty = InverseLerp(0.01f, Pi, jointDirection.AngleBetween(upDirection)) * penaltyFactor;

        if (double.IsNaN(penalty))
            return 0f;

        return penalty;
    }
}
