using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;

public class CodeCrack : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<NamelessDeityBoss>
{
    /// <summary>
    /// Whether this lightning crack has played a graze sound yet or not.
    /// </summary>
    public bool HasPlayedGrazeSound
    {
        get => Projectile.localAI[0] == 1f;
        set => Projectile.localAI[0] = value.ToInt();
    }

    /// <summary>
    /// How long this crack has exist for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// The initial direction of this code crack.
    /// </summary>
    public ref float InitialDirection => ref Projectile.ai[1];

    /// <summary>
    /// How long this crack should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(1f);

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 800;
    }

    public override void SetDefaults()
    {
        Projectile.width = 54;
        Projectile.height = 54;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.MaxUpdates = 5;
        Projectile.timeLeft = Projectile.MaxUpdates * Lifetime;
    }

    public override void AI()
    {
        // Die if Nameless is not present.
        if (NamelessDeityBoss.Myself is null)
        {
            Projectile.Kill();
            return;
        }

        if (Time <= 2f)
            InitialDirection = Projectile.velocity.ToRotation();

        // Accelerate.
        if (Time >= 20f)
            Projectile.velocity = (Projectile.velocity * 1.05f).ClampLength(0f, 62f / Projectile.MaxUpdates);

        if (Time < 20f)
            Projectile.position += Projectile.velocity.SafeNormalize(Vector2.Zero) * (1f - Time / 20f) * 5.7f;

        float distanceToPlayer = Main.LocalPlayer.Distance(Projectile.Center);
        if (distanceToPlayer <= 124f && distanceToPlayer >= 70f && !HasPlayedGrazeSound)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.CodeLightningGraze with { Volume = 1.1f, MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew });
            HasPlayedGrazeSound = true;
        }

        // Fade out based on how long the crack has existed.
        Projectile.scale = InverseLerp(0f, 20f, Lifetime - Time);

        if (Projectile.IsFinalExtraUpdate())
        {
            Time++;

            if (Time % 4f == 3f && Time >= 26f)
            {
                float maxAngleVariance = Lerp(0.1f, 1.1f, InverseLerp(30f, 45f, Time));
                float offsetAngle = Main.rand.NextFloatDirection() * maxAngleVariance;
                Vector2 newDirection = (InitialDirection + offsetAngle).ToRotationVector2(); ;

                Projectile.velocity = newDirection * Projectile.velocity.Length();
                Projectile.netUpdate = true;
            }
        }
    }

    public override bool? CanDamage() => Time <= 35f;

    public float FlameTrailWidthFunction(float completionRatio)
    {
        return InverseLerp(0f, 0.2f, completionRatio) * Projectile.width * Projectile.scale;
    }

    public Color FlameTrailColorFunction(float completionRatio)
    {
        return Projectile.GetAlpha(Color.White);
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ManagedShader shader = ShaderManager.GetShader("NoxusBoss.GenericFlameTrail");
        shader.SetTexture(GennedAssets.Textures.TrailStreaks.StreakMagma, 1, SamplerState.LinearWrap);

        PrimitiveSettings settings = new PrimitiveSettings(FlameTrailWidthFunction, FlameTrailColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: shader, Smoothen: false);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 24);
    }
}
