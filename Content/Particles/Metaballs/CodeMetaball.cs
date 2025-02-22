using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Particles.Metaballs;

public class CodeMetaball : MetaballType
{
    public override string MetaballAtlasTextureToUse => "NoxusBoss.BasicMetaballHexagon.png";

    public override bool ShouldRender => ActiveParticleCount >= 1 || AnyProjectiles(ModContent.ProjectileType<BigNamelessPunchImpact>());

    public override Func<Texture2D>[] LayerTextures => new Func<Texture2D>[]
    {
        () => CodeBackgroundManager.CodeBackgroundTarget
    };

    public override Color EdgeColor => new(255, 58, 82);

    public override bool ShouldKillParticle(MetaballInstance particle) => particle.Size <= 2f;

    public override bool DrawnManually => true;

    public override void Load()
    {
        On_Main.DrawProjectiles += DrawMetaballs;
        base.Load();
    }

    private static void DrawMetaballs(On_Main.orig_DrawProjectiles orig, Main self)
    {
        CodeMetaball metaball = ModContent.GetInstance<CodeMetaball>();
        if (metaball.ShouldRender)
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            metaball.RenderLayerWithShader();
            Main.spriteBatch.End();
        }

        orig(self);
    }

    public override void UpdateParticle(MetaballInstance particle)
    {
        ref float time = ref particle.ExtraInfo[1];
        time++;

        float fadeOutInterpolant = InverseLerp(0f, 10f, time);
        particle.Size *= 1f - fadeOutInterpolant * 0.04f;
        particle.Size -= fadeOutInterpolant * 4f;
        particle.Velocity *= 0.8f;
        particle.ExtraInfo[0] += particle.Velocity.X * 0.02f;
    }

    public override void DrawInstances()
    {
        AtlasTexture texture = AtlasManager.GetTexture(MetaballAtlasTextureToUse);

        foreach (MetaballInstance particle in Particles)
        {
            float rotation = particle.ExtraInfo[0];
            Main.spriteBatch.Draw(texture, particle.Center - Main.screenPosition, null, Color.White, rotation, null, new Vector2(particle.Size) / texture.Frame.Size(), SpriteEffects.None);
        }

        ExtraDrawing();
    }

    public override void PrepareShaderForTarget(int layerIndex)
    {
        // Store the in an easy to use local variables.
        ManagedShader metaballShader = ShaderManager.GetShader("NoxusBoss.BinaryMetaballShader");

        // Fetch the layer texture. This is the texture that will be overlaid over the greyscale contents on the screen.
        Texture2D layerTexture = LayerTextures[layerIndex]();

        // Calculate the layer scroll offset. This is used to ensure that the texture contents of the given metaball have parallax, rather than being static over the screen
        // regardless of world position.
        // This may be toggled off optionally by the metaball.
        Vector2 screenSize = Main.ScreenSize.ToVector2();
        Vector2 layerScrollOffset = Main.screenPosition / screenSize + CalculateManualOffsetForLayer(layerIndex);
        if (LayerIsFixedToScreen(layerIndex))
            layerScrollOffset = Vector2.Zero;

        float[] blurWeights = new float[9];
        for (int i = 0; i < blurWeights.Length; i++)
            blurWeights[i] = GaussianDistribution(i - blurWeights.Length * 0.5f, 1.8f) / 9f;

        // Supply shader parameter values.
        metaballShader.TrySetParameter("layerSize", layerTexture.Size());
        metaballShader.TrySetParameter("screenSize", screenSize);
        metaballShader.TrySetParameter("layerOffset", layerScrollOffset);
        metaballShader.TrySetParameter("edgeColorBlur", EdgeColor.ToVector4());
        metaballShader.TrySetParameter("blurWeights", blurWeights);
        metaballShader.TrySetParameter("blurOffset", Vector2.One * 3f / layerTexture.Size());
        metaballShader.TrySetParameter("singleFrameScreenOffset", (Main.screenLastPosition - Main.screenPosition) / screenSize);

        // Supply the metaball's layer texture to the graphics device so that the shader can read it.
        metaballShader.SetTexture(layerTexture, 1, SamplerState.LinearWrap);

        // Apply the metaball shader.
        metaballShader.Apply();
    }

    public override void ExtraDrawing()
    {
        int lightningID = ModContent.ProjectileType<CodeLightningArc>();
        foreach (Projectile lightning in Main.ActiveProjectiles)
        {
            if (lightning.type != lightningID)
                continue;

            Color _ = Color.White;
            lightning.ModProjectile?.PreDraw(ref _);
        }
    }
}
