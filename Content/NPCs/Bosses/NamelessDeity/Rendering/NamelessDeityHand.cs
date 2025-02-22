using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.ID;
using NamelessDeityArmVariant = NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.NamelessDeityArmsRegistry.ArmRenderData;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering;

public class NamelessDeityHand
{
    /// <summary>
    /// The overriding visual direction of this hand.
    /// </summary>
    /// <remarks>
    /// A value of 0 indicates that no overriding shall occur.<br></br>
    /// When this override is not in use the direction corresponds to whichever side this hand is relative to its owner.
    /// </remarks>
    public int DirectionOverride;

    /// <summary>
    /// Whether the visual direction of this hand should be flipped.
    /// </summary>
    public bool VisuallyFlipHand;

    /// <summary>
    /// The overriding direction of this hand when deciding whether inverse kinematic calculations need to flip angles.
    /// </summary>
    /// <remarks>
    /// When this override is not in use the flip corresponds to whichever side this hand is relative to its owner.
    /// </remarks>
    public bool? ArmInverseKinematicsFlipOverride;

    /// <summary>
    /// The type of hand that should be drawn, such as standard drawing or a fist.
    /// </summary>
    public NamelessDeityHandType HandType;

    /// <summary>
    /// The rotational offset that this hand should be drawn with.
    /// </summary>
    public float RotationOffset;

    /// <summary>
    /// Whether this hand should do contact damage or not.
    /// </summary>
    public bool CanDoDamage;

    /// <summary>
    /// Whether this hand is currently wielding a glock or not.
    /// </summary>
    public bool HasGlock;

    /// <summary>
    /// Whether this hand has arms and consequently should be attached to Nameless.
    /// </summary>
    public bool HasArms;

    /// <summary>
    /// The forearm inverse kinematics length factor for this hand.
    /// </summary>
    public float ForearmIKLengthFactor;

    /// <summary>
    /// The opacity of this hand.
    /// </summary>
    public float Opacity
    {
        get;
        set;
    } = 1f;

    /// <summary>
    /// The scale factor of this hand.
    /// </summary>
    public float ScaleFactor = 1f;

    /// <summary>
    /// The center position of this hand.
    /// </summary>
    /// <remarks>
    /// This is distinct from <see cref="ActualCenter"/> in that this position is <b>not</b> restricted by inverse kinematics. This variable is where the hand <i>wants to be, rather than where it actually is</i>.
    /// </remarks>
    public Vector2 FreeCenter;

    /// <summary>
    /// The center position of this hand, restricted by the limitations of the reach of inverse kinematics.
    /// </summary>
    public Vector2 ActualCenter;

    /// <summary>
    /// The direction of this hand, as dictated by inverse kinematics calculations. This is defined based on the rotation that the hand is drawn with.
    /// </summary>
    public Vector2 Direction
    {
        get;
        private set;
    }

    /// <summary>
    /// The velocity of this hand.
    /// </summary>
    public Vector2 Velocity;

    /// <summary>
    /// The general scaling factor applied to arms.
    /// </summary>
    public const float ArmScale = 0.5f;

    /// <summary>
    /// The offset angle used when dictating the positioning of hands on forearms.
    /// </summary>
    public static readonly float HandPositionalOffsetAngle = ToRadians(9.75f);

    /// <summary>
    /// The standard origin that should be used when drawing any and all arms.
    /// </summary>
    public static readonly Vector2 StandardArmOrigin = new Vector2(236f, 192f);

    /// <summary>
    /// The standard origin that should be used when drawing elbow joint connectors for forearms.
    /// </summary>
    public static readonly Vector2 StandardElbowJointOrigin = new Vector2(74f, 54f);

    /// <summary>
    /// A condensed representation of hand draw data, containing a texture and frame.
    /// </summary>
    /// <param name="Texture">The texture that the hand should draw.</param>
    /// <param name="Frame">The frame that should be used when drawing the hand.</param>
    public record HandTextureDrawSet(Texture2D Texture, Rectangle Frame);

    /// <summary>
    /// A condensed representation of hand/arm draw data, containing the standard textures for the hand, arm, and forearm.
    /// </summary>
    /// <param name="HandTexture">The hand texture.</param>
    /// <param name="ArmTexture">The arm texture.</param>
    /// <param name="ForearmTexture">The forearm texture.</param>
    public record HandArmTextureSet(Texture2D HandTexture, Texture2D ArmTexture, Texture2D ForearmTexture);

