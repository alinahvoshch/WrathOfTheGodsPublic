using Microsoft.Xna.Framework;
using Terraria;
using static NoxusBoss.Core.Graphics.ScreenShatter.ScreenShatterSystem;

namespace NoxusBoss.Core.Graphics.ScreenShatter;

public class ScreenTriangleShard
{
    public int Time;

    public int SlowDuration;

    public bool Slow => Time <= SlowDuration;

    public float RotationX;

    public float RotationY;

    public float RotationZ;

    public Vector3 RotationalAxis;

    public Vector2 ScreenCoord1;

    public Vector2 ScreenCoord2;

    public Vector2 ScreenCoord3;

    public Vector2 DrawPosition;

    public Vector2 Velocity;

    public void Update()
    {
        float angularSlowdownInterpolant = InverseLerp(0.112f, 1f, Velocity.Length()) * (Slow ? 0.03f : 1f);
        RotationX += angularSlowdownInterpolant * RotationalAxis.X;
        RotationY += angularSlowdownInterpolant * RotationalAxis.Y;
        RotationZ += angularSlowdownInterpolant * RotationalAxis.Z;
        Velocity *= Slow ? 1.03f : 1.3f;
        DrawPosition += InverseLerp(0.97f, 0.55f, ShardOpacity) * Velocity * (Slow ? 0.01f : 1f);
        Time++;
    }

    public ScreenTriangleShard(int slowDuration, Vector2 a, Vector2 b, Vector2 c, Vector2 drawPosition)
    {
        ScreenCoord1 = a;
        ScreenCoord2 = b;
        ScreenCoord3 = c;
        DrawPosition = drawPosition;
        Velocity = (DrawPosition - ShatterFocalPoint).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(40f, 50f);
        RotationalAxis = new(Main.rand.NextFloatDirection() * 0.06f, Main.rand.NextFloatDirection() * 0.06f, Main.rand.NextFloatDirection() * 0.03f);
        SlowDuration = slowDuration;
    }
}
