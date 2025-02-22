using CalamityMod.UI.DraedonSummoning;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Content.NPCs.Bosses.Draedon.Draedon;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.DraedonDialogue;

[JITWhenModsEnabled(CalamityCompatibility.ModName)]
[ExtendsFromMod(CalamityCompatibility.ModName)]
public class DraedonWorldDialogue
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
    public Vector2 FlyOffset => Vector2.Zero;

    public Vector2 Scale
    {
        get
        {
            float generalScale = 1.1f;
            return Vector2.One * generalScale;
        }
    }

    public Vector2 ScreenFluff => Vector2.Transform(new Vector2(272f, 96f) * Scale, Matrix.Invert(Main.GameViewMatrix.TransformationMatrix));

    public DraedonWorldDialogue(string textLocalizationKey, int direction, Vector2 position, int lifetime)
    {
        this.textLocalizationKey = textLocalizationKey;
        Direction = direction;
        Position = position;
        Lifetime = lifetime;
    }

    /// <summary>
    /// Updates this dialogue instance.
    /// </summary>
    public void Update()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        Position.Y -= InverseLerp(0.1f, 0f, LifetimeRatio).Squared() * 7f;

        Time++;
    }

    /// <summary>
    /// Renders this dialogue instance.
    /// </summary>
    public void Render()
    {
        DynamicSpriteFont font = FontRegistry.Instance.DraedonText;

        float opacity = InverseLerpBump(0f, 0.15f, 0.85f, 1f, LifetimeRatio);

        Vector2 scale = Scale;

        int index = 0;
        float rotation = Rotation * Direction;
        string[] lines = Utils.WordwrapString(Text, font, (int)(Scale.X * 450f), 20, out _);
        Vector2 fontOrientation = (rotation + PiOver2).ToRotationVector2();
        Vector2 finalDrawPosition = Position - Main.screenPosition;
        Vector2 finalOrigin = Vector2.Zero;

        int totalLines = 0;
        int totalCharacters = 0;
        float maxHorizontalTextSize = 0f;
        float spacingPerLine = scale.Y * 32f;
        float pixelation = (1f - InverseLerpBump(0.01f, 0.1f, 0.9f, 0.99f, LifetimeRatio)) * 8f + 0.001f;
        foreach (string? line in lines)
        {
            if (string.IsNullOrEmpty(line))
                break;

            maxHorizontalTextSize = MathF.Max(maxHorizontalTextSize, font.MeasureString(line).X);
            totalLines++;
            totalCharacters += line.Length;
        }

        int totalCharactersToDisplay = (int)(Time * 0.75f);
        if (totalCharactersToDisplay < totalCharacters && Time % 4 == 0 && Main.instance.IsActive && !Main.gamePaused)
            SoundEngine.PlaySound(Main.rand.Next(CodebreakerUI.DraedonTalks), Position);

        foreach (string? line in lines)
        {
            if (string.IsNullOrEmpty(line))
                break;

            string prunedLine = string.Concat(line.Take(totalCharactersToDisplay));
            Vector2 textSize = font.MeasureString(prunedLine);
            Vector2 origin = textSize * new Vector2(Direction == 1 ? 0f : 1f, 0.5f);
            Vector2 drawPosition = Position - Main.screenPosition;
            drawPosition.X -= Direction * maxHorizontalTextSize * scale.X * 0.5f;
            drawPosition.Y += (index - totalLines) * spacingPerLine;
            drawPosition += FlyOffset;

            Color color = new Color(155, 255, 255) * opacity;

            float outlineOpacity = InverseLerp(5f, 0f, pixelation);
            for (int i = 0; i < 4; i++)
            {
                Vector2 drawOffset = (TwoPi * i / 4f).ToRotationVector2() * 2.5f;
                Main.spriteBatch.DrawString(font, prunedLine, drawPosition + drawOffset, new Color(9, 51, 112) * opacity * outlineOpacity, rotation, origin, scale, 0, 0f);
            }

            ManagedShader dialogueShader = ShaderManager.GetShader("NoxusBoss.DraedonWorldDialogueShader");
            dialogueShader.TrySetParameter("secondaryColor", new Color(255, 255, 255).ToVector4());
            dialogueShader.TrySetParameter("fontCenter", Vector2.Transform(drawPosition - fontOrientation * scale.Y * 4f, Main.GameViewMatrix.TransformationMatrix));
            dialogueShader.TrySetParameter("fontOrientation", fontOrientation);
            dialogueShader.TrySetParameter("lineDistance", Main.GameViewMatrix.Zoom.X * 4f);
            dialogueShader.TrySetParameter("pixelation", pixelation);
            dialogueShader.TrySetParameter("textSize", textSize * Scale);
            dialogueShader.Apply();

            Main.spriteBatch.DrawString(font, prunedLine, drawPosition, color, rotation, origin, scale, 0, 0f);

            index++;
            finalDrawPosition = Vector2.Max(drawPosition, finalDrawPosition);
            finalOrigin = font.MeasureString(line) * 0.5f;
            totalCharactersToDisplay -= prunedLine.Length;
        }

        RenderOutline(new Vector2(Position.X - Main.screenPosition.X - Direction * 10f, finalDrawPosition.Y + 40f), fontOrientation, scale, finalOrigin);
    }

    private void RenderOutline(Vector2 drawPosition, Vector2 fontOrientation, Vector2 scale, Vector2 origin)
    {
        int draedonIndex = NPC.FindFirstNPC(ModContent.NPCType<QuestDraedon>());
        Vector2 draedonPosition = draedonIndex >= 0 ? Main.npc[draedonIndex].Top : Vector2.Zero;

        Vector2 up = fontOrientation;
        Vector2 forward = up.RotatedBy(PiOver2);
        Vector2[] positions = new Vector2[27];
        positions[0] = drawPosition + Main.screenPosition - up * scale * 40f - forward * scale * Direction * origin.X * 1.05f;

        // Create an L shaped outline that.
        for (int i = 0; i < positions.Length; i++)
        {
            Vector2 direction = Direction * forward;

            if (i >= 1)
                positions[i] += positions[i - 1];

            Vector2 backwards = positions[i].SafeDirectionTo(Main.MouseWorld);
            if (i >= positions.Length / 2)
                backwards = backwards.RotatedBy(-PiOver2);

            Vector2 offset = direction * 5f;
            positions[i] += offset * scale;
        }

        // Apply spiky effects to the outline.
        Vector2 cornerPosition = positions[positions.Length / 2];
        for (int i = 0; i < positions.Length; i++)
        {
            float completionRatio = i / (float)(positions.Length - 1f);
            Vector2 mainSpikiness = cornerPosition.SafeDirectionTo(draedonPosition) * InverseLerpBump(0.42f, 0.5f, 0.5f, 0.58f, completionRatio) * 60f;
            Vector2 spikiness = mainSpikiness;
            positions[i] += spikiness;
        }

        ManagedShader spikeShader = ShaderManager.GetShader("NoxusBoss.SolynFightDialogueSpikyBorderShader");
        PrimitiveSettings primitiveSettings = new PrimitiveSettings(SpikeWidthFunction, SpikeColorFunction, null, false, false, spikeShader);

        PrimitiveRenderer.RenderTrail(positions, primitiveSettings, 96);
    }

    public float SpikeWidthFunction(float completionRatio)
    {
        return InverseLerp(0.15f, 0f, FlyOffInterpolant) * InverseLerp(0.08f, 0.16f, LifetimeRatio) * Convert01To010(completionRatio) * 1.5f;
    }

    public static Color SpikeColorFunction(float completionRatio) => Color.White;
}
