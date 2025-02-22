using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Content.NPCs.Friendly;
using ReLogic.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers.SolynDialogue;

public class SolynWorldDialogue
{
    /// <summary>
    /// The localization key for this dialogue text.
    /// </summary>
    private readonly string textLocalizationKey;

    /// <summary>
    /// The horizontal direction offset of this text.
    /// </summary>
    public int Direction;

    /// <summary>
    /// The text that this dialogue should display.
    /// </summary>
    public string Text => Language.GetTextValue(textLocalizationKey);

    /// <summary>
    /// The position of this dialogue instance in the world.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// How long this dialogue has existed for in the world so far.
    /// </summary>
    public int Time;

    /// <summary>
    /// How long this dialogue should exist for in the world.
    /// </summary>
    public int Lifetime;

    /// <summary>
    /// Whether this dialogue is being yelled or not.
    /// </summary>
    public bool Yell;

    /// <summary>
    /// The rotation of this dialogue.
    /// </summary>
    public float Rotation;

    /// <summary>
    /// The time to lifetime ratio of this dialogue.
    /// </summary>
    public float LifetimeRatio => Saturate(Time / (float)Lifetime);

    /// <summary>
    /// How much this dialogue is flying off, as a 0-1 interpolant.
    /// </summary>
    public float FlyOffInterpolant => Pow(InverseLerp(0.9f, 1f, LifetimeRatio), 2.9f);

    /// <summary>
    /// How much the dialogue needs to fly off by.
    /// </summary>
    public Vector2 FlyOffset => Vector2.UnitX * FlyOffInterpolant * Direction * -2200f;

    public Vector2 Scale
    {
        get
        {
            float scaleX = EasingCurves.Elastic.Evaluate(EasingType.Out, InverseLerp(0f, 0.3f, LifetimeRatio));
            float scaleY = EasingCurves.Elastic.Evaluate(EasingType.Out, InverseLerp(0.05f, 0.42f, LifetimeRatio));
            float generalScale = Yell ? 0.88f : 0.74f;
            Vector2 textSize = FontRegistry.Instance.SolynFightDialogue.MeasureString(Text);
            if (textSize.X > 1200f)
                generalScale *= 1200f / textSize.X;

            Vector2 scale = new Vector2(scaleX, scaleY) * (1f + InverseLerp(0.925f, 1f, LifetimeRatio) * 0.5f) * generalScale;
            scale.X *= 1f + Sqrt(FlyOffInterpolant) * 0.6f;
            scale.Y *= 1f - Cbrt(FlyOffInterpolant) * 0.74f;
            return scale;
        }
    }

    public Vector2 ScreenFluff => Vector2.Transform(new Vector2(272f, 132f) * Scale, Matrix.Invert(Main.GameViewMatrix.TransformationMatrix));

    public SolynWorldDialogue(string textLocalizationKey, int direction, Vector2 position, int lifetime, bool yell)
    {
        this.textLocalizationKey = textLocalizationKey;
        Direction = direction;
        Position = position;
        Lifetime = lifetime;
        Yell = yell;
    }

    /// <summary>
    /// Updates this dialogue instance.
    /// </summary>
    public void Update()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        Position += Main.rand.NextVector2Circular(5f, 3f) * InverseLerp(0.35f, 0f, LifetimeRatio);
        Position.X -= Direction * InverseLerp(0.15f, 0f, LifetimeRatio).Squared() * 16f;
        Position.Y -= InverseLerp(0.1f, 0f, LifetimeRatio).Squared() * 7f;

