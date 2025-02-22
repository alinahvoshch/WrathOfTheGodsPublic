using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.Graphics.Automators;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;

public class ExplodingStar : ModProjectile, IDrawsWithShader, IProjOwnedByBoss<NamelessDeityBoss>
{
    public bool ShaderShouldDrawAdditively => false;

    public bool SetActiveFalseInsteadOfKill => true;

    /// <summary>
    /// Whether this projectile was created from Nameless' inward star patterened explosions attack.
    /// </summary>
    public bool FromStarConvergenceAttack
    {
        get
        {
            if (!Projectile.TryGetGenericOwner(out NPC nameless) || nameless.ModNPC is not NamelessDeityBoss namelessModNPC)
                return false;

            return namelessModNPC.CurrentState == NamelessDeityBoss.NamelessAIType.InwardStarPatternedExplosions;
        }
    }

    /// <summary>
    /// The temperature of this star. Used for color calculations.
    /// </summary>
    public ref float Temperature => ref Projectile.localAI[0];

    /// <summary>
    /// How long this star has exieted for, in framed.
    /// </summary>
    public ref float Time => ref Projectile.localAI[1];

    /// <summary>
    /// The base scale for this star.
    /// </summary>
    public ref float ScaleGrowBase => ref Projectile.ai[0];

    /// <summary>
    /// How long this star should exist for, in frames.
    /// </summary>
    public ref float Lifetime => ref Projectile.ai[2];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.width = 200;
        Projectile.height = 200;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.timeLeft = 40;

        if (!Projectile.TryGetGenericOwner(out NPC nameless) || nameless.ModNPC is not NamelessDeityBoss namelessModNPC)
            return;

