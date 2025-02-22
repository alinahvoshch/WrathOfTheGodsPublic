using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    /// <summary>
    /// The factor by which all render targets are downscaled and updated for performance reasons.
    /// </summary>
    /// 
    /// <remarks>
    /// Smaller values equate to better performance, but less visual precision.
    /// </remarks>
    public static float TargetDownscaleFactor => 0.71f;

    /// <summary>
    /// Whether render target contents should be processed or not.
    /// </summary>
    public static bool TargetsShouldBeProcessed => true;

    /// <summary>
    /// Generates a new render target for the Avatar to use.
    /// </summary>
    /// <param name="width">The render target width.</param>
    /// <param name="height">The render target height.</param>
    internal static ManagedRenderTarget CreateTarget(int width, int height)
    {
        return new(false, (_, _2) =>
        {
            return new(Main.instance.GraphicsDevice, (int)(width * TargetDownscaleFactor), (int)(height * TargetDownscaleFactor));
        });
    }

    private void ProcessTargets()
    {
        ProcessTargets_Body();
        ProcessTargets_ShadowyParts();
        ProcessTargets_Silhouette();
        ProcessTargets_Final();
    }

    public void ForwardKinematics(bool leftArm, float scale, Color color, Vector2 start, Vector2[] origins, Texture2D[] textures, Vector2[] lengths, Vector2 startingDirection, float[]? angularOffsets = null)
    {
        // Initialize draw data for the first limb
        Vector2 drawPosition = start;
        Vector2 currentDirection = startingDirection;

        for (int i = 0; i < textures.Length; i++)
        {
            // Calculate the rotation of the current limb based on the current direction, along with an optional offset.
            float rotation = currentDirection.ToRotation() - PiOver2 + (angularOffsets?[i] ?? 0f);

            // Draw the line.
            Main.spriteBatch.Draw(textures[i], drawPosition, null, color * NPC.Opacity * (leftArm ? LeftFrontArmOpacity : RightFrontArmOpacity), rotation, textures[i].Size() * origins[i], scale, 0, 0f);

            // Prepare for the next iteration by updating the direction in accordance with the rotation (Which may have been changed by an angular offset) as well as
            // by moving forward to the next limb.
            // This results in a propagating effect where upcoming limbs depend on the direction and position of old ones.
            currentDirection = (rotation + PiOver2).ToRotationVector2();
            drawPosition += currentDirection * lengths[i].X * scale + currentDirection.RotatedBy(PiOver2) * lengths[i].Y * scale;
        }
    }
}
