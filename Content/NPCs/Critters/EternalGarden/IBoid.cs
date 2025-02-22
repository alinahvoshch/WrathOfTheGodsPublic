using Microsoft.Xna.Framework;

namespace NoxusBoss.Content.NPCs.Critters.EternalGarden;

public interface IBoid
{
    public int GroupID
    {
        get;
    }

    public float FlockmateDetectionRange
    {
        get;
    }

    public Vector2 BoidCenter
    {
        get;
    }

    public Rectangle BoidArea
    {
        get;
    }

    public List<BoidsManager.BoidForceApplicationRule> SimulationRules
    {
        get;
    }

    public ref Vector2 BoidVelocity
    {
        get;
    }

    public bool CurrentlyUsingBoidBehavior
    {
        get;
        set;
    }
}
