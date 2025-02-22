using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar;

public class AvatarShadowArm(Vector2 spawnPosition, Vector2 anchorOffset, bool canDoDamage = false)
{
    public enum HandVariant
    {
        StaticHand1,
        StaticHand2,
        StaticHand3,
        StaticHand4,
        GrabbingHand1,
        GrabbingHand2,
        GrabbingHand3,
    }

    /// <summary>
    /// How many frames this arm has existed for.
    /// </summary>
    public int Time;

    /// <summary>
    /// A random ID assigned to this arm as a means of designating identity in visuals.
    /// </summary>
    public int RandomID = Main.rand.Next(10000000);

    /// <summary>
    /// Whether IK calculations for this arm should use a flipped angle.
    /// </summary>
    public bool VerticalFlip;

    /// <summary>
    /// Whether this arm should be able to do damage or not.
    /// </summary>
    public bool CanDoDamage = canDoDamage;

    /// <summary>
    /// The angular offset of the hand's rotation.
    /// </summary>
    public float HandRotationAngularOffset;

    /// <summary>
    /// Whether the hand should have its visual direction flipped or not.
    /// </summary>
    public bool FlipHandDirection;

    /// <summary>
    /// The opacity of this arm.
    /// </summary>
    public float Opacity = 1f;

    /// <summary>
    /// The scale of this arm.
    /// </summary>
    public float Scale = 0.78f;

    /// <summary>
    /// The center position of this arm.
    /// </summary>
    public Vector2 Center = spawnPosition;

    /// <summary>
    /// The offset this arm's starting point should be at relative to its owner.
    /// </summary>
    public Vector2 AnchorOffset = anchorOffset;

    /// <summary>
    /// The effective end position of the arm after drawing.
    /// </summary>
    public Vector2 ForearmEnd
    {
        get;
        private set;
    }

    /// <summary>
    /// Draws this arm relative to its owner.
    /// </summary>
    /// <param name="screenPos">The camera draw offset.</param>
    /// <param name="owner">The owner NPC of this arm.</param>
    /// <param name="variantOverride">An optional parameter that may be used to override the hand variant.</param>
    /// <param name="frameY">An optional parameter that may be used to decide the hand's vertical frame</param>
    public void Draw(Vector2 screenPos, Entity owner, HandVariant? variantOverride = null, int frameY = 0)
    {
        // Collect draw information.
        Vector2 ownerCenter = owner.Center;

        // Calculate the starting position of the arm and ending position of the forearm.
        Vector2 armBackPosition = ownerCenter + AnchorOffset - screenPos;
        Vector2 forearmFrontPosition = Center - screenPos;

        // Calculate arm lengths.
        float armLength = Scale * 450f;
        float forearmLength = Scale * 490f;

        // Use IK to calculate the starting position of the forearm.
        bool flip = VerticalFlip;
        Vector2 forearmBackPosition = CalculateElbowPosition(armBackPosition, forearmFrontPosition, armLength, forearmLength, flip);

        // Draw arm pieces.
        float armRotation = (armBackPosition - forearmBackPosition).ToRotation() + Pi + 0.3f;
        float forearmRotation = (forearmFrontPosition - forearmBackPosition).ToRotation();
        SpriteEffects forearmDirection = SpriteEffects.None;
        if (flip)
            forearmDirection = SpriteEffects.FlipVertically;

        HandVariant handVariant = variantOverride ?? (HandVariant)(RandomID % 4);

        // Store the forearm position.
        Vector2 armDirection = (armRotation - 0.3f).ToRotationVector2();
        Vector2 armEnd = armBackPosition + armDirection * armLength;
        ForearmEnd = armEnd + forearmRotation.ToRotationVector2() * forearmLength + screenPos;

        // This is necessary to ensure that the primitive scissor test culling is applied correctly.
        // If this isn't done, you'll run into dreaded bugs where some resolutions simply have the Avatar's shadow arms getting cut off.
        int oldScreenWidth = Main.screenWidth;
        int oldScreenHeight = Main.screenHeight;
        Main.screenWidth = Main.instance.GraphicsDevice.Viewport.Width;
        Main.screenHeight = Main.instance.GraphicsDevice.Viewport.Height;

        // Render the arm.
        PrimitiveSettings armSettings = new PrimitiveSettings(c =>
        {
            float baseWidth = 20f;
            switch (handVariant)
            {
                case HandVariant.StaticHand1:
                case HandVariant.GrabbingHand1:
                    baseWidth = SmoothStep(39f, 28f, c) + AperiodicSin(c * 11f) * 8f;
                    break;
                case HandVariant.StaticHand2:
                case HandVariant.GrabbingHand2:
                    baseWidth = SmoothStep(64f, 50f, c) + AperiodicSin(c * 12f) * 9f;
                    break;
                case HandVariant.StaticHand3:
                case HandVariant.GrabbingHand3:
                    baseWidth = SmoothStep(67f, 48f, c) + AperiodicSin(c * 4.5f) * 8f;
                    break;
                case HandVariant.StaticHand4:
                    baseWidth = SmoothStep(88f, 66f, c) + AperiodicSin(c * 4.2f) * 6f;
                    break;
            }

            return Scale * baseWidth;
        }, c => Color.Black, _ => Main.screenPosition, ProjectionAreaWidth: Main.instance.GraphicsDevice.Viewport.Width, ProjectionAreaHeight: Main.instance.GraphicsDevice.Viewport.Height, UseUnscaledMatrix: true);
        PrimitiveRenderer.RenderTrail(GeneratePointsBetween(armBackPosition, armEnd), armSettings, 21);

        // Render the forearm.
        PrimitiveSettings forearmSettings = new PrimitiveSettings(c =>
        {
            float baseWidth = 20f;
            switch (handVariant)
            {
                case HandVariant.StaticHand1:
                case HandVariant.GrabbingHand1:
                    baseWidth = AperiodicSin(c * 7f) * 6f + 28f;
                    break;
                case HandVariant.StaticHand2:
                case HandVariant.GrabbingHand2:
                    baseWidth = SmoothStep(50f, 32f, c) + AperiodicSin(c * 10f) * 9f;
                    break;
                case HandVariant.StaticHand3:
                case HandVariant.GrabbingHand3:
                    baseWidth = SmoothStep(48f, 40f, c) + AperiodicSin(c * 7f) * 8f;
                    break;
                case HandVariant.StaticHand4:
                    baseWidth = SmoothStep(66f, 28f, Pow(c, 0.7f)) + AperiodicSin(c * 6f) * 6f;
                    break;
            }

            return Scale * baseWidth;
        }, c => Color.Black, _ => Main.screenPosition, ProjectionAreaWidth: Main.instance.GraphicsDevice.Viewport.Width, ProjectionAreaHeight: Main.instance.GraphicsDevice.Viewport.Height, UseUnscaledMatrix: true);
        PrimitiveRenderer.RenderTrail(GeneratePointsBetween(armEnd - armDirection * armSettings.WidthFunction(1f) * 1.2f, ForearmEnd - screenPos), forearmSettings, 21);

        float handRotation = (ForearmEnd - screenPos - armBackPosition).ToRotation() + HandRotationAngularOffset;
        DrawHand(handVariant, ForearmEnd - screenPos, handRotation, forearmDirection ^ SpriteEffects.FlipVertically, frameY);

        Main.screenWidth = oldScreenWidth;
        Main.screenHeight = oldScreenHeight;
    }

