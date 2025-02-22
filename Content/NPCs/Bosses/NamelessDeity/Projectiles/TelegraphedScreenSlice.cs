using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;

public class TelegraphedScreenSlice : ModProjectile, IProjOwnedByBoss<NamelessDeityBoss>
{
    /// <summary>
    /// How long this slice should spend telegraping the trajectory of impending daggers.
    /// </summary>
    public int ShotProjectileTelegraphTime => (int)Clamp(TelegraphTime * 2f - 22f, 10f, 60f);

    /// <summary>
    /// How long this slice should telegraph for, in frames.
    /// </summary>
    public ref float TelegraphTime => ref Projectile.ai[0];

    /// <summary>
    /// The length of this slice.
    /// </summary>
    public ref float LineLength => ref Projectile.ai[1];

    /// <summary>
    /// The spacing offset used when summoning arrays of daggers from this slice.
    /// </summary>
    public ref float DaggerSpacingOffset => ref Projectile.ai[2];

    /// <summary>
    /// How long this screen slice has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.localAI[0];

    /// <summary>
    /// Whether Nameless is currently performing his sword slashes attack or not.
    /// </summary>
    public bool PerformingSwordSlashes
    {
        get
        {
            if (!Projectile.TryGetGenericOwner(out NPC nameless) || nameless.ModNPC is not NamelessDeityBoss namelessModNPC)
                return false;

            return namelessModNPC.CurrentState == NamelessDeityBoss.NamelessAIType.SwordConstellation;
        }
    }

    public int SliceTime => SecondsToFrames(PerformingSwordSlashes ? 0.26f : (1f / 6f));

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 20000;

    public override void SetDefaults()
    {
        Projectile.width = 105;
        Projectile.height = 105;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.timeLeft = 60000;
        if (PerformingSwordSlashes)
            Projectile.Size *= 1.45f;
    }

    public override void AI()
    {
        if (!Projectile.TryGetGenericOwner(out NPC nameless) || nameless.ModNPC is not NamelessDeityBoss namelessModNPC)
        {
            Projectile.Kill();
            return;
        }

        // Decide the rotation of the line.
        Projectile.rotation = Projectile.velocity.ToRotation();

        // Define the universal opacity.
        Projectile.Opacity = InverseLerp(TelegraphTime + SliceTime - 1f, TelegraphTime + SliceTime - 12f, Time);

        if (Time >= TelegraphTime + SliceTime)
            Projectile.Kill();

        // Split the screen and create daggers if the telegraph is over.
        if (Time == TelegraphTime - 1f)
        {
            bool clockExists = AnyProjectiles(ModContent.ProjectileType<ClockConstellation>());
            bool cosmicLaserExists = AnyProjectiles(ModContent.ProjectileType<SuperCosmicBeam>());
            float sliceWidth = Projectile.width * (clockExists ? 1f : 0.5f) * (cosmicLaserExists ? 0.4f : 1f);

            // Perform screen split and shake effects.
            ScreenShakeSystem.StartShake(1.25f);
            LocalScreenSplitSystem.Start(Projectile.Center + Projectile.velocity * LineLength * 0.5f, SliceTime * 2 + 3, Projectile.rotation, sliceWidth);

            if (clockExists)
                RadialScreenShoveSystem.Start(Projectile.Center + Projectile.velocity * LineLength * 0.5f, 60);

            // Release the daggers. After a graze the sound is significantly quieter.
            if (clockExists)
                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.RealityTear with { Volume = 1.7f, MaxInstances = 1, PitchVariance = 0.24f });

            if (Main.netMode != NetmodeID.MultiplayerClient && !PerformingSwordSlashes)
            {
                float daggerOffset = 48f;
                float daggerSpacing = 192f;
                if (clockExists)
                {
                    daggerSpacing = 160f;
                    daggerOffset = 0f;
                }
                if (cosmicLaserExists)
                {
                    daggerSpacing = 230f;
                    daggerOffset = 0f;
                }
                if (namelessModNPC.CurrentState == NamelessDeityBoss.NamelessAIType.RealityTearDaggers)
                    daggerOffset = 25f;
                if (namelessModNPC.CurrentState == NamelessDeityBoss.NamelessAIType.RealityTearPunches)
                    daggerOffset = -8f;

                int daggerIndex = 0;
                for (float d = daggerOffset + DaggerSpacingOffset; d < LineLength; d += daggerSpacing)
                {
                    float hueInterpolant = d / LineLength * 2f % 1f;
                    Vector2 daggerStartingVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(PiOver2) * 16f;
                    Vector2 left = Projectile.Center + Projectile.velocity * d - daggerStartingVelocity * 3f;
                    Vector2 right = Projectile.Center + Projectile.velocity * d + daggerStartingVelocity * 3f;

                    Projectile.NewProjectileBetter_InheritedOwner(Projectile.GetSource_FromThis(), left, daggerStartingVelocity, ModContent.ProjectileType<LightDagger>(), NamelessDeityBoss.DaggerDamage, 0f, -1, ShotProjectileTelegraphTime, hueInterpolant, daggerIndex);
                    Projectile.NewProjectileBetter_InheritedOwner(Projectile.GetSource_FromThis(), right, -daggerStartingVelocity, ModContent.ProjectileType<LightDagger>(), NamelessDeityBoss.DaggerDamage, 0f, -1, ShotProjectileTelegraphTime, hueInterpolant, daggerIndex + 1);
                    daggerIndex += 2;
                }
            }
        }

        if (Time >= TelegraphTime + SliceTime)
            Projectile.Kill();

        Time++;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (Time <= TelegraphTime)
            return false;

        float _ = 0f;
        Vector2 start = Projectile.Center;
        Vector2 end = start + Projectile.velocity * LineLength;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, Projectile.scale * Projectile.width * 0.9f, ref _);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        // Create a telegraph.
        if (Time <= TelegraphTime)
        {
            float telegraphInterpolant = InverseLerp(0f, TelegraphTime - 4f, Time);
            Color telegraphColor = Color.Lerp(Color.IndianRed, Color.White, Pow(telegraphInterpolant, 0.6f)) * telegraphInterpolant;
            telegraphColor.A = 0;

            Main.spriteBatch.DrawBloomLine(Projectile.Center, Projectile.Center + Projectile.velocity * LineLength, telegraphColor, Projectile.width * telegraphInterpolant * 2f);
        }
        return true;
    }

    public override bool ShouldUpdatePosition() => false;
}
