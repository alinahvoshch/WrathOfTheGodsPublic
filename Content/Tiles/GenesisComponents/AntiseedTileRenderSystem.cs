using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Items.GenesisComponents;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Content.Tiles.GenesisComponents.TheAntiseedTile;

namespace NoxusBoss.Content.Tiles.GenesisComponents;

public class AntiseedTileRenderSystem : GenesisPlantTileRenderingSystem
{
    /// <summary>
    /// The render target responsible for holding seeds data.
    /// </summary>
    public static InstancedRequestableTarget SeedTarget
    {
        get;
        private set;
    }

    public override bool AffectedByLight => false;

    public override int ItemID => ModContent.ItemType<TheAntiseed>();

    public override int TileID => ModContent.TileType<TheAntiseedTile>();

    public override void OnModLoad()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            SeedTarget = new InstancedRequestableTarget();
            Main.ContentThatNeedsRenderTargets.Add(SeedTarget);
        }

        base.OnModLoad();
    }

    public override void InstaceRenderFunction(bool disappearing, float growthInterpolant, float growthInterpolantModified, int i, int j, SpriteBatch spriteBatch)
    {
        ulong seed = (ulong)(i * 13 + j * 71);
        int direction = (Utils.RandomInt(ref seed, 2) % 2 == 0).ToDirectionInt();
        float rotation = Lerp(0.04f, 0.17f, Utils.RandomFloat(ref seed)) * direction;
        rotation += Main.instance.TilesRenderer.GetWindGridPush(i, j, 30, 0.003f) + Main.instance.TilesRenderer.GetWindCycle(i, j, Main.GlobalTimeWrappedHourly * 0.5f) * 0.02f;

        // Draw a subtractive backglow.
        Vector2 drawPosition = new Vector2(i * 16f, j * 16f + 24f) - Main.screenPosition;
        Vector2 backglowDrawPosition = drawPosition - Vector2.UnitY.RotatedBy(rotation) * (StemHeight - 32f) * growthInterpolantModified;
        DrawShadowBackglow(growthInterpolantModified, backglowDrawPosition, spriteBatch);

        // Prepare the seed target.
        int identifier = i * 720000 + j;
        SeedTarget.Request(400, 400, identifier, () => DrawInstanceToTarget(growthInterpolantModified, seed, i, j, spriteBatch));

        float glowInterpolant = InverseLerpBump(0f, 0.2f, 0.2f, 1f, growthInterpolant);
        float fadeToWhiteInterpolant = InverseLerp(0.6f, 0.3f, growthInterpolant);
        float seedOpacity = InverseLerp(0.5f, 0.25f, growthInterpolant);
        float seedWhiteBias = InverseLerp(0f, 0.3f, growthInterpolant);
        if (!disappearing)
        {
            glowInterpolant = 0f;
            fadeToWhiteInterpolant = 0f;
            seedOpacity = 0f;
            seedWhiteBias = 0f;
        }

        if (SeedTarget.TryGetTarget(identifier, out RenderTarget2D? target) && target is not null)
        {
            float[] blurWeights = new float[9];
            for (int k = 0; k < blurWeights.Length; k++)
                blurWeights[k] = GaussianDistribution(k - blurWeights.Length * 0.5f, 1.8f) / 9f;

            Vector2 origin = target.Size() * new Vector2(0.5f, 1f);
            ManagedShader backglowShader = ShaderManager.GetShader("NoxusBoss.AntiseedBackglowShader");
            backglowShader.TrySetParameter("blurWeights", blurWeights);
            backglowShader.TrySetParameter("blurOffset", Vector2.One * 2f / target.Size());
            backglowShader.TrySetParameter("fadeToWhiteInterpolant", fadeToWhiteInterpolant);
            backglowShader.Apply();

            Main.spriteBatch.Draw(target, drawPosition, null, Color.White, rotation, origin, 1f, 0, 0f);

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(target, drawPosition, null, Color.White * (1f - fadeToWhiteInterpolant).Cubed(), rotation, origin, 1f, 0, 0f);
        }

        // Draw the glow when disappearing.
        Texture2D glow = BloomFlare.Value;
        Vector2 glowDrawPosition = drawPosition - Vector2.UnitY * 16f;
        Main.spriteBatch.Draw(glow, glowDrawPosition, null, Color.White with { A = 0 } * glowInterpolant, Pi * growthInterpolant, glow.Size() * 0.5f, glowInterpolant * 0.2f, 0, 0f);

        // Draw the seed when about to fade out.
        ManagedShader whiteBiasShader = ShaderManager.GetShader("NoxusBoss.WhiteColorBiasShader");
        whiteBiasShader.TrySetParameter("whiteInterpolant", seedWhiteBias);
        whiteBiasShader.Apply();

        Texture2D seedTexture = TextureAssets.Item[ItemID].Value;
        Main.spriteBatch.Draw(seedTexture, glowDrawPosition, null, Color.White * seedOpacity, 0f, seedTexture.Size() * 0.5f, 1f, 0, 0f);
    }

    private static void DrawShadowBackglow(float growthInterpolant, Vector2 backglowDrawPosition, SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, SubtractiveBlending, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

        float backgroundBrightness = Vector3.Dot(Main.ColorOfTheSkies.ToVector3(), new Vector3(0.3f, 0.6f, 0.1f));
        float opacityFactor = Utils.Remap(backgroundBrightness, 0.1f, 0.7f, 0.26f, 1f);
        float scaleFactor = Pow(MathF.Min(growthInterpolant, 1f), 4f) * Lerp(0.9f, 1.1f, Cos01(Main.GlobalTimeWrappedHourly * 2.8f));
        Main.spriteBatch.Draw(BloomCircleSmall.Value, backglowDrawPosition, null, Color.White * opacityFactor * 0.27f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 3f, 0, 0f);
        Main.spriteBatch.Draw(BloomCircleSmall.Value, backglowDrawPosition, null, Color.White * opacityFactor * 0.4f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 2f, 0, 0f);
        Main.spriteBatch.Draw(BloomCircleSmall.Value, backglowDrawPosition, null, Color.White * opacityFactor * 0.67f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 1.1f, 0, 0f);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
    }

    private static void DrawInstanceToTarget(float growthInterpolant, ulong seed, int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        Vector2 start = new Vector2(Main.instance.GraphicsDevice.Viewport.Width * 0.5f, Main.instance.GraphicsDevice.Viewport.Height);
        Vector2 desiredEnd = start - Vector2.UnitY * MathF.Max(1f, growthInterpolant) * StemHeight;
        Vector2 effectiveEnd = start - Vector2.UnitY * growthInterpolant * StemHeight;
        Vector2 perpendicular = Vector2.UnitX;

        Vector2[] desiredStemPositions = new Vector2[10];
        Vector2[] effectiveStemPositions = new Vector2[10];
        for (int k = 0; k < desiredStemPositions.Length; k++)
        {
            float completionRatio = k / (float)(desiredStemPositions.Length - 1f);
            Vector2 stemOffset = perpendicular * Sin(Pi * completionRatio * 3f) * -2.5f;
            desiredStemPositions[k] = Vector2.Lerp(start, desiredEnd, completionRatio * 0.9f) + stemOffset;
            effectiveStemPositions[k] = Vector2.Lerp(start, effectiveEnd, completionRatio * 0.9f) + stemOffset;
        }
        DeCasteljauCurve desiredStemCurve = new DeCasteljauCurve(desiredStemPositions);
        DeCasteljauCurve effectiveStemCurve = new DeCasteljauCurve(effectiveStemPositions);

        // Draw the branches.
        Texture2D branch = Branches.Value;
        int branchCount = Utils.RandomInt(ref seed, 2) + 3;
        for (int k = branchCount - 1; k >= 0; k--)
        {
            int branchSide = (Utils.RandomInt(ref seed, 2) == 0).ToDirectionInt();
            int branchVariant = Utils.RandomInt(ref seed, 8);
            float branchRotation = -branchSide * Lerp(0.1f, 0.24f, Utils.RandomFloat(ref seed)) * Pi;

            float positionEvaluationInterpolant = k / (float)(branchCount - 1f) * 0.4f + 0.12f;
            float branchScale = InverseLerp(growthInterpolant, growthInterpolant - 0.5f, positionEvaluationInterpolant);

            Vector2 branchDrawPosition = desiredStemCurve.Evaluate(positionEvaluationInterpolant) + branchRotation.ToRotationVector2() * branchSide * -5f;
            Rectangle branchFrame = branch.Frame(1, 8, 0, branchVariant);

            Vector2 branchOrigin = new Vector2(-0.1f, 0.5f);
            if (branchSide == -1)
                branchOrigin.X = 1f - branchOrigin.X;

            SpriteEffects branchDirection = branchSide == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.spriteBatch.Draw(branch, branchDrawPosition, branchFrame, Color.White, branchRotation, branchFrame.Size() * branchOrigin, new Vector2(1f, 0.6f) * branchScale, branchDirection, 0f);
        }

        // Draw the stem.
        PrimitiveSettings stemSettings = new PrimitiveSettings(c =>
        {
            float width = (1f - Cos01(Pi * c * 3.5f)) * 4f + 1.5f;
            width = Lerp(width, 9f, InverseLerp(0.35f, 0f, c));
            width = Lerp(width, 16f, InverseLerp(0.5f, 0.9f, c).Squared());

            return width * InverseLerp(0f, -0.1f, c - growthInterpolant);
        }, _ => Color.Black, _ => Main.screenPosition, UseUnscaledMatrix: true, ProjectionAreaWidth: Main.instance.GraphicsDevice.Viewport.Width, ProjectionAreaHeight: Main.instance.GraphicsDevice.Viewport.Height);

        PrimitiveRenderer.RenderTrail(desiredStemPositions, stemSettings, 30);

        // Draw branches around the top of the antiseed.
        float topScale = InverseLerp(0.5f, 1f, growthInterpolant).Squared() + MathF.Max(0f, growthInterpolant - 1f);
        branchCount = Utils.RandomInt(ref seed, 3) + 8;
        for (int k = branchCount - 1; k >= 0; k--)
        {
            float branchRotation = TwoPi * k / branchCount;
            Vector2 branchDrawPosition = effectiveStemCurve.Evaluate(0.95f) + branchRotation.ToRotationVector2() * topScale * 50f;
            if (Sin(branchRotation) >= 0.1f)
                continue;

            SpriteEffects branchDirection = SpriteEffects.None;
            int branchVariant = Utils.RandomInt(ref seed, 8);
            if (branchVariant == 2 || branchVariant == 3 || branchVariant == 7)
                branchDirection = SpriteEffects.FlipVertically;

            Rectangle branchFrame = branch.Frame(1, 8, 0, branchVariant);

            Main.spriteBatch.Draw(branch, branchDrawPosition, branchFrame, Color.White, branchRotation + 0.4f, branchFrame.Size() * 0.5f, topScale.Cubed() * 0.7f, branchDirection, 0f);
        }

        // Draw the top part of the antiseed.
        Texture2D top = TopTexture.Value;
        spriteBatch.Draw(top, effectiveEnd, null, Color.White, 0f, new Vector2(0.5f, 0f) * top.Size(), topScale, 0, 0f);

        // Draw the bottom part of the antiseed.
        float bottomScale = InverseLerp(0f, 0.4f, growthInterpolant);
        Texture2D texture = BottomTexture.Value;
        spriteBatch.Draw(texture, start, null, Color.White, 0f, new Vector2(0.5f, 1f) * texture.Size(), bottomScale, 0, 0f);

        spriteBatch.End();
    }
}