        Time++;
    }

    /// <summary>
    /// Renders this dialogue instance.
    /// </summary>
    public void Render()
    {
        DynamicSpriteFont font = FontRegistry.Instance.SolynFightDialogue;

        float fadeIn = InverseLerp(0f, 0.16f, LifetimeRatio);
        float opacity = fadeIn;

        Vector2 scale = Scale;
        Vector2 textSize = FontRegistry.Instance.SolynFightDialogue.MeasureString(Text);
        Vector2 origin = textSize * 0.5f;
        Vector2 drawPosition = Position - Main.screenPosition;
        Vector2 left = drawPosition - Vector2.UnitX * textSize * Scale * 0.5f;
        Vector2 right = drawPosition + Vector2.UnitX * textSize * Scale * 0.5f;
        Vector2 top = drawPosition - Vector2.UnitY * textSize * Scale * 0.5f;
        Vector2 bottom = drawPosition + Vector2.UnitY * textSize * Scale * 0.5f;

        float rightFluff = (Main.mapEnabled && Main.mapStyle == 1 ? 200f : 0f) + ScreenFluff.X;
        float rotation = Rotation * Direction;

        Vector2 screenSize = Main.ScreenSize.ToVector2();

        if (left.X < ScreenFluff.X)
            drawPosition.X += ScreenFluff.X - left.X;
        if (right.X > screenSize.X - rightFluff)
            drawPosition.X += screenSize.X - rightFluff - right.X;
        if (top.Y < ScreenFluff.Y)
            drawPosition.Y += ScreenFluff.Y - top.Y;
        if (bottom.Y > screenSize.Y - ScreenFluff.Y)
            drawPosition.Y += screenSize.Y - ScreenFluff.Y - bottom.Y;

        drawPosition += FlyOffset;

        Color color = new Color(255, 199, 8) * opacity;
        Vector2 fontOrientation = (rotation + PiOver2).ToRotationVector2();

        for (int i = 0; i < 4; i++)
        {
            Vector2 drawOffset = (TwoPi * i / 4f).ToRotationVector2() * 2f;
            Main.spriteBatch.DrawString(font, Text, drawPosition + drawOffset, new Color(119, 0, 43) * opacity, rotation, origin, scale, 0, 0f);
        }

        ManagedShader dialogueShader = ShaderManager.GetShader("NoxusBoss.SolynWorldDialogueShader");
        dialogueShader.TrySetParameter("secondaryColor", new Color(255, 90, 148).ToVector4());
        dialogueShader.TrySetParameter("fontCenter", Vector2.Transform(drawPosition - fontOrientation * scale.Y * 4f, Main.GameViewMatrix.TransformationMatrix));
        dialogueShader.TrySetParameter("fontOrientation", fontOrientation);
        dialogueShader.TrySetParameter("lineDistance", Main.GameViewMatrix.Zoom.X * 4f);
        dialogueShader.Apply();

        Main.spriteBatch.DrawString(font, Text, drawPosition, color, rotation, origin, scale, 0, 0f);
        RenderSpikyOutline(drawPosition, fontOrientation, scale, origin);
    }

    private void RenderSpikyOutline(Vector2 drawPosition, Vector2 fontOrientation, Vector2 scale, Vector2 origin)
    {
        int solynIndex = NPC.FindFirstNPC(ModContent.NPCType<Solyn>());
        if (solynIndex == -1)
            solynIndex = NPC.FindFirstNPC(ModContent.NPCType<BattleSolyn>());

        Vector2 solynPosition = solynIndex >= 0 ? Main.npc[solynIndex].Center : Vector2.Zero;

        Vector2 up = fontOrientation;
        Vector2 forward = up.RotatedBy(PiOver2);
        Vector2[] positions = new Vector2[27];

        if (!Yell)
        {
            drawPosition.X -= Direction * scale.X * 50f;
            drawPosition.Y += positions.Length * scale.Y * 1.5f;
            up = -forward;
        }

        positions[0] = drawPosition + Main.screenPosition - up * scale * 40f - forward * scale * Direction * origin.X * 1.05f;

        // Create an L shaped outline that.
        for (int i = 0; i < positions.Length; i++)
        {
            Vector2 direction = i >= positions.Length / 2 ? Direction * forward : up;

            if (i >= 1)
                positions[i] += positions[i - 1];

            Vector2 backwards = positions[i].SafeDirectionTo(Main.MouseWorld);
            if (i >= positions.Length / 2)
                backwards = backwards.RotatedBy(-PiOver2);

            Vector2 offset = direction * 6f;
            positions[i] += offset * scale;
        }

        // Apply spiky effects to the outline.
        Vector2 cornerPosition = positions[positions.Length / 2];
        for (int i = 0; i < positions.Length; i++)
        {
            float completionRatio = i / (float)(positions.Length - 1f);
            Vector2 mainSpikiness = cornerPosition.SafeDirectionTo(solynPosition) * InverseLerpBump(0.44f, 0.5f, 0.5f, 0.56f, completionRatio) * 38f;

            Vector2 direction = i < positions.Length / 2 ? -Direction * forward : up;
            Vector2 sideSpikiness = direction * Pow(TriangleWave(completionRatio * 7f + Main.GlobalTimeWrappedHourly * 1.9f), 1f) * Sin01(completionRatio * Pi * 3f) * 20f;
            Vector2 spikiness = mainSpikiness + sideSpikiness * Yell.ToInt();
            positions[i] += spikiness;
        }

        ManagedShader spikeShader = ShaderManager.GetShader("NoxusBoss.SolynFightDialogueSpikyBorderShader");
        PrimitiveSettings primitiveSettings = new PrimitiveSettings(SpikeWidthFunction, SpikeColorFunction, null, false, false, spikeShader);

        PrimitiveRenderer.RenderTrail(positions, primitiveSettings, 96);
    }

    public static float TriangleWave(float x) => Abs(x - Floor(x + 0.5f)) * 2f;

    public float SpikeWidthFunction(float completionRatio)
    {
        return InverseLerp(0.15f, 0f, FlyOffInterpolant) * InverseLerp(0.08f, 0.16f, LifetimeRatio) * Convert01To010(completionRatio) * 1.5f;
    }

    public static Color SpikeColorFunction(float completionRatio) => new(255, 199, 8);
}
