using Microsoft.Xna.Framework;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Rendering;

public class BurnMarkImpact
{
    /// <summary>
    /// The position of the impact relative to Mars.
    /// </summary>
    public Vector2 RelativePosition;

    /// <summary>
    /// How long this impact has existed for, in frames.
    /// </summary>
    public int Time;

    /// <summary>
    /// The lifetime of this impact.
    /// </summary>
    public int Lifetime;

    /// <summary>
    /// A unique value specific to this impact. Used to create vfx variance.
    /// </summary>
    public float Seed;

    /// <summary>
    /// The scale of the burn mark impact.
    /// </summary>
    public float Scale;
}
