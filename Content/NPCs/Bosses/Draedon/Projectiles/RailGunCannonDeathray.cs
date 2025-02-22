using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.Graphics.ArbitraryScreenDistortion;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles;

public class RailGunCannonDeathray : ModProjectile, IProjOwnedByBoss<MarsBody>
{
    private readonly float[] lengthRatios = new float[10];

    /// <summary>
    /// Whether this railgun has hit a forcefield.
    /// </summary>
    public bool HasHitForcefield
    {
        get;
        set;
    }

    /// <summary>
    /// The <see cref="Owner"/> index in the NPC array.
    /// </summary>
    public int OwnerIndex => (int)Projectile.ai[0];

    /// <summary>
    /// The owner of this laserbeam.
    /// </summary>
    public NPC Owner => Main.npc[OwnerIndex];

    /// <summary>
    /// How long this laserbeam has existed, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[1];

    /// <summary>
    /// How long this laserbeam currently is.
    /// </summary>
    public ref float LaserbeamLength => ref Projectile.ai[2];

    /// <summary>
    /// How long this laserbeam should exist for, in frames.
    /// </summary>
    public static int Lifetime => MarsBody.FirstPhaseMissileRailgunCombo_ShootTime;

    /// <summary>
    /// The maximum length of this laserbeam.
    /// </summary>
    public static float MaxLaserbeamLength => 4000f;

