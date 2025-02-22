using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace NoxusBoss.Content.Rarities;

public class AvatarRarity : ModRarity
{
    public static Color ColorA => new Color(255, 0, 0);

    public static Color ColorB => new Color(105, 255, 255);

    public static float ColorInterpolant
    {
        get
        {
            float baseInterpolant = Cos01(Main.GlobalTimeWrappedHourly * 2.1f);
            float colorInterpolant = EasingCurves.Cubic.Evaluate(EasingType.InOut, baseInterpolant);
            return colorInterpolant;
        }
    }

    public static Color InvertedRarityColor => Color.Lerp(ColorA, ColorB, 1f - ColorInterpolant);

    public override Color RarityColor => Color.Lerp(ColorA, ColorB, ColorInterpolant);

    public override void Load() => GlobalItemEventHandlers.PreDrawTooltipLineEvent += RenderSplitRarity;

    private bool RenderSplitRarity(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (item.rare == Type && line.Name == "ItemName" && line.Mod == "Terraria")
        {
            int splitLength = line.Text.Length / 2;

            // Prioritize trying to split along natural spaces.
            int? spaceIndex = null;
            float minDistance = 9999f;
            for (int i = 0; i < line.Text.Length; i++)
            {
                float distanceFromSplit = Distance(i, splitLength);
                if (distanceFromSplit < minDistance && line.Text[i] == ' ')
                {
                    spaceIndex = i;
                    minDistance = distanceFromSplit;
                }
            }
            if (spaceIndex is not null)
                splitLength = spaceIndex.Value;

            string partA = new string(line.Text.AsSpan(0, splitLength));
            string partB = new string(line.Text.AsSpan(splitLength, line.Text.Length - splitLength));

            Vector2 drawPosition = new Vector2(line.X, line.Y);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, partA, drawPosition, RarityColor, line.Rotation, line.Origin, line.BaseScale, line.MaxWidth, line.Spread);

            drawPosition.X += line.Font.MeasureString(partA).X * line.BaseScale.X;
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, partB, drawPosition, InvertedRarityColor, line.Rotation, line.Origin, line.BaseScale, line.MaxWidth, line.Spread);
            return false;
        }

        return true;
    }
}
