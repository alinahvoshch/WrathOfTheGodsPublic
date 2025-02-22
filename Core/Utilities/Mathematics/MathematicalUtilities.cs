using Microsoft.Xna.Framework;

namespace NoxusBoss.Core.Utilities;

public static partial class Utilities
{
    /// <summary>
    /// Returns the result of a number that steps incrementally towards some ideal value.
    /// </summary>
    /// <param name="x">The input number.</param>
    /// <param name="idealValue">The ideal value.</param>
    /// <param name="stepRate">The amount by which the number increments.</param>
    public static float StepTowards(this float x, float idealValue, float stepRate)
    {
        float stepDirection = Sign(idealValue - x);
        x += stepDirection * stepRate;

        if (Distance(x, idealValue) <= stepRate)
            return idealValue;

        return x;
    }

    /// <summary>
    /// Subdivides a rectangle into frames.
    /// </summary>
    /// <param name="rectangle">The base rectangle.</param>
    /// <param name="horizontalFrames">The amount of horizontal frames to subdivide into.</param>
    /// <param name="verticalFrames">The amount of vertical frames to subdivide into.</param>
    /// <param name="frameX">The index of the X frame.</param>
    /// <param name="frameY">The index of the Y frame.</param>
    public static Rectangle Subdivide(this Rectangle rectangle, int horizontalFrames, int verticalFrames, int frameX, int frameY)
    {
        int width = rectangle.Width / horizontalFrames;
        int height = rectangle.Height / verticalFrames;
        return new Rectangle(rectangle.Left + width * frameX, rectangle.Top + height * frameY, width, height);
    }
}
