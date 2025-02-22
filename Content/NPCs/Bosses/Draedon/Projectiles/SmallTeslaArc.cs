using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles;

public class SmallTeslaArc : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<MarsBody>
{
    /// <summary>
    /// The width factor to be used in the <see cref="ArcWidthFunction(float)"/>.
    /// </summary>
    public float WidthFactor;

    /// <summary>
    /// The color to be used in the <see cref="ArcColorFunction(float)"/>.
    /// </summary>
    public Color ArcColor;

    /// <summary>
    /// The set of all points used to draw the composite arc.
    /// </summary>
    /// 
    /// <remarks>
    /// This array is not synced, but since it's a purely visual effect it shouldn't need to be.
    /// </remarks>
    public Vector2[] ArcPoints;

    /// <summary>
    /// Whether the tesla arc should be colored red or not.
    /// </summary>
    public bool Red
    {
        get => Projectile.ai[1] == 1f;
        set => Projectile.ai[1] = value.ToInt();
    }

    /// <summary>
    /// Whether the tesla arc is from a synthetic seedling or not.
    /// </summary>
    public bool FromSyntheticSeedling
    {
        get => Projectile.ai[1] == 2f;
        set => Projectile.ai[1] = value ? 2f : 0f;
    }

    /// <summary>
    /// How long this sphere should exist, in frames.
    /// </summary>
    public ref float Lifetime => ref Projectile.ai[0];

    /// <summary>
    /// How long this sphere has existed, in frames.
    /// </summary>
    public ref float Time => ref Projectile.localAI[0];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 12;
    }

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.timeLeft = 300;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public void GenerateArcPoints()
    {
        ArcPoints = new Vector2[25];

        Vector2 start = Projectile.Center;
        Vector2 lengthForPerpendicular = Projectile.velocity.ClampLength(0f, 640f);
        Vector2 end = start + Projectile.velocity * Main.rand.NextFloat(0.67f, 1.2f) + Main.rand.NextVector2Circular(30f, 30f);
        Vector2 farFront = start - lengthForPerpendicular.RotatedByRandom(3.1f) * Main.rand.NextFloat(0.26f, 0.8f);
        Vector2 farEnd = end + lengthForPerpendicular.RotatedByRandom(3.1f) * 4f;

        for (int i = 0; i < ArcPoints.Length; i++)
        {
            ArcPoints[i] = Vector2.CatmullRom(farFront, start, end, farEnd, i / (float)(ArcPoints.Length - 1f));

            if (Main.rand.NextBool(9))
                ArcPoints[i] += Main.rand.NextVector2CircularEdge(10f, 10f);
        }
    }

    public override void AI()
    {
        if (ArcPoints is null)
            GenerateArcPoints();
        else
        {
            for (int i = 0; i < ArcPoints.Length; i += 2)
            {
                float trailCompletionRatio = i / (float)(ArcPoints.Length - 1f);
                float arcProtrudeAngleOffset = Main.rand.NextGaussian(0.63f) + PiOver2;
                float arcProtrudeDistance = Main.rand.NextGaussian(4.6f);
                if (Main.rand.NextBool(100))
                    arcProtrudeDistance *= 7f;
                Vector2 arcOffset = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(arcProtrudeAngleOffset) * arcProtrudeDistance;

                ArcPoints[i] += arcOffset * Convert01To010(trailCompletionRatio);
            }
        }

        Time++;
        if (Time >= Lifetime)
            Projectile.Kill();
    }

    public float ArcWidthFunction(float completionRatio)
    {
        float lifetimeRatio = Time / Lifetime;
        float lifetimeSquish = InverseLerpBump(0.1f, 0.35f, 0.75f, 1f, lifetimeRatio);
        return Lerp(1f, 3f, Convert01To010(completionRatio)) * lifetimeSquish * WidthFactor;
    }

    public Color ArcColorFunction(float completionRatio)
    {
        if (FromSyntheticSeedling)
            return Color.Lerp(Color.White, ArcColor, Saturate(completionRatio.Cubed() * 0.5f + Time * 0.085f));

        return Projectile.GetAlpha(ArcColor);
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        if (ArcPoints is null)
            return;

        float lifetimeRatio = Time / Lifetime;
        ManagedShader shader = ShaderManager.GetShader("NoxusBoss.TeslaArcShader");
        shader.TrySetParameter("lifetimeRatio", lifetimeRatio);
        shader.TrySetParameter("erasureThreshold", FromSyntheticSeedling ? 1f : 0.7f);
        shader.SetTexture(DendriticNoiseZoomedOut, 1, SamplerState.LinearWrap);
        shader.Apply();

        PrimitiveSettings settings = new PrimitiveSettings(ArcWidthFunction, ArcColorFunction, Smoothen: false, Pixelate: true, Shader: shader);

        float colorInterpolant = Projectile.identity / 19f % 1f;
        if (FromSyntheticSeedling)
            ArcColor = new Color(0f, 0.9f, 1f);
        else if (Red)
            ArcColor = Color.Lerp(new Color(1f, 0.2f, 0f), new Color(1f, 0f, 0f), colorInterpolant * 0.25f);
        else
            ArcColor = Color.Lerp(new Color(0.3f, 0.86f, 1f), new Color(0.75f, 0.83f, 1f), colorInterpolant);

        if (Projectile.ai[2] <= 0f)
            Projectile.ai[2] = 1f;
        WidthFactor = Projectile.ai[2] * 1f;

        PrimitiveRenderer.RenderTrail(ArcPoints, settings, 39);
    }

    public override bool ShouldUpdatePosition() => false;
}
