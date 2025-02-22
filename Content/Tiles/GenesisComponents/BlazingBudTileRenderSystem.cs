using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Items.GenesisComponents;
using NoxusBoss.Content.Particles;
using NoxusBoss.Content.Particles.Metaballs;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Tiles.GenesisComponents;

public class BlazingBudTileRenderSystem : GenesisPlantTileRenderingSystem
{
    public override int ItemID => ModContent.ItemType<BlazingBud>();

    public override int TileID => ModContent.TileType<BlazingBudTile>();

    public override void UpdatePoint(Point p)
    {
        Lighting.AddLight(new Point(p.X, p.Y - 11).ToWorldCoordinates(), new Color(255, 217, 185).ToVector3());
    }

    public override void InstaceRenderFunction(bool disappearing, float growthInterpolant, float growthInterpolantModified, int i, int j, SpriteBatch spriteBatch)
    {
        if (!Main.gamePaused)
        {
            Vector2 heatDistortionPosition = new Vector2(i, j).ToWorldCoordinates() + new Vector2(-4f, -184f);
            if (Main.LocalPlayer.WithinRange(heatDistortionPosition, 2400f))
            {
                ModContent.GetInstance<HeatDistortionMetaball>().CreateParticle(heatDistortionPosition, Main.rand.NextVector2Circular(3f, 0.2f) - Vector2.UnitY * 8f, 100f);

                if (Main.rand.NextBool(7))
                {
                    Color color = Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat(0.4f, 0.8f));
                    BloomPixelParticle particle = new BloomPixelParticle(heatDistortionPosition + Main.rand.NextVector2Circular(40f, 10f) - Vector2.UnitY * 40f, -Vector2.UnitY.RotatedByRandom(0.35f) * Main.rand.NextFloat(7f), Color.White, color, 60, Vector2.One * 0.08f, Vector2.One * 4f);
                    particle.Spawn();
                }
            }
        }

        float fadeToWhiteInterpolant = InverseLerp(0.6f, 0.3f, growthInterpolant);
        float seedOpacity = InverseLerp(0.28f, 0.1f, growthInterpolant);
        float seedWhiteBias = InverseLerp(0f, 0.3f, growthInterpolant);
        if (!disappearing)
        {
            fadeToWhiteInterpolant = 0f;
            seedOpacity = 0f;
            seedWhiteBias = 0f;
        }

        ManagedShader whiteBiasShader = ShaderManager.GetShader("NoxusBoss.WhiteColorBiasShader");
        whiteBiasShader.TrySetParameter("whiteInterpolant", fadeToWhiteInterpolant);
        whiteBiasShader.Apply();

        // Draw the flower.
        Vector2 drawPosition = new Vector2((i + 0.5f) * 16f, j * 16f + 24f) - Main.screenPosition;
        Texture2D texture = GennedAssets.Textures.GenesisComponents.BlazingBudTileReal.Value;
        Vector2 scale = new Vector2(Pow(growthInterpolantModified, 1.7f), growthInterpolantModified);
        Main.spriteBatch.Draw(texture, drawPosition, null, Color.White, 0f, texture.Size() * new Vector2(0.5f, 1f), scale, 0, 0f);

        whiteBiasShader.TrySetParameter("whiteInterpolant", seedWhiteBias);
        whiteBiasShader.Apply();

        // Draw the seed when about to fade out.
        float seedScale = EasingCurves.Cubic.Evaluate(EasingType.InOut, seedOpacity);
        Texture2D seedTexture = TextureAssets.Item[ItemID].Value;
        Main.spriteBatch.Draw(seedTexture, drawPosition + new Vector2(-8f, -30f), null, Color.White * seedOpacity, 0f, seedTexture.Size() * 0.5f, seedScale, 0, 0f);
    }
}
