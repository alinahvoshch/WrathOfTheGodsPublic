using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;

public class LightLaserCamellia : ModProjectile, IProjOwnedByBoss<NamelessDeityBoss>
{
    /// <summary>
    /// How long this camellia has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.localAI[0];

    /// <summary>
    /// How much this camellia should be stretched.
    /// </summary>
    public ref float Stretch => ref Projectile.localAI[1];

    /// <summary>
    /// How long this camellia should wait for, in frames, before shooting.
    /// </summary>
    public ref float ShootDelay => ref Projectile.ai[0];

    /// <summary>
    /// How long this camellia should exist for, in frames.
    /// </summary>
    public ref float Lifetime => ref Projectile.ai[1];

    public override string Texture => GetAssetPath("Content/NPCs/Bosses/NamelessDeity/Projectiles", Name);

    public override void SetStaticDefaults() => Main.projFrames[Type] = 50;

    public override void SetDefaults()
    {
        Projectile.width = 4;
        Projectile.height = 4;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 9600;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void SendExtraAI(BinaryWriter writer) => writer.Write(Time);

    public override void ReceiveExtraAI(BinaryReader reader) => Time = reader.ReadSingle();

    public override void AI()
    {
        Time++;

        int arcReleaseRate = 5;
        float lifetimeRatio = Time / Lifetime;
        float frameInterpolant = InverseLerpBump(0f, 0.6f, 0.8f, 1f, lifetimeRatio);

        if (Time <= ShootDelay)
            Stretch = InverseLerp(0.2f, 0f, lifetimeRatio) * 0.9f;
        else
        {
            Stretch = Cos01(Time / 5f + Projectile.identity * 1.9f) * -0.35f;
            arcReleaseRate = 1;
        }

        // Release arcs.
        if (Main.netMode != NetmodeID.MultiplayerClient && Time % arcReleaseRate == 0 && Time <= Lifetime - 12f)
        {
            int arcLifetime = Main.rand.Next(9, 23);
            Vector2 arcSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
            Vector2 arcOffset = Projectile.velocity.RotatedByRandom(1.53f) * Main.rand.NextFloat(100f, 560f);
            if (Time >= ShootDelay)
            {
                arcOffset *= 1.56f;
                arcLifetime /= 2;
            }

            Vector2 arcDestination = arcSpawnPosition + arcOffset;
            Vector2 arcLength = (arcDestination - arcSpawnPosition) * Main.rand.NextFloat(0.9f, 1f);
            Projectile.NewProjectileBetter_InheritedOwner(Projectile.GetSource_FromThis(), arcSpawnPosition, arcLength, ModContent.ProjectileType<LightLaserElectricityArc>(), 0, 0f, -1, arcLifetime, 1f);
        }

        Projectile.frame = (int)(frameInterpolant * (Main.projFrames[Type] - 1f));
        Projectile.scale = InverseLerpBump(0f, 15f, Lifetime - 7f, Lifetime, Time) * 0.8f;
        Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;

        if (Time >= Lifetime)
            Projectile.Kill();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.PrepareForShaders();

        float hueOffset = 0f;
        if (Time >= ShootDelay)
            hueOffset = Cos(Main.GlobalTimeWrappedHourly * 11f + Projectile.identity * 3.43f) * 0.1f;

        Vector3[] palette = new Vector3[]
        {
            new Color(125, 0, 123).HueShift(hueOffset).ToVector3(),
            new Color(255, 0, 12).HueShift(hueOffset).ToVector3(),
            new Color(247, 0, 105).HueShift(hueOffset).ToVector3(),
            Color.White.ToVector3(),
            Color.White.ToVector3(),
        };

        ManagedShader flowerShader = ShaderManager.GetShader("NoxusBoss.LightLaserFlowerShader");
        flowerShader.TrySetParameter("disappearanceInterpolant", InverseLerp(Lifetime - 10f, Lifetime, Time));
        flowerShader.TrySetParameter("hueOffset", -Main.GlobalTimeWrappedHourly * 1.3f + Projectile.identity * 0.4f);
        flowerShader.TrySetParameter("gradient", palette);
        flowerShader.TrySetParameter("gradientCount", palette.Length);
        flowerShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        flowerShader.Apply();

        Texture2D texture = GennedAssets.Textures.Projectiles.LightLaserCamellia.Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Vector2 origin = frame.Size() * new Vector2(0.5f, 0.75f);
        Vector2 scale = new Vector2(1f - Stretch * 0.6f, 1f + Stretch) * Projectile.scale;

        Main.spriteBatch.Draw(texture, drawPosition, frame, Projectile.GetAlpha(Color.White), Projectile.rotation, origin, scale, 0, 0f);

        Main.spriteBatch.ResetToDefault();
        return false;
    }

    public override bool ShouldUpdatePosition() => false;
}
