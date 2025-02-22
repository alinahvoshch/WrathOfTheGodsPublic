using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class ArcticBlast : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<AvatarOfEmptiness>
{
    public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.AfterProjectiles;

    /// <summary>
    /// The damage grace period for this blast.
    /// </summary>
    public int DamageGracePeriod
    {
        get;
        set;
    }

    /// <summary>
    /// The starting Y position of this blast.
    /// </summary>
    public float StartingYPosition
    {
        get;
        set;
    }

    /// <summary>
    /// The horizontal spin direction of this blast.
    /// </summary>
    public ref float HorizontalSpinDirection => ref Projectile.ai[0];

    /// <summary>
    /// How long this blast has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[1];

    /// <summary>
    /// The Z position of this blast.
    /// </summary>
    public ref float ZPosition => ref Projectile.ai[2];

    /// <summary>
    /// How long this blast should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(8f);

    public override string Texture => GetAssetPath("Content/NPCs/Bosses/Avatar/Projectiles", Name);

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
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
        if (DamageGracePeriod == 0)
            DamageGracePeriod = AvatarOfEmptiness.WhirlingIceStorm_ArcticBlastDamageGracePeriod;

        // Ensure the Avatar is present. If he isn't, die immediately.
        if (AvatarOfEmptiness.Myself is null)
        {
            Projectile.Kill();
            return;
        }

        if (StartingYPosition == 0f)
            StartingYPosition = Projectile.Center.Y;

        Projectile.scale = 1f / (ZPosition + 1f);
        Projectile.Resize((int)(Projectile.scale * 42f), (int)(Projectile.scale * 42f));

        if (Projectile.timeLeft < 60)
        {
            ZPosition += 0.1f;
            Projectile.velocity *= 1.02f;
        }
        else
            Projectile.velocity = SpinAround(HorizontalSpinDirection, (int)Time, StartingYPosition, ref ZPosition);

        Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;
        Projectile.Opacity = InverseLerp(Lifetime, Lifetime - 8f, Projectile.timeLeft);

        // Increment time.
        Time++;
    }

    /// <summary>
    /// Performs spin-around motion for a given blast.
    /// </summary>
    /// <param name="horizontalSpinDirection">The horizontal spin direction.</param>
    /// <param name="time">How many frames this blast has existed for.</param>
    /// <param name="startingYPosition">The starting Y position the blast.</param>
    /// <param name="zPosition">The Z position of the blast.</param>
    public static Vector2 SpinAround(float horizontalSpinDirection, int time, float startingYPosition, ref float zPosition)
    {
        float spinCompletion = InverseLerp(0f, Lifetime - 60, time);
        float maxHorizontalSpeed = Cos(TwoPi * startingYPosition / 1040f) * 6f + 20f;
        float spinArc = Pi * spinCompletion * 4f;
        zPosition = Saturate(Cos01(spinArc)).Cubed() * 3f;
        return spinArc.ToRotationVector2() * new Vector2(maxHorizontalSpeed * horizontalSpinDirection, 5f);
    }

    public override bool? CanDamage() => ZPosition <= 0.27f && Projectile.timeLeft <= Lifetime - DamageGracePeriod;

    public override Color? GetAlpha(Color lightColor)
    {
        float darknessInterpolant = InverseLerp(0.2f, 0.6f, ZPosition);
        Color baseColor = Color.Lerp(lightColor, new Color(47, 47, 47) * 0.4f, darknessInterpolant * 0.9f);

        return baseColor * Projectile.Opacity;
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
        Rectangle viewBox = Projectile.Hitbox;
        Rectangle screenBox = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
        viewBox.Inflate(540, 540);
        if (!viewBox.Intersects(screenBox))
            return;

        ManagedShader iceTailShader = ShaderManager.GetShader("NoxusBoss.IceTailShader");
        iceTailShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly + Projectile.identity * 0.32f);
        iceTailShader.SetTexture(BubblyNoise.Value, 1, SamplerState.LinearWrap);
        iceTailShader.SetTexture(DendriticNoiseZoomedOut.Value, 2, SamplerState.LinearWrap);

        PrimitiveSettings settings = new PrimitiveSettings(IceTailWidthFunction, IceTailColorFunction, _ => Projectile.Size * 0.5f + (Projectile.rotation - PiOver2).ToRotationVector2() * Projectile.scale * 26f, Shader: iceTailShader, Pixelate: true);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 8);
    }
}