    public void DrawHand(HandVariant handVariant, Vector2 drawPosition, float rotation, SpriteEffects direction, int frameY = 0)
    {
        Vector2 origin = Vector2.Zero;
        float handRotationOffset = 0f;
        float scaleFactor = 0.6f;
        Rectangle? frame = null;
        Texture2D handTexture;

        switch (handVariant)
        {
            case HandVariant.StaticHand1:
                handTexture = GennedAssets.Textures.SecondPhaseForm.ShadowHand1.Value;
                origin = new Vector2(79f, 239f);
                break;
            case HandVariant.StaticHand2:
                handTexture = GennedAssets.Textures.SecondPhaseForm.ShadowHand2.Value;
                origin = new Vector2(61f, 110f);
                break;
            case HandVariant.StaticHand3:
                handTexture = GennedAssets.Textures.SecondPhaseForm.ShadowHand3.Value;
                origin = new Vector2(103f, 438f);
                break;
            case HandVariant.StaticHand4:
                handTexture = GennedAssets.Textures.SecondPhaseForm.ShadowHand4.Value;
                origin = new Vector2(94f, 99f);
                break;
            case HandVariant.GrabbingHand1:
                handTexture = GennedAssets.Textures.SecondPhaseForm.GrabbingShadowHand1.Value;
                frame = handTexture.Frame(1, 5, 0, frameY);
                origin = frame.Value.Size() * 0.5f;
                handRotationOffset = -PiOver2;
                scaleFactor = 1.35f;
                break;
            case HandVariant.GrabbingHand2:
                handTexture = GennedAssets.Textures.SecondPhaseForm.GrabbingShadowHand2.Value;
                frame = handTexture.Frame(1, 5, 0, frameY);
                origin = frame.Value.Size() * 0.5f;
                handRotationOffset = -PiOver2;
                scaleFactor = 1.35f;
                break;
            case HandVariant.GrabbingHand3:
                handTexture = GennedAssets.Textures.SecondPhaseForm.GrabbingShadowHand3.Value;
                frame = handTexture.Frame(1, 5, 0, frameY);
                origin = frame.Value.Size() * 0.5f;
                handRotationOffset = -PiOver2;
                scaleFactor = 1.35f;
                break;
            default:
                handTexture = InvisiblePixel;
                break;
        }

        if (FlipHandDirection)
            direction ^= SpriteEffects.FlipHorizontally;

        if (direction.HasFlag(SpriteEffects.FlipHorizontally))
            origin.X = (frame?.Width ?? handTexture.Width) - origin.X;
        if (direction.HasFlag(SpriteEffects.FlipVertically))
            origin.Y = (frame?.Height ?? handTexture.Height) - origin.Y;

        Main.spriteBatch.Draw(handTexture, drawPosition, frame, Color.White, rotation + handRotationOffset, origin, Scale * scaleFactor, direction, 0f);
    }

    public static List<Vector2> GeneratePointsBetween(Vector2 start, Vector2 end)
    {
        List<Vector2> points = new List<Vector2>(10);
        for (int i = 0; i < 10; i++)
            points.Add(Vector2.Lerp(start, end, i / 9f));

        return points;
    }

    /// <summary>
    /// Writes information about this arm to a <see cref="BinaryWriter"/> for multiplayer syncing purposes.
    /// </summary>
    /// <param name="writer">The writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(Time);
        writer.Write(Scale);
        writer.WriteVector2(Center);
        writer.WriteVector2(AnchorOffset);
    }

    /// <summary>
    /// Reads information about an arm from a <see cref="BinaryReader"/> for multiplayer syncing purposes.
    /// </summary>
    /// <param name="reader">The reader.</param>
    public static AvatarShadowArm ReadFrom(BinaryReader reader)
    {
        int time = reader.ReadInt32();
        float scale = reader.ReadSingle();
        Vector2 center = reader.ReadVector2();
        Vector2 anchorOffset = reader.ReadVector2();
        return new AvatarShadowArm(center, anchorOffset)
        {
            Time = time,
            Scale = scale
        };
    }
}
