using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.Rendering;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    /// <summary>
    /// The left prop on the Avatar's mandible.
    /// </summary>
    public AvatarOfEmptinessProp LeftMandibleProp;

    /// <summary>
    /// The right prop on the Avatar's mandible.
    /// </summary>
    public AvatarOfEmptinessProp RightMandibleProp;

    private void RenderHead(Vector2 start)
    {
        if (!TargetsShouldBeProcessed)
            return;

        float headScale = HeadScale / NPC.scale * TargetDownscaleFactor * 1.4f;
        Vector2 end = start + (HeadPosition - NPC.Center) * TargetDownscaleFactor * 0.6f;
        float headRotation = (end - start).ToRotation() - PiOver2;
        if (Cos(headRotation) < 0f)
            headRotation += Pi;
        float maskRotation = headRotation + MaskRotation;

        // Draw the neck segments.
        float[] neckOffsetInterpolants =
        [
            0.9f,
            0.65f,
            0.4f,
            0.14f
        ];

        // Calculate colors.
        Color[] segmentColors =
        [
            // First segment.
            Color.Lerp(new(99, 99, 99, 255), Color.White, InverseLerp(0.2f, 0.4f, NeckAppearInterpolant)) * (NeckAppearInterpolant >= 0.2f).ToInt(),

            // Second segment.
            Color.Lerp(new(99, 99, 99, 255), Color.White, InverseLerp(0.4f, 0.6f, NeckAppearInterpolant)) * (NeckAppearInterpolant >= 0.4f).ToInt(),

            // Third segment.
            Color.Lerp(new(99, 99, 99, 255), Color.White, InverseLerp(0.6f, 0.8f, NeckAppearInterpolant)) * (NeckAppearInterpolant >= 0.6f).ToInt(),

            // Fourth segment.
            Color.Lerp(new(99, 99, 99, 255), Color.White, InverseLerp(0.8f, 1f, NeckAppearInterpolant)) * (NeckAppearInterpolant >= 0.8f).ToInt(),

            // Head.
            Color.Lerp(new(99, 99, 99, 255), Color.White, InverseLerp(0.1f, 0.35f, NeckAppearInterpolant)) * InverseLerp(0f, 0.2f, NeckAppearInterpolant),
        ];

        Vector2 previousNeckDrawPosition = Vector2.Zero;
        for (int i = neckOffsetInterpolants.Length - 1; i >= 0; i--)
        {
            float neckOffsetInterpolant = neckOffsetInterpolants[i];
            Vector2 neckDrawPosition = Vector2.Lerp(start, end, neckOffsetInterpolant);
            float neckRotation = (Vector2.Lerp(start, end, neckOffsetInterpolant + 0.01f) - neckDrawPosition).ToRotation() - PiOver2;
            if (Cos(neckRotation) < 0f)
                neckRotation += Pi;

            Texture2D neck = GennedAssets.Textures.SecondPhaseForm.Neck1;
            switch (i)
            {
                case 1:
                    neck = GennedAssets.Textures.SecondPhaseForm.Neck2;
                    break;
                case 2:
                    neck = GennedAssets.Textures.SecondPhaseForm.Neck3;
                    break;
                case 3:
                    neck = GennedAssets.Textures.SecondPhaseForm.Neck4;
                    break;
            }

            Vector2 origin = neck.Size() * 0.5f;

            int invertedIndex = neckOffsetInterpolants.Length - i - 1;
            float neckSegmentOpacity = InverseLerp(invertedIndex / (float)neckOffsetInterpolants.Length, (invertedIndex + 0.1f) / neckOffsetInterpolants.Length, HeadOpacity);
            Main.spriteBatch.Draw(neck, neckDrawPosition, null, segmentColors[i] * neckSegmentOpacity, neckRotation, origin, headScale, 0, 0f);

            // Draw strings between neck offsets.
            float stringOpacity = neckSegmentOpacity.Cubed() * (segmentColors[Math.Min(i + 1, segmentColors.Length - 1)].A / 255f);
            switch (i)
            {
                case 2:
                    DrawTautString(previousNeckDrawPosition + new Vector2(-86f, 14f).RotatedBy(neckRotation) * headScale, neckDrawPosition + new Vector2(-114f, -10f).RotatedBy(neckRotation) * headScale, new Color(122, 0, 0) * stringOpacity);
                    DrawTautString(previousNeckDrawPosition + new Vector2(72f, 14f).RotatedBy(neckRotation) * headScale, neckDrawPosition + new Vector2(90f, -2f).RotatedBy(neckRotation) * headScale, new Color(122, 0, 0) * stringOpacity);
                    break;
                case 1:
                    DrawTautString(previousNeckDrawPosition + new Vector2(-120f, 18f).RotatedBy(neckRotation) * headScale, neckDrawPosition + new Vector2(-120f, -40f).RotatedBy(neckRotation) * headScale, new Color(117, 0, 0) * stringOpacity);
                    DrawTautString(previousNeckDrawPosition + new Vector2(98f, 18f).RotatedBy(neckRotation) * headScale, neckDrawPosition + new Vector2(124f, -1f).RotatedBy(neckRotation) * headScale, new Color(210, 36, 36) * stringOpacity);
                    break;
                case 0:
                    DrawTautString(previousNeckDrawPosition + new Vector2(-60f, -42f).RotatedBy(neckRotation) * headScale, neckDrawPosition + new Vector2(-184f, -10f).RotatedBy(neckRotation) * headScale, new Color(147, 0, 0) * stringOpacity);

                    DrawTautString(neckDrawPosition + new Vector2(-180f, 22f).RotatedBy(neckRotation) * headScale, end + new Vector2(-20f, 60f).RotatedBy(neckRotation) * headScale, new Color(172, 0, 32) * stringOpacity);
                    DrawTautString(neckDrawPosition + new Vector2(196f, 22f).RotatedBy(neckRotation) * headScale, end + new Vector2(66f, 68f).RotatedBy(neckRotation) * headScale, new Color(216, 39, 39) * stringOpacity);

                    break;
            }

            previousNeckDrawPosition = neckDrawPosition;
        }

        // Initialize props.
        LeftMandibleProp ??= new(GennedAssets.Textures.SecondPhaseForm.Lantern1, new(0.5f, 0.06f), -PiOver2);
        RightMandibleProp ??= new(GennedAssets.Textures.SecondPhaseForm.Lantern2, new(0.5f, 0.06f), -PiOver2);

        // Move props.
        for (int i = 0; i < (NPC.IsABestiaryIconDummy ? 30 : 1); i++)
        {
            LeftMandibleProp.Start = end + new Vector2(-240f, 390f).RotatedBy(headRotation) * TargetDownscaleFactor;
            LeftMandibleProp.MoveTowards(end + new Vector2(-240f, 540f).RotatedBy(headRotation) * TargetDownscaleFactor);
            RightMandibleProp.Start = end + new Vector2(232f, 444f).RotatedBy(headRotation) * TargetDownscaleFactor;
            RightMandibleProp.MoveTowards(end + new Vector2(232f, 586f).RotatedBy(headRotation) * TargetDownscaleFactor);
        }

        // Draw props.
        float headOpacity = InverseLerp(0.9f, 1f, HeadOpacity);
        LeftMandibleProp.Render(new Color(251, 0, 0) * headOpacity);
        RightMandibleProp.Render(new Color(251, 0, 0) * headOpacity);

        // Draw the head.
        Texture2D head = GennedAssets.Textures.SecondPhaseForm.Head.Value;
        Main.spriteBatch.Draw(head, end, null, segmentColors[^1] * headOpacity, headRotation, head.Size() * new Vector2(0.5f, 0f), headScale, 0, 0f);

        // Draw the mask over the head.
        Texture2D mask = GennedAssets.Textures.SecondPhaseForm.AvatarOfEmptiness.Value;
        Rectangle frame = mask.Frame(1, 24, 0, MaskFrame);
        Main.spriteBatch.Draw(mask, end + new Vector2(16f, 206f).RotatedBy(headRotation) * headScale, frame, segmentColors[^1] * headOpacity, maskRotation, frame.Size() * 0.5f, headScale, 0, 0f);
    }

    /// <summary>
    /// Draws a single taut string with a given color. For use with the Avatar's head.
    /// </summary>
    /// <param name="start">The starting point of the string.</param>
    /// <param name="end">The ending point of the string.</param>
    /// <param name="stringColor">The color of the string.</param>
    private static void DrawTautString(Vector2 start, Vector2 end, Color stringColor)
    {
        Main.spriteBatch.DrawLineBetter(start + Main.screenPosition, end + Main.screenPosition, stringColor, TargetDownscaleFactor * 3f);
    }
}
