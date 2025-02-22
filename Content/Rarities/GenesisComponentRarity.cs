using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace NoxusBoss.Content.Rarities;

public class GenesisComponentRarity : ModRarity
{
    public override Color RarityColor => Color.HotPink;

    /// <summary>
    /// The color palette for this rarity.
    /// </summary>
    public static readonly Color[] RarityPalette = new Color[]
    {
        new(127, 81, 255),
        new(255, 236, 71),
        new(240, 109, 228),
    };

    public override void Load() => GlobalItemEventHandlers.PreDrawTooltipLineEvent += RenderRarityWithShader;

    private bool RenderRarityWithShader(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (item.rare == Type && line.Name == "ItemName" && line.Mod == "Terraria")
        {
            Main.spriteBatch.PrepareForShaders(null, true);

            Vector2 drawPosition = new Vector2(line.X, line.Y);

            float pulse = Main.GlobalTimeWrappedHourly * 1.4f % 1f;
            Vector2 textSize = line.Font.MeasureString(line.Text);
            Vector2 backglowOrigin = textSize * 0.5f;
            Vector2 backglowScale = line.BaseScale * (Vector2.One + new Vector2(0.1f, 0.5f) * pulse);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, line.Text, drawPosition + backglowOrigin, line.Color * Pow(1f - pulse, 1.5f), line.Rotation, backglowOrigin, backglowScale, line.MaxWidth, line.Spread);

            ManagedShader rarityShader = ShaderManager.GetShader("NoxusBoss.GenesisComponentRarityShader");
            rarityShader.TrySetParameter("gradient", RarityPalette.Select(r => r.ToVector3()).ToArray());
            rarityShader.TrySetParameter("gradientCount", RarityPalette.Length);
            rarityShader.TrySetParameter("hueExponent", 3.1f);
            rarityShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
            rarityShader.Apply();

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, line.Text, drawPosition, Color.White, line.Rotation, line.Origin, line.BaseScale, line.MaxWidth, line.Spread);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            return false;
        }

        return true;
    }
}
