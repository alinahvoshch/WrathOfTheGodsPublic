using Luminance.Common.DataStructures;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.Particles;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class DeadStarIron : ModProjectile, IProjOwnedByBoss<AvatarOfEmptiness>
{
    public bool StartedByFallingDownward
    {
        get => Projectile.ai[1] == 1f;
        set => Projectile.ai[1] = value.ToInt();
    }

    public ref float Time => ref Projectile.ai[0];

    public override string Texture => GetAssetPath("Content/NPCs/Bosses/Avatar/Projectiles", Name);

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 2;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
    }

    public override void SetDefaults()
    {
        Projectile.width = 26;
        Projectile.height = 26;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 420;
        Projectile.scale = 1.4f;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        // Decide a frame to use.
        if (Projectile.localAI[0] == 0f)
        {
            Projectile.frame = Main.rand.Next(Main.projFrames[Type]);
            StartedByFallingDownward = Projectile.velocity.Y > 0f;
            Projectile.localAI[0] = 1f;
            Projectile.netUpdate = true;
        }

        // Fall downward.
        Projectile.velocity.X *= 0.995f;
        Projectile.velocity.Y += 0.32f;
        if (!StartedByFallingDownward && Projectile.velocity.Y > 20f)
            Projectile.velocity.Y = 20f;

        // Only collide with tiles if below the nearest player.
        Player closestPlayer = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
        Projectile.tileCollide = Projectile.Center.Y >= closestPlayer.Center.Y && Time >= 32f;

        // Decide the current rotation.
        Projectile.rotation += Projectile.velocity.Y * 0.021f;

        Time++;
    }

    public override void OnKill(int timeLeft)
    {
        // Create iron particles upon hitting the ground.
        for (int i = 0; i < 16; i++)
        {
            Color ironColor = Color.Lerp(new(90, 70, 55), new(181, 166, 153), Main.rand.NextFloat(0.81f));
            ironColor.A = 0;

            float particleScale = Main.rand.NextFloat(0.9f, 1.5f);
            SmallSmokeParticle iron = new SmallSmokeParticle(Projectile.Center, Projectile.velocity * 0.04f + particleScale * Main.rand.NextVector2Circular(8.5f, 8.5f), ironColor * 0.9f, ironColor * 0.4f, particleScale * 2.5f, 50f, Main.rand.NextFloatDirection() * 0.02f)
            {
                Rotation = Main.rand.NextFloat(TwoPi),
            };
            iron.Spawn();
        }

        SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact with { Volume = 0.9f, PitchVariance = 0.2f, MaxInstances = 10, Pitch = -0.4f }, Projectile.Center);
    }

    public override bool? CanDamage() => Time >= 16f || Projectile.velocity.Y >= 7f;

    public override bool PreDraw(ref Color lightColor)
    {
        Color drawColor = Color.Lerp(new(255, 210, 210, 180), new(255, 102, 102), InverseLerp(36f, 0f, Time));
        DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], drawColor, positionClumpInterpolant: 0.75f);
        return false;
    }
}
