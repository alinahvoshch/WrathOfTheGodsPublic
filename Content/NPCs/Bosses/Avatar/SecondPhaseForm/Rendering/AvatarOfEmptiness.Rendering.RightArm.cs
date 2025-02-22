using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.Rendering;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    /// <summary>
    /// The left prop on the Avatar's right arm.
    /// </summary>
    public AvatarOfEmptinessProp LeftRightArmProp;

    /// <summary>
    /// The right prop on the Avatar's right arm.
    /// </summary>
    public AvatarOfEmptinessProp RightRightArmProp;

    /// <summary>
    /// The information holder for the Avatar's right arm.
    /// </summary>
    public AvatarOfEmptinessArmInfo RightArmInfo;

    private void RenderRightArm(Vector2 armDrawPosition)
    {
        if (!TargetsShouldBeProcessed)
            return;

        Texture2D arm = GennedAssets.Textures.SecondPhaseForm.FrontArmRight.Value;
        Texture2D forearm = GennedAssets.Textures.SecondPhaseForm.FrontForearmRight.Value;

        Vector2 startingPoint = NPC.Center + new Vector2(40f, -2f);
        Vector2 endingPoint = RightArmPosition;

        float scale = RightFrontArmScale;
        Vector2 elbowPosition = CalculateElbowPosition(startingPoint, endingPoint, scale * 520f, scale * 553f, false);

        float armRotation = startingPoint.AngleTo(elbowPosition) + Atan(arm.Height / (float)arm.Width);
        float forearmRotation = elbowPosition.AngleTo(endingPoint) + (Pi - Atan(forearm.Height / (float)forearm.Width));
        Vector2 forearmDrawPosition = armDrawPosition + startingPoint.SafeDirectionTo(elbowPosition) * TargetDownscaleFactor * scale * 540f;

        Color color = Color.White * NPC.Opacity * LeftFrontArmOpacity;
        Main.spriteBatch.Draw(arm, armDrawPosition, null, color, armRotation + Pi, arm.Size() * new Vector2(0.82f, 0.06f), scale * TargetDownscaleFactor, SpriteEffects.FlipVertically, 0f);
        Main.spriteBatch.Draw(forearm, forearmDrawPosition, null, color, forearmRotation + Pi, forearm.Size() * new Vector2(0.11f, 0.11f), scale * TargetDownscaleFactor, 0, 0f);

        Vector2 armDirection = (endingPoint - elbowPosition).SafeNormalize(Vector2.Zero);
        Vector2 armDirectionPerp = armDirection.RotatedBy(PiOver2);
        Vector2 handPosition = forearmDrawPosition + armDirection * TargetDownscaleFactor * scale * 430f + armDirectionPerp * TargetDownscaleFactor * scale * -208f;

        RightArmInfo = new(elbowPosition - armDirectionPerp * 90f, elbowPosition + armDirection * 1000f + armDirectionPerp * 50f);

        // Draw the first right finger.
        Texture2D[] rightFinger1Digits =
        [
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandRightFinger1Digit1").Value,
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandRightFinger1Digit2").Value
        ];
        ForwardKinematics(false, TargetDownscaleFactor * scale, color, handPosition + armDirection * TargetDownscaleFactor * scale * 174f + armDirectionPerp * TargetDownscaleFactor * scale * 220f,
        [
            new(0.5f, 0f),
            new(0.5f, 0.15f)
        ], rightFinger1Digits,
        [
            new(96f, 40f),
            Vector2.UnitX * 224f
        ], armDirection.RotatedBy(0.1f));

        // Draw the second right finger.
        Texture2D[] rightFinger2Digits =
        [
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandRightFinger2Digit1").Value,
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandRightFinger2Digit2").Value
        ];
        ForwardKinematics(false, TargetDownscaleFactor * scale, color, handPosition + armDirection * TargetDownscaleFactor * scale * 482f + armDirectionPerp * TargetDownscaleFactor * scale * 102f,
        [
            new(0f, 0f),
            new(0f, 0.15f)
        ], rightFinger2Digits,
        [
            new(120f, -60f),
            Vector2.UnitX * 124f
        ], armDirection.RotatedBy(0.74f));

        // Draw the third right finger.
        Texture2D[] rightFinger3Digits =
        [
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandRightFinger3Digit1").Value,
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandRightFinger3Digit2").Value
        ];
        ForwardKinematics(false, TargetDownscaleFactor * scale, color, handPosition + armDirection * TargetDownscaleFactor * scale * 693f + armDirectionPerp * TargetDownscaleFactor * scale * 264f,
        [
            new(0.75f, 0.16f),
            new(0.9f, 0.1f)
        ], rightFinger3Digits,
        [
            new(120f, 50f),
            Vector2.UnitX * 124f
        ], armDirection.RotatedBy(0.14f),
        [
            -HandBaseGraspAngle - HandGraspAngle * 1.1f,
            -HandBaseGraspAngle - HandGraspAngle * 2f
        ]);

        // Draw the fourth right finger.
        Texture2D[] rightFinger4Digits =
        [
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandRightFinger4Digit1").Value,
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandRightFinger4Digit2").Value
        ];
        ForwardKinematics(false, TargetDownscaleFactor * scale, color, handPosition + armDirection * TargetDownscaleFactor * scale * 640f + armDirectionPerp * TargetDownscaleFactor * scale * 308f,
        [
            new(0.87f, 0.1f),
            new(0.73f, 0.06f)
        ], rightFinger4Digits,
        [
            new(176f, 62f),
            Vector2.UnitX * 124f
        ], armDirection.RotatedBy(0.37f),
        [
            -HandBaseGraspAngle - HandGraspAngle * 1.3f,
            -HandBaseGraspAngle - HandGraspAngle * 2.4f + 0.24f
        ]);

        // Initialize props.
        LeftRightArmProp ??= new(GennedAssets.Textures.SecondPhaseForm.Bell, new(0.5f, 0.04f), -PiOver2);
        RightRightArmProp ??= new(GennedAssets.Textures.SecondPhaseForm.Beads1, new(0.5f, 0.04f), -PiOver2, Vector2.UnitY * -70f);

        // Move props.
        for (int i = 0; i < (NPC.IsABestiaryIconDummy ? 30 : 1); i++)
        {
            Vector2 leftPropStart = armDrawPosition + new Vector2(1f, -312f).RotatedBy(armRotation) * TargetDownscaleFactor;
            LeftRightArmProp.Start = leftPropStart;
            LeftRightArmProp.MoveTowards(leftPropStart - Vector2.UnitY * TargetDownscaleFactor * 110f);

            Vector2 rightPropStart = armDrawPosition + new Vector2(164f, -462f).RotatedBy(armRotation) * TargetDownscaleFactor;
            RightRightArmProp.Start = rightPropStart;
            RightRightArmProp.MoveTowards(rightPropStart - Vector2.UnitY * TargetDownscaleFactor * 280f);
        }

        // Draw props.
        LeftRightArmProp.Render(new Color(171, 2, 2) * RightFrontArmOpacity);
        RightRightArmProp.Render(new Color(171, 2, 2) * RightFrontArmOpacity);
    }
}