    /// <summary>
    /// A collection of draw information necessary for the drawing of arms and forearms.
    /// </summary>
    /// <param name="ArmLength">The length of the arm.</param>
    /// <param name="ArmRotation">The rotation of the arm.</param>
    /// <param name="ForearmRotation">The rotation of the forearm.</param>
    /// <param name="ElbowRotateAngle">The positional angular offset angle applied to the elbow, as a consequence of the fact that the arm sprite does not point forward to the right.</param>
    /// <param name="ArmOrigin">The origin pivot point of the arm.</param>
    /// <param name="ForearmOrigin">The origin pivot point of the forearm arm.</param>
    /// <param name="ElbowJointOrigin">The origin pivot point of the elbow joint.</param>
    /// <param name="ArmStartingPosition">The starting position of the arm.</param>
    /// <param name="ElbowPosition">The elbow position of arm/forearm/hand set.</param>
    /// <param name="ArmDirection">The visual flip direction of the arm.</param>
    public record ArmAndForearmDrawData(float ArmLength, float ArmRotation, float ForearmRotation, float ElbowRotateAngle, Vector2 ArmOrigin, Vector2 ForearmOrigin, Vector2 ElbowJointOrigin, Vector2 ArmStartingPosition, Vector2 ElbowPosition, SpriteEffects ArmDirection);

