using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.World.Subworlds;
using Terraria;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace NoxusBoss.Core.Graphics.TentInterior;

public class Constellation
{
    /// <summary>
    /// The overall area of the constellation.
    /// </summary>
    public readonly Vector2 Area;

    /// <summary>
    /// The set of all star points composed by the constellation.
    /// </summary>
    public readonly List<Vector2> StarPoints = [];

    /// <summary>
    /// The set of all lines composed by the constellation.
    /// </summary>
    public readonly List<LineSegment> Lines = [];

    public Constellation(Vector2 area, List<Vector2> starPoints, List<LineSegment> lines)
    {
        Area = area;
        StarPoints = starPoints;
        Lines = lines;
    }

    public void Render(Vector2 position, float scale, float opacity, float rotation)
    {
        float lineOpacity = opacity * 0.43f;
        Vector2 center = Area * 0.5f;
        foreach (LineSegment line in Lines)
        {
            Vector2 start = (line.Start - center).RotatedBy(rotation) * scale + position + Main.screenPosition;
            Vector2 end = (line.End - center).RotatedBy(rotation) * scale + position + Main.screenPosition;

            Vector2 midpoint = (line.Start + line.End) * 0.5f;

            float lineScaleFactor = scale * 1.85f;
            float hueShiftInterpolant = Cos01(midpoint.Distance(center) * 0.3f + Main.GlobalTimeWrappedHourly * 3.2f);
            float hueShift = SmoothStep(-0.4f, -0.6f, hueShiftInterpolant);
            DrawBloomLine(start, end, new Color(255, 255, 182, 0).HueShift(hueShift) * lineOpacity * 0.25f, lineScaleFactor * 50f);
            DrawBloomLine(start, end, new Color(255, 190, 151, 0).HueShift(hueShift) * lineOpacity * 0.5f, lineScaleFactor * 35f);
            DrawBloomLine(start, end, new Color(255, 196, 89, 0).HueShift(hueShift) * lineOpacity * 0.8f, lineScaleFactor * 11f);
        }

        UnifiedRandom rng = new UnifiedRandom(2775);
        foreach (Vector2 starPoint in StarPoints)
        {
            Vector2 absoluteStarPosition = (starPoint - center).RotatedBy(rotation) * scale + position;
            absoluteStarPosition += rng.NextVector2Circular(0.1f, 0f) * scale;

            RenderStar(rng, absoluteStarPosition, scale * Lerp(0.0091f, 0.03f, rng.NextFloat().Cubed()), opacity);
        }
    }

    private static void RenderStar(UnifiedRandom rng, Vector2 drawPosition, float scale, float opacity)
    {
        Texture2D starTexture = GennedAssets.Textures.GreyscaleTextures.Star.Value;

        Color bloomColor = Color.Blue * opacity * 0.3f;
        Color starColor = EternalGardenSkyStarRenderer.StarPalette.SampleColor(rng.NextFloat());
        starColor = Color.Lerp(starColor, Color.Wheat, 0.25f) * opacity;

        Main.spriteBatch.Draw(BloomCirclePinpoint, drawPosition, null, Color.Wheat with { A = 0 } * opacity * 0.3f, 0f, BloomCirclePinpoint.Size() * 0.5f, scale * 8f, 0, 0f);
        Main.spriteBatch.Draw(BloomCirclePinpoint, drawPosition, null, starColor with { A = 0 } * 0.4f, 0f, BloomCirclePinpoint.Size() * 0.5f, scale * 11f, 0, 0f);

        TwinkleParticle.DrawTwinkle(starTexture, drawPosition, 4, 0f, bloomColor with { A = 0 }, starColor with { A = 0 }, Vector2.One * scale, 0.85f);
    }

    private static void DrawBloomLine(Vector2 start, Vector2 end, Color color, float width)
    {
        if (start != end)
        {
            start -= Main.screenPosition;
            end -= Main.screenPosition;
            float rotation = (end - start).ToRotation() + PiOver2;
            Texture2D texture = BloomLine2;
            Vector2 scale = new Vector2(width, Vector2.Distance(start, end)) / texture.Size();
            Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height);
            Main.spriteBatch.Draw(texture, start, null, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
