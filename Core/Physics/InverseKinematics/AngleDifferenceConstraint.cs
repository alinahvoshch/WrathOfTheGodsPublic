using Microsoft.Xna.Framework;
using Terraria;

namespace NoxusBoss.Core.Physics.InverseKinematics;

public class AngleDifferenceConstraint(float maxAngleDifference) : IJointConstraint
{
    /// <summary>
    /// The maximum angular quantity by which angles can exceed.
    /// </summary>
    public readonly float MaxAngleDifference = maxAngleDifference;

    public double ApplyPenaltyLoss(Joint owner, float gradientDescentCompletion)
    {
        if (owner.previousJoint is null)
            return 0f;

        Vector2 jointDirection = owner.Offset.SafeNormalize(Vector2.UnitY);
        Vector2 previousJointDirection = owner.previousJoint.Offset.SafeNormalize(Vector2.UnitY);
        float penalty = InverseLerp(MaxAngleDifference, Pi, jointDirection.AngleBetween(previousJointDirection)) * 9f;

        if (double.IsNaN(penalty))
            return 0f;

        return penalty;
    }
}
