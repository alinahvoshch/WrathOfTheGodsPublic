using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Graphics.GenesisEffects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Projectiles;

public class GenesisConvergingEnergy : ModProjectile, IPixelatedPrimitiveRenderer
{
    /// <summary>
    /// How long this energy has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// The current angle of this energy.
    /// </summary>
    public ref float Angle => ref Projectile.ai[1];

    /// <summary>
    /// The radius of this energy.
    /// </summary>
    public ref float Radius => ref Projectile.ai[2];

    /// <summary>
    /// How long this energy should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(3f);

    /// <summary>
    /// The palette that this energy cycles through when rendering.
    /// </summary>
    public static readonly Palette EnergyPalette = new Palette().
        AddColor(new Color(0, 0, 0)).
        AddColor(new Color(71, 35, 137)).
        AddColor(new Color(120, 60, 231)).
        AddColor(new Color(46, 156, 211)).
        AddColor(new Color(0, 0, 0)).
        AddColor(new Color(245, 245, 193)).
        AddColor(new Color(0, 0, 0));

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 100;
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;
    }

    public override void SetDefaults()
    {
        Projectile.width = Main.rand?.Next(15, 50) ?? 100;
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
        Vector2 flyDestination = GenesisVisualsSystem.Position;
        if (Time == 1f)
        {
            Angle = flyDestination.AngleTo(Projectile.Center);
            Radius = flyDestination.Distance(Projectile.Center);
            Projectile.netUpdate = true;
        }

        float erring = AperiodicSin(Projectile.Center.X * 0.0093f + Projectile.Center.Y * 0.0041f + Time / 30f) * 0.07f +
                       AperiodicSin(Projectile.Center.X * 0.0045f + Projectile.Center.Y * 0.0088f + Time / 25f) * 0.07f;

        Radius *= 0.91f;
        Angle += erring * InverseLerp(0f, 16f, Time);

        if (Radius >= 50f)
        {
            Radius -= 6f;
        }
        else if (Time >= 2f)
        {
            Projectile.timeLeft = 3;
            Projectile.MaxUpdates = 4;
            Projectile.Center = flyDestination;

            if (Projectile.width > 9)
                Projectile.width = Utils.Clamp(Projectile.width - 1, 0, 1000);

            if (Projectile.oldPos[^1].WithinRange(Projectile.oldPos[0], 5f))
                Projectile.Kill();
        }

        if (Time >= 2f)
            Projectile.Center = flyDestination + Angle.ToRotationVector2() * new Vector2(1f, 0.56f).SafeNormalize(Vector2.Zero) * Radius;

        Projectile.Opacity = InverseLerp(10f, 30f, Time);

        Time++;
    }

    public override Color? GetAlpha(Color lightColor) => Color.White;

    public float EnergyWidthFunction(float completionRatio)
    {
        float baseWidth = Projectile.width;
        return baseWidth * InverseLerp(0f, 0.4f, completionRatio) * Projectile.scale;
    }

    public Color EnergyColorFunction(float completionRatio)
    {
        float lifetimeRatio = Time / Lifetime + completionRatio * 0.4f;
        float hue = Projectile.identity / 17f;
        hue += InverseLerp(0.4f, 0.5f, lifetimeRatio) * 0.35f;
        hue += InverseLerp(0.81f, 0.9f, lifetimeRatio) * 0.25f;

        return EnergyPalette.SampleColor(hue.Modulo(1f)) * InverseLerpBump(0f, 0.4f, 0.6f, 0.9f, completionRatio) * Projectile.Opacity;
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ManagedShader trailShader = ShaderManager.GetShader("NoxusBoss.ConvergingGenesisEnergyShader");
        trailShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        trailShader.Apply();

        PrimitiveSettings settings = new PrimitiveSettings(EnergyWidthFunction, EnergyColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: trailShader);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 60);
    }
}
