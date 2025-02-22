using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Enemies.RiftEclipse;

public class FogwoodRoot : ModProjectile
{
    public ref float Time => ref Projectile.ai[0];

    public static int Lifetime => 150;

    public static int GrowDelay => 7;

    public static int ExtendReelbackDelay => 60;

    public static int ExtendDelay => 70;

    public override string Texture => GetAssetPath("Content/NPCs/Enemies/RiftEclipse", Name);

    public override void SetStaticDefaults() => Main.projFrames[Type] = 4;

    public override void SetDefaults()
    {
        Projectile.width = 26;
        Projectile.height = 98;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.hide = true;
        Projectile.timeLeft = Lifetime;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.Bottom = Projectile.Center;
        Projectile.spriteDirection = Main.rand.NextFromList(-1, 1);
        Projectile.rotation = Main.rand.NextFloatDirection() * 0.14f;
    }

    public override void AI()
    {
        Time++;

        if (Time <= GrowDelay)
            Projectile.frame = 0;
        else if (Time <= ExtendReelbackDelay)
            Projectile.frame = 1;
        else if (Time <= ExtendDelay)
            Projectile.frame = 2;
        else
            Projectile.frame = 3;

        // Create upward rubble.
        if (Time == ExtendDelay)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust rubble = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.Stone : DustID.Dirt);
                rubble.velocity = -Vector2.UnitY.RotatedBy(Projectile.rotation + Main.rand.NextFloatDirection() * 0.5f) * Main.rand.NextFloat(2f, 12f);
                rubble.scale = Main.rand.NextFloat(1f, 1.95f);
                rubble.noGravity = rubble.velocity.Length() >= 8f;
            }
        }

        // Fade out.
        Projectile.Opacity = InverseLerp(0f, 54f, Projectile.timeLeft, true);
        Projectile.scale = InverseLerp(0f, 20f, Projectile.timeLeft, true);
        Projectile.gfxOffY = (1f - Projectile.scale) * 30f;
    }

    public override Color? GetAlpha(Color lightColor) => Color.Lerp(Color.Gray, Color.Purple, Projectile.identity * 1191.1476f % 0.6f) * Projectile.Opacity;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCsAndTiles.Add(index);
    }

    public override bool? CanDamage() => Time > ExtendDelay && Projectile.Opacity >= 0.7f;
}
