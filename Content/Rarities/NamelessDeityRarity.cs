using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace NoxusBoss.Content.Rarities;

public class NamelessDeityRarity : ModRarity
{
    public override Color RarityColor => Color.White;

    public override void Load() => GlobalItemEventHandlers.PreDrawTooltipLineEvent += RenderRarityWithShader;

    private bool RenderRarityWithShader(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (item.rare == Type && line.Name == "ItemName" && line.Mod == "Terraria")
        {
            Main.spriteBatch.PrepareForShaders(null, true);

            ManagedShader barShader = ShaderManager.GetShader("NoxusBoss.NamelessBossBarShader");
            barShader.TrySetParameter("textureSize", Vector2.One * 560f);
            barShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.4f);
            barShader.TrySetParameter("chromaticAberrationOffset", Cos01(Main.GlobalTimeWrappedHourly * 0.8f) * 4f);
            barShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
            barShader.Apply();

            Vector2 drawPosition = new Vector2(line.X, line.Y);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, line.Text, drawPosition, Color.White, line.Rotation, line.Origin, line.BaseScale, line.MaxWidth, line.Spread);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            return false;
        }

        return true;
    }
}