        if (namelessModNPC.CurrentState == NamelessDeityBoss.NamelessAIType.ConjureExplodingStars)
            Projectile.timeLeft = 30;
        if (FromStarConvergenceAttack)
            Projectile.timeLeft = 15;
        if (namelessModNPC.CurrentState == NamelessDeityBoss.NamelessAIType.CrushStarIntoQuasar)
        {
            Projectile.width = 1600;
            Projectile.height = 1600;
        }
    }

    public override void AI()
    {
        if (!Projectile.TryGetGenericOwner(out NPC nameless) || nameless.ModNPC is not NamelessDeityBoss namelessModNPC)
            return;

        // Initialize the star temperature. This is used for determining colors.
        if (Temperature <= 0f)
            Temperature = Main.rand.NextFloat(3000f, 32000f);

        if (Lifetime <= 0f)
            Lifetime = Projectile.timeLeft;

        Time++;

        // Perform scale effects to do the explosion.
        if (namelessModNPC.CurrentState == NamelessDeityBoss.NamelessAIType.CrushStarIntoQuasar)
        {
            if (Lifetime <= 0.8f && !FromStarConvergenceAttack)
                Projectile.scale = Pow(InverseLerp(0f, Lifetime * 0.8f, Time), 0.6f);
            else
            {
                if (ScaleGrowBase < 1f)
                    ScaleGrowBase = 1.066f;

                Projectile.scale *= ScaleGrowBase;
            }
        }
        else
        {
            if (Time <= 20f && !FromStarConvergenceAttack)
                Projectile.scale = Pow(InverseLerp(0f, Lifetime * 0.5f, Time), 2.7f);
            else
            {
                if (ScaleGrowBase < 1f)
                    ScaleGrowBase = 1.066f;

                Projectile.scale *= ScaleGrowBase;
            }
        }

        float fadeIn = InverseLerp(0.05f, 0.2f, Projectile.scale);
        float fadeOut = InverseLerp(Lifetime, Lifetime * 0.7f, Time);
        Projectile.Opacity = fadeIn * fadeOut;

        // Create screenshake and play explosion sounds when ready.
        if (Time == 11f)
        {
            ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, 5f, TwoPi, Vector2.UnitX, 0.1f);

            SoundStyle explosionSound = FromStarConvergenceAttack ?
                (GennedAssets.Sounds.NamelessDeity.Supernova with { Volume = 0.8f, MaxInstances = 20 }) :
                (GennedAssets.Sounds.NamelessDeity.GenericBurst with { Pitch = 0.5f });

            if (namelessModNPC.CurrentState != NamelessDeityBoss.NamelessAIType.CrushStarIntoQuasar)
                SoundEngine.PlaySound(explosionSound with { MaxInstances = 3 });
            NamelessDeityKeyboardShader.BrightnessIntensity += 0.23f;
        }
        if (fadeOut <= 0.8f)
            Projectile.damage = 0;

        if (Time == Lifetime - 6f)
        {
            Color twinkleColor = Color.Lerp(Color.Aqua, Color.Blue, Main.rand.NextFloat(0.1f, 0.67f));
            TwinkleParticle twinkle = new TwinkleParticle(Projectile.Center, Vector2.Zero, Color.White, 45, 4, Vector2.One * 2.5f, twinkleColor);
            twinkle.Time += 10;
            twinkle.Rotation = Main.rand.NextFloat(TwoPi);
            twinkle.Spawn();
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return CircularHitboxCollision(Projectile.Center, Projectile.width * Projectile.scale * 0.19f, targetHitbox);
    }

    public override void OnKill(int timeLeft)
    {
        if (!Projectile.TryGetGenericOwner(out NPC nameless) || nameless.ModNPC is not NamelessDeityBoss namelessModNPC)
            return;

        // Release an even spread of starbursts.
        int starburstCount = 5;
        int arcingStarburstID = ModContent.ProjectileType<ArcingStarburst>();
        float starburstSpread = TwoPi;
        float starburstSpeed = 22f;
        if (namelessModNPC.CurrentState == NamelessDeityBoss.NamelessAIType.InwardStarPatternedExplosions)
            starburstCount = 7;

        if (namelessModNPC.CurrentState == NamelessDeityBoss.NamelessAIType.BackgroundStarJumpscares)
            return;
        if (namelessModNPC.CurrentState == NamelessDeityBoss.NamelessAIType.CrushStarIntoQuasar)
            return;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Vector2 directionToTarget = Projectile.SafeDirectionTo(Main.player[Player.FindClosest(Projectile.Center, 1, 1)].Center);
            for (int i = 0; i < starburstCount; i++)
            {
                Vector2 starburstVelocity = directionToTarget.RotatedBy(Lerp(-starburstSpread, starburstSpread, i / (float)(starburstCount - 1f)) * 0.5f) * starburstSpeed + Main.rand.NextVector2Circular(starburstSpeed, starburstSpeed) / 11f;
                Projectile.NewProjectileBetter_InheritedOwner(Projectile.GetSource_FromThis(), Projectile.Center, starburstVelocity, arcingStarburstID, NamelessDeityBoss.StarburstDamage, 0f, -1);
            }
        }
    }

    public void DrawWithShader(SpriteBatch spriteBatch)
    {
        float colorInterpolant = InverseLerp(3000f, 32000f, Temperature);
        Color starColor = Color.Lerp(Color.Orange, new(255, 236, 140), colorInterpolant) * Projectile.Opacity;

        ManagedShader fireballShader = ShaderManager.GetShader("NoxusBoss.FireExplosionShader");
        fireballShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly + Projectile.identity * 0.31f);
        fireballShader.TrySetParameter("lifetimeRatio", Time / Lifetime);
        fireballShader.TrySetParameter("explosionShapeIrregularity", 0.17f);
        fireballShader.TrySetParameter("accentColor", new Vector4(0.8f, -0.85f, -1f, 0f));
        fireballShader.SetTexture(FireNoiseA, 1, SamplerState.LinearWrap);
        fireballShader.SetTexture(PerlinNoise, 2, SamplerState.LinearWrap);
        fireballShader.Apply();

        spriteBatch.Draw(InvisiblePixel, Projectile.Center - Main.screenPosition, null, starColor, Projectile.rotation, InvisiblePixel.Size() * 0.5f, Projectile.width * Projectile.scale * 1.3f, SpriteEffects.None, 0f);
    }
}
