using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class ConvergingSnowEnergy : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<AvatarOfEmptiness>
{
    /// <summary>
    /// How long this energy has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    public ref float Angle => ref Projectile.ai[1];

    public ref float Radius => ref Projectile.ai[2];

    /// <summary>
    /// How long this energy should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(3f);

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 100;
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;
    }

    public override void SetDefaults()
    {
        Projectile.width = Main.rand?.Next(36, 100) ?? 100;
        Projectile.height = Projectile.width;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.hide = true;
        Projectile.timeLeft = Lifetime;
        Projectile.Opacity = 0f;

        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        if (AvatarOfEmptiness.Myself is null)
        {
            Projectile.Kill();
            return;
        }

        Vector2 flyDestination = AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().SpiderLilyPosition;
        if (Time == 1f)
        {
            Angle = flyDestination.AngleTo(Projectile.Center);
            Radius = flyDestination.Distance(Projectile.Center);
            Projectile.netUpdate = true;
        }

        float speedUpInterpolant = AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().LilyFreezeInterpolant;
        float angularVelocity = Lerp(0.08f, 0.175f, Projectile.identity / 8f % 1f);

        Radius *= 0.95f - speedUpInterpolant * 0.05f;
        Angle += InverseLerp(40f, 200f, Radius) * Lerp(1f, 1.67f, speedUpInterpolant) * angularVelocity;

        if (Radius >= 40f)
            Projectile.Center = flyDestination + Angle.ToRotationVector2() * new Vector2(1f, 0.56f).SafeNormalize(Vector2.Zero) * Radius;
        else
            Projectile.width = (int)(Projectile.width * 0.91f);

        Projectile.Opacity = InverseLerp(10f, 30f, Time) * InverseLerp(50f, 270f, Radius);

        Time++;
    }

    public override Color? GetAlpha(Color lightColor) => Color.White;

    public float EnergyWidthFunction(float completionRatio)
    {
        float baseWidth = Projectile.width;
        return baseWidth * Projectile.scale;
    }

    public Color EnergyColorFunction(float completionRatio)
    {
        Color energyColor = Color.Lerp(Color.PaleTurquoise, Color.Cyan, Projectile.identity / 10f % 1f);
        energyColor.A = 0;

        return energyColor * InverseLerpBump(0f, 0.4f, 0.6f, 0.9f, completionRatio) * Projectile.Opacity * Convert01To010(completionRatio);
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        if (AvatarOfEmptiness.Myself is null)
            return;

        ManagedShader trailShader = ShaderManager.GetShader("NoxusBoss.ConvergingEnergyShader");
        trailShader.SetTexture(PerlinNoise.Value, 1, SamplerState.LinearWrap);
        trailShader.Apply();

        PrimitiveSettings settings = new PrimitiveSettings(EnergyWidthFunction, EnergyColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: trailShader);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 45);
    }
}
