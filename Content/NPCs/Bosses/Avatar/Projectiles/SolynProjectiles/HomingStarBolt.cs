using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Dusts;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Draedon;
using NoxusBoss.Content.NPCs.Friendly;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles.SolynProjectiles;

public class HomingStarBolt : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<BattleSolyn>, INotResistedByMars
{
    public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.BeforeProjectiles;

    /// <summary>
    /// Whether this bolt is a cosmic variant.
    /// </summary>
    public bool CosmicVariant => Projectile.identity % 2 == 0;

    /// <summary>
    /// Whether this bolt should only focus on the Avatar.
    /// </summary>
    public bool OnlyFocusOnAvatar => Projectile.ai[2] == 1f;

    /// <summary>
    /// How long this bolt has existed, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[1];

    /// <summary>
    /// How long this bolt should last, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(6f);

    /// <summary>
    /// The standard palette used by this bolt.
    /// </summary>
    public static readonly Vector4[] StarPalette =
    {
        new Color(255, 165, 0).ToVector4(),
        new Color(255, 105, 180).ToVector4(),
        new Color(253, 190, 15).ToVector4()
    };

    /// <summary>
    /// The palette used used by this bolt when <see cref="CosmicVariant"/> is <see langword="true"/>.
    /// </summary>
    public static readonly Vector4[] CosmicPalette =
    {
        new Color(0, 0, 0).ToVector4(),
        new Color(71, 35, 137).ToVector4(),
        new Color(120, 60, 231).ToVector4(),
        new Color(46, 156, 211).ToVector4(),
        new Color(0, 0, 0).ToVector4(),
        new Color(0, 0, 0).ToVector4(),
        new Color(0, 0, 0).ToVector4()
    };

