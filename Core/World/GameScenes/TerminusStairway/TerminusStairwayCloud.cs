using Microsoft.Xna.Framework;
using Terraria;

namespace NoxusBoss.Core.World.GameScenes.TerminusStairway;

public class TerminusStairwayCloud
{
    /// <summary>
    /// The texture variant of this cloud.
    /// </summary>
    public int TextureVariant;

    /// <summary>
    /// The scale of this cloud.
    /// </summary>
    public float Scale;

    /// <summary>
    /// Whether this cloud should render atop floating islands in the background or not.
    /// </summary>
    public bool InFrontOfFloatingIslands;

    /// <summary>
    /// The position of this cloud.
    /// </summary>
    public Vector2 Position;

    public TerminusStairwayCloud(Vector2 position, bool inFrontOfFloatingIslands)
    {
        Position = position;
        InFrontOfFloatingIslands = inFrontOfFloatingIslands;
        TextureVariant = Main.rand.Next(22);
        Scale = Main.rand.NextFloat(0.35f, 1.4f);
    }
}
