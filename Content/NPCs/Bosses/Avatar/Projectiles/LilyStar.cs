using Luminance.Common.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.Particles;
using NoxusBoss.Content.Particles.Metaballs;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class LilyStar : ModProjectile, IProjOwnedByBoss<AvatarOfEmptiness>
{
    public bool SetActiveFalseInsteadOfKill => true;

    public float MaxSpeedBoostFactor;

    public Vector2 FlyDestination
    {
        get => new(Projectile.ai[1], Projectile.ai[2]);
        set
        {
            Projectile.ai[1] = value.X;
            Projectile.ai[2] = value.Y;
        }
    }

    public ref float ZPosition => ref Projectile.ai[0];

    public ref float Time => ref Projectile.localAI[0];

    public static bool VortexTravelVariant => AvatarOfEmptiness.Myself is not null && AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().CurrentState == AvatarOfEmptiness.AvatarAIType.TravelThroughVortex;

    public override string Texture => ModContent.GetInstance<DisgustingStar>().Texture;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 2;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
    }

    public override void SetDefaults()
    {
        Projectile.width = 112;
        Projectile.height = 112;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 240;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        // Decide the scale based on Z position.
        Projectile.scale = 1.6f / Pow(ZPosition + 1f, VortexTravelVariant ? 1.5f : 3f);

        if (MaxSpeedBoostFactor == 0f)
            MaxSpeedBoostFactor = AvatarOfEmptiness.GetAIFloat("LilyStars_MaximumFlySpeedBoostFactor");

        // Move towards the fly destination.
        float flySpeedInterpolant = Lerp(0.01f, 0.15f, Pow(InverseLerp(0f, 40f, Time), 2.2f));
        float flySpeedBoostInterpolant = Projectile.identity / 10f % 1f;
        flySpeedInterpolant *= flySpeedBoostInterpolant * MaxSpeedBoostFactor + 1f;

        if (VortexTravelVariant)
        {
            ZPosition -= 0.16f;
            Projectile.velocity *= 0.98f;
            if (ZPosition < -0.05f)
                Projectile.damage = 0;
            Projectile.Opacity = InverseLerp(-0.77f, 0f, ZPosition).Squared() * InverseLerp(240f, 231f, Projectile.timeLeft);

            if (Projectile.Opacity <= 0f && Projectile.timeLeft <= 230)
                Projectile.Kill();
        }
        else
        {
            Projectile.Center = Vector2.Lerp(Projectile.Center, FlyDestination, flySpeedInterpolant);
            Projectile.velocity *= 1f - flySpeedInterpolant * 1.5f;

            // Approach the start of the screen.
            ZPosition = Lerp(ZPosition, -0.1f, flySpeedInterpolant * 0.4f);

            // Explode once the Z position is small enough.
            if (ZPosition < -0.05f)
                Projectile.Kill();
        }

        Projectile.rotation = Projectile.velocity.ToRotation();

        Time++;
    }

    public override bool? CanDamage() => ZPosition <= 0.1f;

    public override bool PreDraw(ref Color lightColor)
    {
        // Draw spires.
        float spireScale = Lerp(0.85f, 1.1f, Sin01(Main.GlobalTimeWrappedHourly * 17.5f + Projectile.identity)) * Projectile.scale * 0.46f;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Main.spriteBatch.Draw(ChromaticSpires, drawPosition, null, ModifyColor(Color.Violet with { A = 0 }), Projectile.rotation + -PiOver4, ChromaticSpires.Size() * 0.5f, spireScale, 0, 0f);
        Main.spriteBatch.Draw(ChromaticSpires, drawPosition, null, ModifyColor(Color.Violet with { A = 0 }), Projectile.rotation + PiOver4, ChromaticSpires.Size() * 0.5f, spireScale, 0, 0f);

        // Draw the star.
        Color starColor = Color.Lerp(Color.MediumPurple, Color.Red, Projectile.identity / 11f % 0.7f) with { A = 0 };
        starColor = Color.Lerp(starColor, Color.White, 0.2f);
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frame = texture.Frame(1, 2, 0, Projectile.frame);
        for (int i = 0; i < 2; i++)
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, ModifyColor(starColor with { A = 0 }), Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, 0, 0f);

        // Draw bloom.
        Vector2 bloomScaleFactor = Vector2.One * (Sin01(Main.GlobalTimeWrappedHourly * 8f) * 0.46f + 1f);
        Color bloomColor = Color.Lerp(new(210, 0, 0, 0), new(182, 24, 31, 0), Projectile.identity / 10f % 1f);
        if (VortexTravelVariant)
            bloomColor = Color.Lerp(new(255, 255, 255, 0), new(4, 162, 255, 0), Projectile.identity / 20f % 1f);

        bloomScaleFactor.X *= 1f + Projectile.velocity.Length() * InverseLerp(14f, 21f, Projectile.velocity.Length()) * 0.21f;

        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, ModifyColor(bloomColor) with { A = 0 }, Projectile.rotation, BloomCircleSmall.Size() * 0.5f, Projectile.scale * bloomScaleFactor * 2f, 0, 0f);
        for (int i = 0; i < 5; i++)
            Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, ModifyColor(new(255, 16, 30, 0)) with { A = 0 } * 0.8f, Projectile.rotation, BloomCircleSmall.Size() * 0.5f, Projectile.scale * bloomScaleFactor * 1.1f, 0, 0f);
        return false;
    }

    public Color ModifyColor(Color baseColor)
    {
        float backgroundInterpolant = InverseLerp(0.3f, 1.3f, ZPosition);

        // Make colors darker and more translucent near the background to help sell the illusion that they're in the background and not simply downscaled.
        float opacity = Lerp(1f, 0.39f, backgroundInterpolant) * Projectile.Opacity;
        baseColor = Color.Lerp(baseColor, new(51, 12, 89), backgroundInterpolant * (VortexTravelVariant ? 0.2f : 0.65f));
        baseColor.A = 0;

        return baseColor * opacity;
    }

    public override void OnKill(int timeLeft)
    {
        if (VortexTravelVariant)
            return;

        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.DisgustingStarExplode with { Volume = 0.45f, MaxInstances = 0, PitchVariance = 0.15f }, Projectile.Center);

        // Explode into a bunch of gore.
        BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
        for (int i = 0; i < 32; i++)
        {
            Vector2 bloodSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
            Vector2 bloodVelocity = Main.rand.NextVector2Circular(23.5f, 8f) - Vector2.UnitY * 9f;
            if (Main.rand.NextBool(6))
                bloodVelocity *= 1.45f;
            if (Main.rand.NextBool(6))
                bloodVelocity *= 1.45f;
            bloodVelocity += Projectile.velocity * 0.85f;
            bloodVelocity *= 0.64f;

            metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(20f, 48f), Main.rand.NextFloat());
        }

        // Create a bunch of stars.
        for (int i = 0; i < 2; i++)
        {
            int starPoints = Main.rand.Next(3, 9);
            float starScaleInterpolant = Main.rand.NextFloat();
            int starLifetime = (int)Lerp(30f, 60f, starScaleInterpolant);
            float starScale = Lerp(0.9f, 1.5f, starScaleInterpolant) * Projectile.scale;
            Color starColor = Color.Lerp(Color.Red, Color.Wheat, 0.4f) * 0.6f;

            // Calculate the star velocity.
            Vector2 starVelocity = Main.rand.NextVector2Circular(25f, 14f);
            TwinkleParticle star = new TwinkleParticle(Projectile.Center, starVelocity, starColor, starLifetime, starPoints, new Vector2(Main.rand.NextFloat(0.4f, 1.6f), 1f) * starScale, starColor * 0.5f);
            star.Spawn();
        }

        StrongBloom bloom = new StrongBloom(Projectile.Center, Vector2.Zero, Color.Crimson, 3f, 30);
        bloom.Spawn();
    }
}
