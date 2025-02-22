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

public class AntimatterBlast : ModProjectile, IProjOwnedByBoss<AvatarOfEmptiness>, IPixelatedPrimitiveRenderer
{
    public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.AfterProjectiles;

    /// <summary>
    /// How long this blast has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// How much the max fall speed of this blast should be boosted.
    /// </summary>
    public ref float MaxFallSpeedBoost => ref Projectile.ai[1];

    /// <summary>
    /// How much the acceleration of this blast should be boosted.
    /// </summary>
    public ref float AccelerationBoost => ref Projectile.ai[2];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
    }

    public override void SetDefaults()
    {
        Projectile.width = Main.rand?.Next(28, 54) ?? 28;
        Projectile.height = Projectile.width;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.hide = true;
        Projectile.timeLeft = 240;

        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        Projectile.velocity.X *= 1.005f;

        if (Abs(Projectile.velocity.X) >= 6f || Time >= 120f)
            Projectile.velocity.Y += InverseLerp(120f, 240f, Time).Squared() * 0.275f + 0.06f + AccelerationBoost;

        float sizeInterpolant = InverseLerp(32f, 190f, Projectile.width);
        float maxFallSpeed = Lerp(24f, 17f, sizeInterpolant) + Time * 0.19f + MaxFallSpeedBoost;
        if (Projectile.velocity.Y > maxFallSpeed)
            Projectile.velocity.Y = maxFallSpeed;

        if (Projectile.timeLeft <= 27)
        {
            Projectile.scale *= 0.877f;
            Projectile.Opacity *= 0.8f;
            Projectile.damage = 0;
            Projectile.velocity *= 1.04f;
        }

        Time++;
    }

    public override bool? CanDamage() => Projectile.velocity.Y >= 0f;

    public float AntimatterWidthFunction(float completionRatio)
    {
        float baseWidth = Projectile.width * Projectile.scale * 0.66f;
        float tipInterpolant = InverseLerp(0.04f, 0.19f, completionRatio);
        float smoothTipCutoff = 1f - Pow(1f - tipInterpolant, 3.5f);
        return smoothTipCutoff * baseWidth;
    }

    public Color AntimatterColorFunction(float completionRatio)
    {
        Color baseColor = Color.Lerp(new Color(48, 0, 127), new Color(0, 84, 132), Projectile.identity / 5f % 1f);
        return Projectile.GetAlpha(baseColor);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        float scaleFactor = Projectile.width * Projectile.scale / 85f;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.Wheat) with { A = 0 } * 0.2f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 1.2f, 0, 0f);
        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.White) with { A = 0 } * 0.4f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 0.64f, 0, 0f);
        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.Orange) with { A = 0 } * 0.4f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 0.3f, 0, 0f);
        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        float hueShift = Main.GlobalTimeWrappedHourly + Projectile.identity * 0.15f;
        Vector3[] palette = new Vector3[]
        {
            // Base colors. Results in biasing towards bright purples and such.
            new Color(0, 0, 0).ToVector3(),
            new Color(70, 3, 74).ToVector3(),

            // Oily colors. Results in strong, vibrant, and varied hues that strongly contrast the base colors and create a vivid look.
            new Color(123, 58, 183).HueShift(hueShift * 0.4f).ToVector3(),
            new Color(255, 0, 157).HueShift(hueShift).ToVector3(),

            // Pure white.
            new Color(255, 255, 255).ToVector3(),
        };

        ManagedShader antimatterShader = ShaderManager.GetShader("NoxusBoss.AntimatterBlastShader");
        antimatterShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly + Projectile.identity * 72.113f);
        antimatterShader.TrySetParameter("gradient", palette);
        antimatterShader.TrySetParameter("gradientCount", palette.Length);
        antimatterShader.TrySetParameter("edgeVanishByDistanceSharpness", 1.6f);
        antimatterShader.SetTexture(WavyBlotchNoiseDetailed, 1, SamplerState.LinearWrap);

        Vector2 perpendicularDirection = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(PiOver2);
        Vector2[] trailPositions = new Vector2[Projectile.oldPos.Length];
        for (int i = 0; i < trailPositions.Length; i++)
        {
            float completionRatio = i / (float)(trailPositions.Length - 1f);
            Vector2 trailPosition = Projectile.oldPos[i];
            if (trailPosition != Vector2.Zero)
            {
                float offsetWave = Sin(TwoPi * completionRatio - Main.GlobalTimeWrappedHourly * 7.5f + Projectile.identity) * 50f;
                trailPosition += perpendicularDirection * offsetWave * completionRatio;
            }

            trailPositions[i] = trailPosition;
        }

        PrimitiveSettings settings = new PrimitiveSettings(AntimatterWidthFunction, AntimatterColorFunction, _ => Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.56f, Pixelate: true, Shader: antimatterShader);
        PrimitiveRenderer.RenderTrail(trailPositions, settings, 23);
    }
}
