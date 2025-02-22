using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.Rendering;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    /// <summary>
    /// The left prop on the Avatar's left arm.
    /// </summary>
    public AvatarOfEmptinessProp LeftLeftArmProp;

    /// <summary>
    /// The right prop on the Avatar's left arm.
    /// </summary>
    public AvatarOfEmptinessProp RightLeftArmProp;

    /// <summary>
    /// The information holder for the Avatar's left arm.
    /// </summary>
    public AvatarOfEmptinessArmInfo LeftArmInfo;

    private void RenderLeftArm(Vector2 armDrawPosition)
    {
        if (!TargetsShouldBeProcessed)
            return;

        Texture2D arm = GennedAssets.Textures.SecondPhaseForm.FrontArmLeft.Value;
        Texture2D forearm = GennedAssets.Textures.SecondPhaseForm.FrontForearmLeft.Value;

        float scale = LeftFrontArmScale;
        Vector2 startingPoint = NPC.Center + new Vector2(-40f, -2f);
        Vector2 endingPoint = LeftArmPosition;
        Vector2 elbowPosition = CalculateElbowPosition(startingPoint, endingPoint, scale * 434f, scale * 553f, true);

        float armRotation = startingPoint.AngleTo(elbowPosition) - Atan(arm.Height / (float)arm.Width);
        float forearmRotation = elbowPosition.AngleTo(endingPoint) - (Pi - Atan(forearm.Height / (float)forearm.Width));
        Vector2 forearmDrawPosition = armDrawPosition + startingPoint.SafeDirectionTo(elbowPosition) * TargetDownscaleFactor * scale * 540f;

        Color color = Color.White * NPC.Opacity * LeftFrontArmOpacity;
        Main.spriteBatch.Draw(arm, armDrawPosition, null, color, armRotation + Pi, arm.Size() * new Vector2(0.82f, 0.94f), TargetDownscaleFactor * scale, 0, 0f);
        Main.spriteBatch.Draw(forearm, forearmDrawPosition, null, color, forearmRotation, forearm.Size() * new Vector2(0.81f, 0.08f), TargetDownscaleFactor * scale, 0, 0f);

        Vector2 armDirection = (endingPoint - elbowPosition).SafeNormalize(Vector2.Zero);
        Vector2 armDirectionPerp = armDirection.RotatedBy(PiOver2);
        Vector2 handPosition = forearmDrawPosition + armDirection * TargetDownscaleFactor * scale * 630f + armDirectionPerp * scale * TargetDownscaleFactor * -208f;

        LeftArmInfo = new(elbowPosition + armDirectionPerp * 130f, elbowPosition + armDirection * 940f - armDirectionPerp * 400f);

        // Draw the first left finger.
        Texture2D[] leftFinger1Digits =
        [
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandLeftFinger1Digit1").Value,
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandLeftFinger1Digit2").Value,
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandLeftFinger1Digit3").Value,
        ];
        this.ForwardKinematics(true, TargetDownscaleFactor * scale, color, handPosition + (armDirection * 250f + armDirectionPerp * -100f) * TargetDownscaleFactor * scale,
        [
            new(0.5f, 0f),
            new(0.5f, 0.15f),
            new(0f, 0f),
        ], leftFinger1Digits,
        [
            Vector2.UnitX * 138f,
            Vector2.UnitX * 124f,
            Vector2.UnitX * 158f
        ], armDirection.RotatedBy(-0.2f),
        [
            HandBaseGraspAngle + (RingFingerAngle ?? HandGraspAngle),
            HandBaseGraspAngle + (RingFingerAngle ?? HandGraspAngle) * 2f,
            HandBaseGraspAngle + (RingFingerAngle ?? HandGraspAngle) * 3f
        ]);

        // Draw the left hand.
        Texture2D leftHand = LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandLeft").Value;
        Main.spriteBatch.Draw(leftHand, handPosition, null, color, forearmRotation, leftHand.Size() * new Vector2(0.5f, 0f), TargetDownscaleFactor * scale, 0, 0f);

        // Draw the second left finger.
        Texture2D[] leftFinger2Digits =
        [
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandLeftFinger2Digit1").Value,
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandLeftFinger2Digit2").Value,
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandLeftFinger2Digit3").Value,
        ];
        this.ForwardKinematics(true, TargetDownscaleFactor * scale, color, handPosition + armDirection * TargetDownscaleFactor * scale * 240f + armDirectionPerp * TargetDownscaleFactor * scale * -174f,
        [
            new(0.5f, 0f),
            new(0.5f, 0f),
            new(0f, 0f),
        ], leftFinger2Digits,
        [
            Vector2.UnitX * 84f,
            Vector2.UnitX * 184f,
            Vector2.UnitX * 158f
        ], armDirection.RotatedBy(-0.2f),
        [
            HandBaseGraspAngle + (IndexFingerAngle ?? HandGraspAngle),
            HandBaseGraspAngle + (IndexFingerAngle ?? HandGraspAngle) * 2f,
            HandBaseGraspAngle + (IndexFingerAngle ?? HandGraspAngle) * 3f
        ]);

        // Draw the third left finger.
        Texture2D[] leftFinger3Digits =
        [
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandLeftFinger3Digit1").Value,
            LazyAsset<Texture2D>.FromPath("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Hands/FrontHandLeftFinger3Digit2").Value
        ];
        this.ForwardKinematics(true, TargetDownscaleFactor * scale, color, handPosition + armDirection * TargetDownscaleFactor * scale * 136f + armDirectionPerp * TargetDownscaleFactor * scale * -180f,
        [
            new(0f, 0.05f),
            new(0.5f, 0.1f)
        ], leftFinger3Digits,
        [
            new Vector2(102f, -100f),
            Vector2.UnitX * 102f
        ], armDirection.RotatedBy(-0.39f),
        [
            -HandBaseGraspAngle - (ThumbAngle ?? HandGraspAngle) * 1.3f,
            -HandBaseGraspAngle - (ThumbAngle ?? HandGraspAngle) * 2.5f
        ]);

        // Initialize props.
        LeftLeftArmProp ??= new(GennedAssets.Textures.SecondPhaseForm.Beads3, new(0.5f, 0.04f), -PiOver2);
        RightLeftArmProp ??= new(GennedAssets.Textures.SecondPhaseForm.Beads2, new(0.5f, 0.04f), -PiOver2, Vector2.UnitY * -70f);

        // Move props.
        for (int i = 0; i < (NPC.IsABestiaryIconDummy ? 30 : 1); i++)
        {
            Vector2 leftPropStart = armDrawPosition + new Vector2(206f, 508f).RotatedBy(armRotation) * TargetDownscaleFactor;
            LeftLeftArmProp.Start = leftPropStart;
            LeftLeftArmProp.MoveTowards(leftPropStart - Vector2.UnitY * TargetDownscaleFactor * 300f);

            Vector2 rightPropStart = armDrawPosition + new Vector2(-12f, 320f).RotatedBy(armRotation) * TargetDownscaleFactor;
            RightLeftArmProp.Start = rightPropStart;
            RightLeftArmProp.MoveTowards(rightPropStart - Vector2.UnitY * TargetDownscaleFactor * 280f);
        }

        // Draw props.
        LeftLeftArmProp.Render(new Color(216, 39, 39) * LeftFrontArmOpacity);
        RightLeftArmProp.Render(new Color(209, 13, 16) * LeftFrontArmOpacity);
    }
}