    /// <summary>
    /// The maximum intensity of distortion effects created by this deathray.
    /// </summary>
    public static float MaxDistortionIntensity => 1.2f;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 8000;
    }

    public override void SetDefaults()
    {
        Projectile.width = 146;
        Projectile.height = 146;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.MaxUpdates = 2;
        Projectile.timeLeft = Lifetime * Projectile.MaxUpdates;
        Projectile.hide = true;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        if (OwnerIndex < 0 || OwnerIndex >= Main.maxNPCs || !Owner.active || Owner.type != ModContent.NPCType<MarsBody>())
        {
            Projectile.Kill();
            return;
        }

        Projectile.velocity = Owner.As<MarsBody>().RailgunCannonAngle.ToRotationVector2();

        Vector2 offset = Projectile.velocity.RotatedBy(PiOver2) * 10f;
        offset.Y = Abs(offset.Y);
        Projectile.Center = Owner.As<MarsBody>().LeftHandPosition + offset;

        LaserbeamLength = Clamp(LaserbeamLength + 250f, 0f, MaxLaserbeamLength);
        Projectile.scale = InverseLerpBump(0f, 6f, Lifetime - 10f, Lifetime - 1f, Time);

        ScreenShakeSystem.StartShake(InverseLerp(0f, 10f, Time) * 8f, TwoPi, null, 0.91f);

        if (Projectile.IsFinalExtraUpdate())
            Time++;

        // Erase missiles in the way.
        int missileID = ModContent.ProjectileType<MarsMissile>();
        foreach (Projectile missile in Main.ActiveProjectiles)
        {
            if (missile.type == missileID && Projectile.Colliding(Projectile.Hitbox, missile.Hitbox))
                missile.Kill();
        }

        CalculateLengthRatios();

        if (Time >= 2f)
            EmitEndParticles();
    }

    /// <summary>
    /// Emits bloom and perpendicular particles at the end of the deathray.
    /// </summary>
    public void EmitEndParticles()
    {
        Vector2 offset = Projectile.position - Projectile.oldPosition;

        float bloomScale = Lerp(1.4f, 1.8f, Cos01(TwoPi * Time / 6f)) * LaserWidthFunction(0f) * 0.2f;
        Vector2 perpendicular = Projectile.velocity.RotatedBy(PiOver2);
        Vector2 left = Projectile.Center - perpendicular * Projectile.width * Projectile.scale * 0.5f;
        Vector2 right = Projectile.Center + perpendicular * Projectile.width * Projectile.scale * 0.5f;
        for (int i = 0; i < lengthRatios.Length; i++)
        {
            Vector2 start = Vector2.Lerp(left, right, i / (float)(lengthRatios.Length - 1f));
            Vector2 end = start + Projectile.velocity * LaserbeamLength * lengthRatios[i];
            Vector2 energyVelocity = perpendicular.RotatedByRandom(0.05f) * Main.rand.NextFromList(-1f, 1f) * Main.rand.NextFloat(7f, 82f) + offset;
            SmallSmokeParticle energy = new SmallSmokeParticle(end, energyVelocity, Color.Wheat * 0.05f, new Color(255, 179, 74, 0) * 0.31f, 2f, 115f);
            energy.Spawn();

            StrongBloom bloom = new StrongBloom(end, Vector2.Zero, new Color(255, 175, 30) * 0.15f, bloomScale, 9);
            bloom.Spawn();

            bloom = new(end, Vector2.Zero, Color.Wheat * 0.25f, bloomScale * 0.4f, 9);
            bloom.Spawn();
        }
    }

    private float WidthEndFade(float completionRatio)
    {
        return Lerp(0.15f, 1f, InverseLerp(0f, 1f, completionRatio - Time * 0.092f + 0.4f));
    }

    public float LaserWidthFunction(float completionRatio) => Projectile.width * Projectile.scale * Lerp(0.7f, 1f, Cos01(Main.GlobalTimeWrappedHourly * 72f)) * WidthEndFade(completionRatio);

    public float BloomWidthFunction(float completionRatio) => LaserWidthFunction(completionRatio) * 2.1f;

    public float DistortionWidthFunction(float completionRatio) => Projectile.width * Projectile.scale * 3f;

    public Color LaserColorFunction(float completionRatio) => Projectile.GetAlpha(new Color(255, 139, 63));

    public Color BloomColorFunction(float completionRatio) => Projectile.GetAlpha(new Color(255, 213, 151)) * InverseLerpBump(0.02f, 0.05f, 0.81f, 0.95f, completionRatio) * 0.3f;

    public Color DistortionColorFunction(float completionRatio) => Color.White * Projectile.scale * InverseLerp(0.7f, 0.4f, completionRatio);

    /// <summary>
    /// Calculates the length ratios of this deathray as a consequence of forcefield interactions relative to the overall laserbeam length.
    /// </summary>
    private void CalculateLengthRatios()
    {
        Vector2 perpendicular = Projectile.velocity.RotatedBy(PiOver2);
        Vector2 left = Projectile.Center - perpendicular * Projectile.width * Projectile.scale * 0.5f;
        Vector2 right = Projectile.Center + perpendicular * Projectile.width * Projectile.scale * 0.5f;
        for (int i = 0; i < lengthRatios.Length; i++)
        {
            Vector2 start = Vector2.Lerp(left, right, i / (float)(lengthRatios.Length - 1f));
            Vector2 end = MarsBody.AttemptForcefieldIntersection(start, start + Projectile.velocity * LaserbeamLength);
            lengthRatios[i] = start.Distance(end) / LaserbeamLength;
        }

        if (!HasHitForcefield && lengthRatios.Min() < 0.99f)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Mars.RailgunForcefieldImpact).WithVolumeBoost(1.15f);
            HasHitForcefield = true;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        DrawSelf();

        // Draw the distortion.
        ArbitraryScreenDistortionSystem.QueueDistortionAction(() =>
        {
            float lengthFactor = lengthRatios.Min();

            ManagedShader distortionShader = ShaderManager.GetShader("NoxusBoss.LinearDistortionPrimitiveShader");
            distortionShader.TrySetParameter("directionInterpolant", WrapAngle360(Projectile.velocity.ToRotation()) / TwoPi);
            distortionShader.TrySetParameter("intensityFactor", Projectile.scale * MaxDistortionIntensity);

            List<Vector2> distortionPositions = Projectile.GetLaserControlPoints(12, LaserbeamLength * lengthFactor);
            distortionPositions[0] += Projectile.velocity * 100f;

            PrimitiveSettings distortionSettings = new PrimitiveSettings(DistortionWidthFunction, DistortionColorFunction, Shader: distortionShader);
            PrimitiveRenderer.RenderTrail(distortionPositions, distortionSettings, 45);
        });
        ArbitraryScreenDistortionSystem.QueueDistortionExclusionAction(DrawSelf);

        return false;
    }

    private void DrawSelf()
    {
        // Draw bloom.
        List<Vector2> bloomPositions = Projectile.GetLaserControlPoints(12, LaserbeamLength * Lerp(lengthRatios.Average(), lengthRatios.Min(), 0.67f) * 1.125f);
        ManagedShader shader = ShaderManager.GetShader("NoxusBoss.PrimitiveBloomShader");
        shader.TrySetParameter("innerGlowIntensity", 0.45f);
        PrimitiveSettings bloomSettings = new PrimitiveSettings(BloomWidthFunction, BloomColorFunction, Shader: shader);
        PrimitiveRenderer.RenderTrail(bloomPositions, bloomSettings, 50);

        // Draw the beam.
        ManagedShader deathrayShader = ShaderManager.GetShader("NoxusBoss.RailGunCannonDeathrayShader");
        deathrayShader.TrySetParameter("lengthRatios", lengthRatios.ToArray());
        deathrayShader.SetTexture(WavyBlotchNoiseDetailed, 1, SamplerState.LinearWrap);

        List<Vector2> laserPositions = Projectile.GetLaserControlPoints(12, LaserbeamLength);
        PrimitiveSettings laserSettings = new PrimitiveSettings(LaserWidthFunction, LaserColorFunction, Shader: deathrayShader);
        PrimitiveRenderer.RenderTrail(laserPositions, laserSettings, 70);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 perpendicular = Projectile.velocity.RotatedBy(PiOver2);
        Vector2 left = Projectile.Center - perpendicular * Projectile.width * Projectile.scale * 0.5f;
        Vector2 right = Projectile.Center + perpendicular * Projectile.width * Projectile.scale * 0.5f;
        for (int i = 0; i < lengthRatios.Length; i++)
        {
            float _ = 0f;
            float laserWidth = Projectile.width * Projectile.scale / lengthRatios.Length;
            Vector2 start = Vector2.Lerp(left, right, i / (float)(lengthRatios.Length - 1f));
            Vector2 end = start + Projectile.velocity * LaserbeamLength * lengthRatios[i];
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, laserWidth, ref _))
                return true;
        }

        return false;
    }

    public override bool ShouldUpdatePosition() => false;
}
