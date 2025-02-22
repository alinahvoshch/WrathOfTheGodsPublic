using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class DirectedArcticBlast : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<AvatarOfEmptiness>
{
    public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.AfterProjectiles;

    /// <summary>
    /// How long this blast has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// How long this blast should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(8f);

    public override string Texture => ModContent.GetInstance<ArcticBlast>().Texture;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 11;
    }

    public override void SetDefaults()
    {
        Projectile.width = 42;
        Projectile.height = 42;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
        Projectile.Opacity = 0f;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        // Ensure the Avatar is present. If he isn't, die immediately.
        if (AvatarOfEmptiness.Myself is null)
        {
            Projectile.Kill();
            return;
        }

        Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;
        Projectile.Opacity = InverseLerp(0f, 14f, Time);

        Projectile.velocity *= 1.024f;

        // Increment time.
        Time++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.White, 1, 2, positionClumpInterpolant: 0.4f);
        return false;
    }

    public float IceTailWidthFunction(float completionRatio)
    {
        float baseWidth = Projectile.width + 24f;
        float tipSmoothenFactor = Sqrt(1f - InverseLerp(0.45f, 0.03f, completionRatio).Cubed());
        return Projectile.scale * baseWidth * tipSmoothenFactor;
    }

    public Color IceTailColorFunction(float completionRatio)
    {
        Color color = Color.Lerp(new Color(26, 132, 217), new Color(132, 13, 23), InverseLerp(0.2f, 0.67f, completionRatio));

        return Projectile.GetAlpha(color);
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ManagedShader iceTailShader = ShaderManager.GetShader("NoxusBoss.IceTailShader");
        iceTailShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly + Projectile.identity * 0.32f);
        iceTailShader.SetTexture(BubblyNoise.Value, 1, SamplerState.LinearWrap);
        iceTailShader.SetTexture(DendriticNoiseZoomedOut.Value, 2, SamplerState.LinearWrap);

        PrimitiveSettings settings = new PrimitiveSettings(IceTailWidthFunction, IceTailColorFunction, _ => Projectile.Size * 0.5f + (Projectile.rotation - PiOver2).ToRotationVector2() * Projectile.scale * 26f, Shader: iceTailShader, Pixelate: true);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 26);
    }
}
