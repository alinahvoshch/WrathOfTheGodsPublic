using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Items.GenesisComponents;
using NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Content.Tiles.GenesisComponents.SyntheticSeedlingTile;

namespace NoxusBoss.Content.Tiles.GenesisComponents;

public class SyntheticSeedlingTileRenderSystem : GenesisPlantTileRenderingSystem
{
    public override int ItemID => ModContent.ItemType<SyntheticSeedling>();

    public override int TileID => ModContent.TileType<SyntheticSeedlingTile>();

    public override void UpdatePoint(Point p)
    {
        Lighting.AddLight(new Point(p.X, p.Y - 9).ToWorldCoordinates(), Color.White.ToVector3());

        if (Main.rand.NextBool(8))
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                float arcReachInterpolant = Main.rand.NextFloat();
                int arcLifetime = Main.rand.Next(5, 12);
                Vector2 arcSpawnPosition = p.ToWorldCoordinates() + new Vector2(16f, -144f) + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 arcOffset = new Vector2(0.6f, -0.6f).SafeNormalize(Vector2.Zero).RotatedByRandom(1.3f) * Lerp(80f, 160f, Pow(arcReachInterpolant, 8f));
                NewProjectileBetter(new EntitySource_TileUpdate(p.X, p.Y), arcSpawnPosition, arcOffset, ModContent.ProjectileType<SmallTeslaArc>(), 0, 0f, -1, arcLifetime, 2f, 1.45f);
            }
        }
    }

    public override void InstaceRenderFunction(bool disappearing, float growthInterpolant, float growthInterpolantModified, int i, int j, SpriteBatch spriteBatch)
    {
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
        Vector2 drawPosition = new Vector2((i + 0.5f) * 16f, j * 16f + 18f) - Main.screenPosition;
        Texture2D texture = ActualTexture.Value;
        Vector2 scale = new Vector2(Pow(growthInterpolantModified, 1.7f), growthInterpolantModified);
        Main.spriteBatch.Draw(texture, drawPosition, null, Color.White, 0f, texture.Size() * new Vector2(0.5f, 1f), scale, 0, 0f);

        // Draw a glow over the flower.
        Texture2D glow = GennedAssets.Textures.GenesisComponents.SyntheticSeedlingBloom.Value;
        Vector2 glowOrigin = new Vector2(37f, 134f);
        float glowScaleFactor = Lerp(1f, 1.05f, Cos01(Main.GlobalTimeWrappedHourly * 2f + i + j)) * 1.3f;
        float glowOpacity = InverseLerp(0f, 0.5f, growthInterpolant);
        Vector2 glowDrawPosition = drawPosition + new Vector2(2f, -136f) * scale;
        Main.spriteBatch.Draw(glow, glowDrawPosition, null, new Color(53, 245, 255, 0) * glowOpacity * 0.75f, 0f, glowOrigin, glowScaleFactor, 0, 0f);

        whiteBiasShader.TrySetParameter("whiteInterpolant", seedWhiteBias);
        whiteBiasShader.Apply();

        // Draw the seed when about to fade out.
        float seedScale = EasingCurves.Cubic.Evaluate(EasingType.InOut, seedOpacity);
        Texture2D seedTexture = TextureAssets.Item[ItemID].Value;
        Main.spriteBatch.Draw(seedTexture, drawPosition + new Vector2(-8f, -30f), null, Color.White * seedOpacity, 0f, seedTexture.Size() * 0.5f, seedScale, 0, 0f);
    }
}
