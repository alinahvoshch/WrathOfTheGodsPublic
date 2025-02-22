using Microsoft.Xna.Framework;
using NoxusBoss.Core.Configuration;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.UI;

[Autoload(Side = ModSide.Client)]
public class AnnoyingConfigMessageSystem : ModSystem
{
    /// <summary>
    /// How long it's been since the config warning text appeared, in frames.
    /// </summary>
    public static int AnimationTimer
    {
        get;
        private set;
    }

    /// <summary>
    /// The color of the config warning text.
    /// </summary>
    public static readonly Color TextColor = new Color(252, 163, 176);

    public override void OnModLoad() => Main.QueueMainThreadAction(() => Main.OnPostDraw += DrawAnimationWrapper);

    public override void OnModUnload() => Main.OnPostDraw -= DrawAnimationWrapper;

    private static void DrawAnimationWrapper(GameTime obj)
    {
        if (Main.gameMenu || !WoTGConfig.Instance.DisplayConfigMessage || Main.playerInventory || Main.gamePaused || Main.netMode != NetmodeID.SinglePlayer)
            return;

        Main.spriteBatch.Begin();
        DrawText();
        Main.spriteBatch.End();
        AnimationTimer++;
    }

    public override void OnWorldUnload() => AnimationTimer = 0;

    private static void DrawText()
    {
        // Calculate dialog draw information.
        float dialogScale = 0.7f;
        float dialogOpacity = InverseLerp(0f, 60f, AnimationTimer);
        string dialog = Language.GetTextValue($"Mods.NoxusBoss.Configs.WoTGConfig.DisplayConfigMessage.WarningText");
        DynamicSpriteFont font = FontAssets.DeathText.Value;

        // Draw the dialog.
        int i = 0;
        foreach (string line in Utils.WordwrapString(dialog, font, (int)(dialogScale * 2000f), 20, out _))
        {
            if (string.IsNullOrEmpty(line))
                break;

            Vector2 dialogSize = font.MeasureString(line);
            Vector2 dialogDrawPosition = Main.ScreenSize.ToVector2() * 0.5f - Vector2.UnitY * 150f;
            dialogDrawPosition.Y += i * dialogScale * 54f;
            Vector2 dialogOrigin = dialogSize * new Vector2(0.5f, 0f);

            for (int j = 0; j < 4; j++)
                Main.spriteBatch.DrawString(font, line, dialogDrawPosition + (TwoPi * j / 4f).ToRotationVector2() * 2f, Color.Black * Pow(dialogOpacity, 2.4f), 0f, dialogOrigin, dialogScale, 0, 0f);
            Main.spriteBatch.DrawString(font, line, dialogDrawPosition, TextColor * dialogOpacity, 0f, dialogOrigin, dialogScale, 0, 0f);
            i++;
        }
    }
}
