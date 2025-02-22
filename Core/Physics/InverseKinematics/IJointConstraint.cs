namespace NoxusBoss.Core.Physics.InverseKinematics;

public interface IJointConstraint
{
    public double ApplyPenaltyLoss(Joint owner, float gradientDescentCompletion);
}