    public NamelessDeityHand(Vector2 spawnPosition, bool hasArms)
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        FreeCenter = spawnPosition;
        HasArms = hasArms;
    }

    /// <summary>
    /// Updates this hand.
    /// </summary>
    public void Update()
    {
        Opacity = Saturate(Opacity + 0.03f);
        FreeCenter += Velocity;
    }

    /// <summary>
    /// Draws the hand and other parts, such as the glock (assuming it's being used) and arms.
    /// </summary>
    /// <param name="screenPos">The screen position.</param>
    /// <param name="ownerCenter">The Nameless Deity's center position.</param>
    /// <param name="zPositionDarkness">The amount of darkness this hand should have a consequence of being in the background.</param>
    /// <param name="opacityFactor">The opacity factor of the hand. This is independent of the <see cref="Opacity"/> value.</param>
    /// <param name="textureName">The hand's texture variant.</param>
    /// <param name="armTextureSet">The set of hand, arm, and forearm textures.</param>
    public void Draw(Vector2 screenPos, Vector2 ownerCenter, float zPositionDarkness, float opacityFactor, string textureName, HandArmTextureSet armTextureSet)
    {
        float handScale = ScaleFactor * 0.5f;
        float handRotation = RotationOffset;
        float generalOpacity = Opacity.Cubed() * opacityFactor;
        Color handColor = Color.Lerp(Color.White, Color.DarkGray, zPositionDarkness) * generalOpacity;
        Vector2 drawPosition = FreeCenter - screenPos;
        HandTextureDrawSet handTextureData = DetermineHandDrawData(armTextureSet.HandTexture);
        SpriteEffects direction = FreeCenter.X > ownerCenter.X ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        if (DirectionOverride != 0)
            direction = (-DirectionOverride).ToSpriteDirection();

        // If the hand doesn't have a palm, draw the arm.
        if (HasArms)
        {
            DrawArmAndForearm(screenPos, ownerCenter, drawPosition, handColor, textureName, direction, armTextureSet, ref handRotation);
            drawPosition = ActualCenter - screenPos;
        }

        // Store the direction of the hand.
        Direction = handRotation.ToRotationVector2();
        if (direction.HasFlag(SpriteEffects.FlipHorizontally))
            Direction *= -1f;

        // Draw the glock, assuming it's in use.
        if (HasGlock)
        {
            DrawGlock(drawPosition, handColor, handRotation - RotationOffset, handScale * 0.9f, direction);
            handRotation += 0.5f;
        }

        // Draw hands.
        DrawHands(handTextureData.Texture, drawPosition, handColor, handTextureData.Frame, handRotation, handScale, direction);
    }

    /// <summary>
    /// Determines what hand frame and texture should be used.
    /// </summary>
    /// <param name="standardHandTexture">The standard hand variant's texture.</param>
    private HandTextureDrawSet DetermineHandDrawData(Texture2D standardHandTexture)
    {
        switch (HandType)
        {
            case NamelessDeityHandType.ClosedFist:
                Texture2D fistTexture = GennedAssets.Textures.NamelessDeity.NamelessDeityHandFist2.Value;
                return new(fistTexture, fistTexture.Frame());

            case NamelessDeityHandType.OpenPalm:
                Texture2D palmTexture = GennedAssets.Textures.NamelessDeity.NamelessDeityPalm.Value;
                return new(palmTexture, palmTexture.Frame());

            default:
                return new(standardHandTexture, standardHandTexture.Frame());
        }
    }

    /// <summary>
    /// Draws the arm and forearm.
    /// </summary>
    /// <param name="screenPos">The screen position.</param>
    /// <param name="ownerCenter">The Nameless Deity's center position.</param>
    /// <param name="desiredHandPosition">The position that the arm should reach towards.</param>
    /// <param name="armColor">The color of the arm.</param>
    /// <param name="textureName">The hand's texture variant.</param>
    /// <param name="handDirection">The facing direction of the hand.</param>
    /// <param name="armTextureSet">The set of hand, arm, and forearm textures.</param>
    /// <param name="handRotation">The rotation of the hand.</param>
    private void DrawArmAndForearm(Vector2 screenPos, Vector2 ownerCenter, Vector2 desiredHandPosition, Color armColor, string textureName, SpriteEffects handDirection, HandArmTextureSet armTextureSet, ref float handRotation)
    {
        Texture2D armTexture = armTextureSet.ArmTexture;
        Texture2D forearmTexture = armTextureSet.ForearmTexture;
        Texture2D elbowJointTexture = GennedAssets.Textures.NamelessDeity.ElbowJoint.Value;

        bool leftOfOwner = handDirection.HasFlag(SpriteEffects.FlipHorizontally);
        NamelessDeityArmVariant arm = NamelessDeityArmsRegistry.ArmLookup[textureName];
        ArmAndForearmDrawData drawData = CalculateArmAndForearmDrawData(leftOfOwner, screenPos, ownerCenter, desiredHandPosition, arm, armTextureSet);

        // Adjust the draw position be at the end of the arm, now that the IK information is known.
        ActualCenter = CalculateActualHandPosition(arm, leftOfOwner, drawData.ForearmRotation, drawData.ArmLength, drawData.ElbowPosition) + screenPos;

        Main.spriteBatch.Draw(armTexture, drawData.ArmStartingPosition, null, armColor, drawData.ArmRotation - drawData.ElbowRotateAngle, drawData.ArmOrigin, ArmScale, drawData.ArmDirection, 0f);
        Main.spriteBatch.Draw(forearmTexture, drawData.ElbowPosition, null, armColor, drawData.ForearmRotation, drawData.ForearmOrigin, ArmScale, drawData.ArmDirection, 0f);
        Main.spriteBatch.Draw(elbowJointTexture, drawData.ElbowPosition, null, armColor, 0f, drawData.ElbowJointOrigin, ArmScale, drawData.ArmDirection, 0f);

        // Now that the arms are drawn and rotation information is known, it's important that the hand rotation be updated for later in the Draw method.
        handRotation += drawData.ForearmRotation;
        if (handDirection.HasFlag(SpriteEffects.FlipHorizontally))
            handRotation += Pi;
    }

    /// <summary>
    /// Calculates an assortment of compact information for arm and forearm drawing.
    /// </summary>
    /// <param name="leftOfOwner">Whether the arm is to the left of its owner.</param>
    /// <param name="screenPos">The screen position.</param>
    /// <param name="ownerCenter">The Nameless Deity's center position.</param>
    /// <param name="desiredHandPosition">The position that the arm should reach towards.</param>
    /// <param name="arm">The arm variant.</param>
    /// <param name="armTextureSet">The set of hand, arm, and forearm textures.</param>
    private ArmAndForearmDrawData CalculateArmAndForearmDrawData(bool leftOfOwner, Vector2 screenPos, Vector2 ownerCenter, Vector2 desiredHandPosition, NamelessDeityArmVariant arm, HandArmTextureSet armTextureSet)
    {
        Texture2D armTexture = armTextureSet.ArmTexture;
        Texture2D forearmTexture = armTextureSet.ForearmTexture;
        Texture2D elbowJointTexture = GennedAssets.Textures.NamelessDeity.ElbowJoint.Value;

        float armLength = ArmScale * 682f;
        float forearmLength = ArmScale * forearmTexture.Width * ForearmIKLengthFactor;

        bool flipIKAngles = ArmInverseKinematicsFlipOverride ?? leftOfOwner;
        Vector2 armStart = ownerCenter + new Vector2((FreeCenter.X - ownerCenter.X).NonZeroSign() * 120f, 40f) - screenPos;
        Vector2 elbowPosition = CalculateElbowPosition(armStart, desiredHandPosition, armLength, forearmLength, flipIKAngles);

        SpriteEffects armDirection = leftOfOwner ? SpriteEffects.FlipVertically : SpriteEffects.None;
        Vector2 forearmOrigin = FlipOriginByDirection(arm.ForearmOrigin, forearmTexture, armDirection);
        Vector2 armOrigin = FlipOriginByDirection(StandardArmOrigin, armTexture, armDirection);
        Vector2 elbowJointOrigin = FlipOriginByDirection(StandardElbowJointOrigin, elbowJointTexture, armDirection);

        // The 0.8 discrepancy factor below is an estimate that attempts to loosely account for irrelevant space that goes into forearm textures, potentially distorting the width/height calculations.
        float elbowRotateAngle = Atan(armTexture.Height / (float)armTexture.Width);
        float forearmSpriteOffsetAngle = Atan(forearmTexture.Height / (float)forearmTexture.Width) * 0.8f;
        if (leftOfOwner)
        {
            forearmSpriteOffsetAngle *= -1f;
            elbowRotateAngle *= -1f;
        }

        // Arm sprites are not at a default 0 angle, they face downwards a bit. As such, the elbow position must be rotated slightly to correct for this.
        elbowPosition = elbowPosition.RotatedBy(elbowRotateAngle, armStart);

        float armRotation = (elbowPosition - armStart).ToRotation();
        float angleFromElbowToHand = (desiredHandPosition - elbowPosition).ToRotation();
        float forearmRotation = angleFromElbowToHand + forearmSpriteOffsetAngle;

        return new(armLength, armRotation, forearmRotation, elbowRotateAngle, armOrigin, forearmOrigin, elbowJointOrigin, armStart, elbowPosition, armDirection);
    }

    /// <summary>
    /// Calculates the effective hand position in screen space, accounting for inverse kinematics discrepancies.
    /// </summary>
    /// <remarks>
    /// This is necessary for the purpose of getting the true final position when a desired position is out of reach of the inverse kinematics system.
    /// </remarks>
    /// <param name="arm">The arm variant.</param>
    /// <param name="leftOfOwner">Whether the hand is to the left of its owner or not.</param>
    /// <param name="handTexture">The hand texture for the arm.</param>
    /// <param name="forearmRotation">The rotation of the forearm.</param>
    /// <param name="forearmRotation">The length of the forearm.</param>
    /// <param name="elbowPosition">The elbow position, as calculated from the inverse kinematics.</param>
    private static Vector2 CalculateActualHandPosition(NamelessDeityArmVariant arm, bool leftOfOwner, float forearmRotation, float forearmLength, Vector2 elbowPosition)
    {
        float angleFromElbowToHand = forearmRotation + leftOfOwner.ToDirectionInt() * HandPositionalOffsetAngle;
        Vector2 directionFromElbowToHand = angleFromElbowToHand.ToRotationVector2();
        Vector2 elbowToHandOffset = directionFromElbowToHand * arm.OffsetFactor * forearmLength;
        return elbowPosition + elbowToHandOffset;
    }

    /// <summary>
    /// Draws hands and their afterimages.
    /// </summary>
    /// <param name="handTexture">The hand texture for the arm.</param>
    /// <param name="drawPosition">The position at which the hand should be drawn.</param>
    /// <param name="color">The color that the hand should be drawn with.</param>
    /// <param name="frame">The frame of the hand.</param>
    /// <param name="rotation">The rotation of the hand.</param>
    /// <param name="scale">The scale of the hand.</param>
    /// <param name="direction">The visual flip direction of the hand.</param>
    private void DrawHands(Texture2D handTexture, Vector2 drawPosition, Color color, Rectangle frame, float rotation, float scale, SpriteEffects direction)
    {
        if (VisuallyFlipHand)
            direction ^= SpriteEffects.FlipHorizontally;

        int afterimageCount = 8;
        Vector2 handOrigin = FlipOriginByDirection(frame.Size() * new Vector2(0.2f, 0.8f), frame, direction);
        for (int i = afterimageCount - 1; i >= 0; i--)
        {
            float afterimageOpacity = 1f - i / (afterimageCount - 1f);
            Vector2 afterimageDrawOffset = Velocity * i * -0.24f;
            Main.spriteBatch.Draw(handTexture, drawPosition + afterimageDrawOffset, frame, color * afterimageOpacity, rotation, handOrigin, scale, direction, 0f);
        }

        Main.spriteBatch.Draw(handTexture, drawPosition, frame, color, rotation, handOrigin, scale, direction, 0f);
        Main.spriteBatch.Draw(handTexture, drawPosition, frame, color with { A = 0 } * 0.5f, rotation, handOrigin, scale, direction, 0f);
    }

    /// <summary>
    /// Draws the glock that Nameless may wield.
    /// </summary>
    /// <param name="drawPosition">The position at which the glock should be drawn.</param>
    /// <param name="color">The color that the glock should be drawn with.</param>
    /// <param name="rotation">The rotation of the glock.</param>
    /// <param name="scale">The scale of the glock.</param>
    /// <param name="direction">The visual flip direction of the glock.</param>
    private void DrawGlock(Vector2 drawPosition, Color color, float rotation, float scale, SpriteEffects direction)
    {
        Texture2D glockTexture = GennedAssets.Textures.NamelessDeity.Glock.Value;
        Vector2 glockOrigin = FlipOriginByDirection(new(40f, 223f), glockTexture, direction);
        Vector2 glockDrawOffset = Direction * 28f + Direction.RotatedBy(PiOver2) * -56f;
        Main.spriteBatch.Draw(glockTexture, drawPosition + glockDrawOffset, null, color, rotation, glockOrigin, scale, direction, 0f);
    }

    /// <summary>
    /// Writes this hand's non-visual data to a <see cref="BinaryWriter"/> for the purposes of packet creation.
    /// </summary>
    /// <param name="writer">The packet's binary writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(DirectionOverride);
        writer.Write((byte)VisuallyFlipHand.ToInt());
        writer.Write((byte)HasArms.ToInt());
        writer.Write((byte)CanDoDamage.ToInt());
        writer.Write((byte)HandType);
        writer.Write(Opacity);
        writer.Write(RotationOffset);
        writer.Write(ScaleFactor);
        writer.WriteVector2(FreeCenter);
        writer.WriteVector2(Velocity);
        writer.WriteVector2(ActualCenter);
    }

    /// <summary>
    /// Reads this hand's non-visual data from a <see cref="BinaryReader"/> for the purposes of packet handling.
    /// </summary>
    /// <param name="reader">The packet's binary reader.</param>
    public static NamelessDeityHand ReadFrom(BinaryReader reader)
    {
        int directionOverride = reader.ReadInt32();
        bool visuallyFlipHand = reader.ReadByte() != 0;
        bool hasArms = reader.ReadByte() != 0;
        bool canDoDamage = reader.ReadByte() != 0;
        NamelessDeityHandType handType = (NamelessDeityHandType)reader.ReadByte();
        float opacity = reader.ReadSingle();
        float rotationOffset = reader.ReadSingle();
        float scaleFactor = reader.ReadSingle();
        Vector2 center = reader.ReadVector2();
        Vector2 velocity = reader.ReadVector2();
        Vector2 actualCenter = reader.ReadVector2();

        return new(center, hasArms)
        {
            VisuallyFlipHand = visuallyFlipHand,
            DirectionOverride = directionOverride,
            HasArms = hasArms,
            CanDoDamage = canDoDamage,
            HandType = handType,
            Opacity = opacity,
            RotationOffset = rotationOffset,
            ScaleFactor = scaleFactor,
            FreeCenter = center,
            Velocity = velocity,
            ActualCenter = actualCenter
        };
    }
}
