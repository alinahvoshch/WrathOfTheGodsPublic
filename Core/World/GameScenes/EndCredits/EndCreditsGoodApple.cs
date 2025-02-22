using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Dusts;
using NoxusBoss.Content.Items;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.EndCredits;

public class EndCreditsGoodApple : ModProjectile
{
    /// <summary>
    /// Whether this apple has been bitten or not.
    /// </summary>
    public bool Bitten
    {
        get;
        set;
    }

    /// <summary>
    /// The scale of this apple.
    /// </summary>
    public ref float Scale => ref Projectile.ai[0];

    /// <summary>
    /// Whether this apple is for Solyn or not.
    /// </summary>
    public bool ForSolyn => Projectile.ai[1] == 0f;

    /// <summary>
    /// How long this apple has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[2];

    /// <summary>
    /// The angular velocity of this apple.
    /// </summary>
    public ref float AngularVelocity => ref Projectile.localAI[0];

    /// <summary>
    /// The psychic pulse opacity of this apple.
    /// </summary>
    public ref float PsychicPulseOpacity => ref Projectile.localAI[1];

    public override string Texture => GetAssetPath("Content/Items", Bitten ? "GoodAppleBitten" : "GoodApple");

    public override void SetDefaults()
    {
        Projectile.width = 22;
        Projectile.height = 22;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 999999;
    }

    public override void AI()
    {
        Projectile.scale = Scale;
        Projectile.rotation += AngularVelocity;
        AngularVelocity = AngularVelocity.AngleLerp(-Projectile.rotation, 0.071f);
        PsychicPulseOpacity = Saturate(PsychicPulseOpacity - 0.0095f);

        Projectile.spriteDirection = -1;

        if (!ForSolyn)
            Main.LocalPlayer.heldProj = Projectile.whoAmI;

        if (Abs(AngularVelocity) >= 0.04f)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust appleParticle = Dust.NewDustPerfect(Projectile.Top + Main.rand.NextVector2Circular(2f, 10f) + Vector2.UnitY * 14f, ModContent.DustType<TwinkleDust>(), Vector2.Zero);
                appleParticle.scale = Main.rand.NextFloat(0.4f);
                appleParticle.color = Color.Lerp(new(232, 20, 3), new(243, 201, 30), Main.rand.NextFloat());
                appleParticle.velocity = Main.rand.NextVector2Circular(1f, 1f);
                appleParticle.customData = 0;
            }
        }

        Projectile.Opacity = InverseLerp(0f, 90f, Projectile.timeLeft);
        Projectile.velocity.X -= (1f - Projectile.Opacity) * 0.12f;
        Projectile.velocity.Y -= (1f - Projectile.Opacity).Squared() * 1.3f;

        Time++;
    }

    public void Bite()
    {
        Projectile.timeLeft = 90;

        Bitten = true;
        SoundEngine.PlaySound(GoodApple.AppleBiteSound, Projectile.Center);

        for (int i = 0; i < 10; i++)
        {
            Dust appleParticle = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(9f, 9f) + Vector2.UnitX * Projectile.spriteDirection * -10f, ModContent.DustType<TwinkleDust>(), Vector2.Zero);
            appleParticle.scale = Main.rand.NextFloat(0.4f);
            appleParticle.color = Color.Lerp(new(232, 20, 3), new(243, 201, 30), Main.rand.NextFloat());
            appleParticle.velocity = Main.rand.NextVector2Circular(1f, 1f) + Vector2.UnitX.RotatedByRandom(0.5f) * Projectile.spriteDirection * Main.rand.NextFloat(2f, 4f);
            appleParticle.customData = 0;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D apple = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * apple.Height * 0.5f;

        float pulse = (Main.GlobalTimeWrappedHourly * 1.3f + Projectile.identity * 0.5431f) % 1f;

        Main.EntitySpriteDraw(apple, drawPosition, null, Projectile.GetAlpha(new(209, 171, 141, 0)) * (1f - pulse).Squared() * Sqrt(PsychicPulseOpacity), Projectile.rotation, apple.Size() * 0.5f, Projectile.scale * (1f + pulse * 0.5f), Projectile.spriteDirection.ToSpriteDirection());
        Main.EntitySpriteDraw(apple, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, apple.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection.ToSpriteDirection());

        return false;
    }
}
