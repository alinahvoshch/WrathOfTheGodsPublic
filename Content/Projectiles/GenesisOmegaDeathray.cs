using Luminance.Assets;
using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Projectiles;

public class GenesisOmegaDeathray : ModProjectile, IPixelatedPrimitiveRenderer
{
    /// <summary>
    /// The width of this deathray.
    /// </summary>
    public ref float DeathrayWidth => ref Projectile.ai[0];

    /// <summary>
    /// The length of this deathray.
    /// </summary>
    public ref float DeathrayLength => ref Projectile.ai[1];

    /// <summary>
    /// How long this deathray has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[2];

    /// <summary>
    /// How long this deathray should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(3.2f);

    // These exist because if Projectile.width or Projectile.height are too big they'll count as being outside of the world too easily and be killed.
    /// <summary>
    /// The maximum width of this deathray.
    /// </summary>
    public static float MaxWidth => 985f;

    /// <summary>
    /// The maximum height of this deathray.
    /// </summary>
    public static float MaxHeight => 6400f;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 24000;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
    }

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;

        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        Time++;

        if (Time == 1f)
        {
            Projectile.Bottom = Projectile.Center;
            Projectile.netUpdate = true;
        }

        // Make the deathray expand outward.
        DeathrayWidth = SmoothStep(0f, MaxWidth, Sqrt(InverseLerp(0f, 40f, Time))) * InverseLerp(0f, 10f, Projectile.timeLeft);

        // Make the deathray rise upward.
        Projectile.Opacity = InverseLerp(0f, 30f, Time);
        DeathrayLength += (1f - Projectile.Opacity) * 200f + 88f;
        if (DeathrayLength > MaxHeight)
            DeathrayLength = MaxHeight;
    }

    public override bool? CanDamage() => DeathrayLength >= MaxHeight * 0.9f && Projectile.Opacity >= 0.8f;

    public float LaserWidthFunction(float completionRatio)
    {
        float frontExpansionInterpolant = InverseLerp(0.015f, 0.12f, completionRatio);
        float maxSize = DeathrayWidth + completionRatio * DeathrayWidth * 1.2f;
        return EasingCurves.Quadratic.Evaluate(EasingType.Out, 2f, maxSize, frontExpansionInterpolant);
    }

    public Color LaserColorFunction(float completionRatio)
    {
        return Projectile.GetAlpha(Color.White);
    }

    public float BloomWidthFunction(float completionRatio) => LaserWidthFunction(completionRatio) * 1.5f;

    public Color BloomColorFunction(float completionRatio)
    {
        float opacity = InverseLerp(0.01f, 0.065f, completionRatio) * InverseLerp(0.9f, 0.7f, completionRatio) * 0.32f;
        return Projectile.GetAlpha(Color.White) * opacity;
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        List<Vector2> laserPositions = Projectile.GetLaserControlPoints(12, DeathrayLength, -Vector2.UnitY);
        Vector3[] palette = new Vector3[]
        {
            new Color(0, 0, 0).ToVector3(),
            new Color(71, 35, 137).ToVector3(),
            new Color(120, 60, 231).ToVector3(),
            new Color(46, 156, 211).ToVector3(),
            new Color(0, 0, 0).ToVector3(),
            new Color(0, 0, 0).ToVector3(),
            new Color(0, 0, 0).ToVector3()
        };

        ManagedShader shader = ShaderManager.GetShader("NoxusBoss.GenesisOmegaDeathrayShader");
        shader.TrySetParameter("laserDirection", Projectile.velocity);
        shader.TrySetParameter("edgeColorSubtraction", Vector3.One - Color.Black.ToVector3());
        shader.TrySetParameter("edgeGlowIntensity", 0.3f);
        shader.TrySetParameter("gradient", palette);
        shader.TrySetParameter("gradientCount", palette.Length);
        shader.SetTexture(WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
        shader.SetTexture(WatercolorNoiseA, 2, SamplerState.LinearWrap);

        PrimitiveSettings laserSettings = new PrimitiveSettings(LaserWidthFunction, LaserColorFunction, Pixelate: true, Shader: shader);
        PrimitiveRenderer.RenderTrail(laserPositions, laserSettings, 80);
    }
}