    /// <summary>
    /// The maximum distance that targets can be away from the bolt before it ceases to notice said targets.
    /// </summary>
    public static float MaxHomeInDistance => 950f;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 23;
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1100;
    }

    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.penetrate = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.friendly = true;
        Projectile.timeLeft = Lifetime;
        Projectile.MaxUpdates = 2;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.DamageType = DamageClass.Generic;
    }

    public override void AI()
    {
        Time++;

        int trailReductionTime = ProjectileID.Sets.TrailCacheLength[Type];
        if (Projectile.penetrate < 2 || Projectile.timeLeft < trailReductionTime)
            Disappear(trailReductionTime);
        else
        {
            NPC? avatar = AvatarOfEmptiness.Myself is null ? AvatarRift.Myself : AvatarOfEmptiness.Myself;
            NPC? potentialTarget = OnlyFocusOnAvatar ? avatar : Projectile.FindTargetWithinRange(MaxHomeInDistance);

            if (!OnlyFocusOnAvatar && potentialTarget is null)
                potentialTarget = MarsBody.Myself;

            if (potentialTarget is not null && potentialTarget.CanBeChasedBy())
                HomeInOnTarget(potentialTarget);
            else if (Projectile.timeLeft > trailReductionTime)
                Projectile.timeLeft = Utils.Clamp(Projectile.timeLeft - 15, trailReductionTime, 10000);
        }

        Dust twinkle = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(40f, 40f), ModContent.DustType<TwinkleDust>(), Vector2.Zero);
        twinkle.scale = Main.rand.NextFloat(0.4f);
        twinkle.color = Color.Lerp(Color.White, CosmicVariant ? Color.Violet : Color.Yellow, Main.rand.NextFloat(0.6f));
        twinkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
        twinkle.customData = 0;

        int size = (int)(InverseLerp(0f, trailReductionTime, Projectile.timeLeft) * 24f);
        Projectile.Resize(size, size);
    }

    /// <summary>
    /// Makes this bolt slow down and disappear, allowing its tail to vanish.
    /// </summary>
    /// <param name="trailReductionTime"></param>
    public void Disappear(int trailReductionTime)
    {
        Projectile.velocity *= 0.75f;
        if (Projectile.timeLeft > trailReductionTime)
            Projectile.timeLeft = trailReductionTime;
        Projectile.MaxUpdates = 1;
    }

    /// <summary>
    /// Makes this bolt home in on an NPC target.
    /// </summary>
    /// <param name="target">The target to home in on.</param>
    public void HomeInOnTarget(NPC target)
    {
        float swirl = Sin(Projectile.timeLeft / 10f) * 0.6f;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.SafeDirectionTo(target.Center).RotatedBy(swirl) * 65f, 0.03f);
        Projectile.velocity += Projectile.SafeDirectionTo(target.Center) * 4f;
    }

    public override bool? CanDamage() => Projectile.penetrate == 2;

    public float BoltWidthFunction(float completionRatio)
    {
        float baseWidth = Projectile.width;
        float tipCutFactor = InverseLerp(0.02f, 0.134f, completionRatio);
        float slownessFactor = Utils.Remap(Projectile.velocity.Length(), 1.5f, 4f, 0.18f, 1f);
        return baseWidth * tipCutFactor * slownessFactor;
    }

    public Color BoltColorFunction(float completionRatio)
    {
        float sineOffset = CalculateSinusoidalOffset(completionRatio);
        return Color.Lerp(Color.White, Color.Black, sineOffset * 0.5f + 0.5f);
    }

    public float CalculateSinusoidalOffset(float completionRatio)
    {
        return Sin(TwoPi * completionRatio + Main.GlobalTimeWrappedHourly * -12f + Projectile.identity) * InverseLerp(0.01f, 0.9f, completionRatio);
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        Vector4[] boltPalette = CosmicVariant ? CosmicPalette : StarPalette;
        ManagedShader trailShader = ShaderManager.GetShader("NoxusBoss.HomingStarBoltShader");
        trailShader.TrySetParameter("gradient", boltPalette);
        trailShader.TrySetParameter("gradientCount", boltPalette.Length);
        trailShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly * 2f + Projectile.identity * 1.8f);
        trailShader.SetTexture(WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
        trailShader.SetTexture(TextureAssets.Extra[ExtrasID.FlameLashTrailShape], 2, SamplerState.LinearWrap);
        trailShader.Apply();

        float perpendicularOffset = Utils.Remap(Projectile.velocity.Length(), 4f, 20f, 0.6f, 2f) * Projectile.width;
        Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(PiOver2) * perpendicularOffset;
        Vector2[] trailPositions = new Vector2[Projectile.oldPos.Length];
        for (int i = 0; i < trailPositions.Length; i++)
        {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float sine = CalculateSinusoidalOffset(i / (float)trailPositions.Length);
            trailPositions[i] = Projectile.oldPos[i] + perpendicular * sine;
        }

        PrimitiveSettings settings = new PrimitiveSettings(BoltWidthFunction, BoltColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: trailShader);
        PrimitiveRenderer.RenderTrail(trailPositions, settings, 12);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Color baseColor = Color.Wheat;
        float luminosity = Vector3.Dot(baseColor.ToVector3(), new Vector3(0.3f, 0.6f, 0.1f));
        baseColor.A = (byte)Utils.Remap(luminosity, 0.1f, 0.4f, 255f, 0f);

        Texture2D glowTexture = TextureAssets.Extra[98].Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 origin = glowTexture.Size() * 0.5f;
        float pulse = Lerp(0.8f, 1.2f, Cos01(Main.GlobalTimeWrappedHourly % 30f * TwoPi * 6f));
        float appearanceInterpolant = InverseLerpBump(0f, 30f, Lifetime - 40f, Lifetime, Time) * pulse * 0.8f;
        Color outerColor = baseColor * appearanceInterpolant;
        Color innerColor = baseColor * appearanceInterpolant * 0.5f;

        int trailReductionTime = ProjectileID.Sets.TrailCacheLength[Type];
        float flicker = Sin(InverseLerp(0f, trailReductionTime, Projectile.timeLeft) * Pi) * InverseLerp(0f, 7f, Projectile.timeLeft) * 0.32f;
        float scaleFactor = BoltWidthFunction(0.5f) / Projectile.width + flicker;

        Vector2 largeScale = new Vector2(0.6f, 8f) * appearanceInterpolant * scaleFactor;
        Vector2 smallScale = new Vector2(0.6f, 2f) * appearanceInterpolant * scaleFactor;
        Main.EntitySpriteDraw(glowTexture, drawPosition, null, outerColor, PiOver2, origin, largeScale, 0);
        Main.EntitySpriteDraw(glowTexture, drawPosition, null, outerColor, 0f, origin, smallScale, 0);
        Main.EntitySpriteDraw(glowTexture, drawPosition, null, innerColor, PiOver2, origin, largeScale * 0.6f, 0);
        Main.EntitySpriteDraw(glowTexture, drawPosition, null, innerColor, 0f, origin, smallScale * 0.6f, 0);

        return false;
    }
}
