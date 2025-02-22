using Microsoft.Xna.Framework;

namespace NoxusBoss.Core.DataStructures;

public readonly struct Triangle(Vector2 a, Vector2 b, Vector2 c)
{
    public readonly Vector2 Vertex1 = a;

    public readonly Vector2 Vertex2 = b;

    public readonly Vector2 Vertex3 = c;
}
