using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.Graphics.Automators;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles.SolynProjectiles;

public class DirectionalSolynForcefield2 : ModProjectile, IProjOwnedByBoss<BattleSolyn>, IDrawsWithShader
{
    /// <summary>
    /// The disappearance timer for this forcefield.
    /// </summary>
    public int DisappearTime
    {
        get;
        set;
    }

    /// <summary>
    /// The 0-1 interpolant for this forcefield's disappearance.
    /// </summary>
    public float DisappearanceInterpolant => InverseLerp(0f, 8f, DisappearTime);

    /// <summary>
    /// How long this forcefield has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// The moving average of this forcefield's spin angular velocity.
    /// </summary>
    public ref float SpinSpeedMovingAverage => ref Projectile.ai[1];

    /// <summary>
    /// The 0-1 completion value for impacts to this forcefield.
    /// </summary>
    public ref float ImpactAnimationCompletion => ref Projectile.ai[2];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 20;
    }

    public override void SetDefaults()
    {
        int width = (int)(MarsBody.CarvedLaserbeam_LaserSafeZoneWidth * 1.6f);
        Projectile.width = width;
        Projectile.height = (int)(width * 0.3f);
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 999999;
        Projectile.penetrate = -1;
        Projectile.Opacity = 0f;
    }

    public override void SendExtraAI(BinaryWriter writer) => writer.Write(DisappearTime);

    public override void ReceiveExtraAI(BinaryReader reader) => DisappearTime = reader.ReadInt32();

    public override void AI()
    {
        int ownerIndex = NPC.FindFirstNPC(ModContent.NPCType<BattleSolyn>());
        if (ownerIndex == -1 || MarsBody.Myself is null)
        {
            Projectile.Kill();
            return;
        }

        HandleImpactAnimationTimings();

        if (DisappearTime >= 1)
            DisappearTime++;

        // Fade in and out.
        Projectile.Opacity = InverseLerp(0f, 16f, Time) * (1f - DisappearanceInterpolant);

        if (ImpactAnimationCompletion <= 0f)
        {
            ImpactAnimationCompletion = 0.01f;
            Projectile.netUpdate = true;
        }

        // Decide the current scale.
        float impactCompletionBump = Convert01To010(ImpactAnimationCompletion);
        float impactAFfectedScale = Lerp(1f, 0.6f, impactCompletionBump) + Pow(impactCompletionBump, 4f) * 0.35f;
        Projectile.scale = impactAFfectedScale + DisappearanceInterpolant * 0.75f;

        // Stick to Solyn.
        float offset = Lerp(90f, 84f, Convert01To010(ImpactAnimationCompletion));
        Vector2 idealDirection = Main.npc[ownerIndex].SafeDirectionTo(MarsBody.Myself.Center);
        Projectile.velocity = Projectile.velocity.SafeNormalize(idealDirection).RotateTowards(idealDirection.ToRotation(), 0.05f);
        Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;
        Projectile.Center = Main.npc[ownerIndex].Center + Projectile.velocity * offset;

        Time++;

        if (DisappearanceInterpolant >= 1f)
            Projectile.Kill();
    }

    /// <summary>
    /// Handles the incrementing and resetting of the <see cref="ImpactAnimationCompletion"/> value.
    /// </summary>
    public void HandleImpactAnimationTimings()
    {
        if (ImpactAnimationCompletion > 0f)
        {
            ImpactAnimationCompletion += 0.14f;
            if (ImpactAnimationCompletion >= 1f)
            {
                ImpactAnimationCompletion = 0f;
                Projectile.netUpdate = true;
            }
        }
    }

    /// <summary>
    /// Initiates the disappearance of this forcefield.
    /// </summary>
    public void BeginDisappearing()
    {
        DisappearTime = 1;
        Projectile.netUpdate = true;
    }

    public void DrawWithShader(SpriteBatch spriteBatch)
    {
        ManagedShader forcefieldShader = ShaderManager.GetShader("NoxusBoss.DirectionalSolynForcefieldShader");
        forcefieldShader.TrySetParameter("colorA", new Color(255, 113, 194).ToVector4());
        forcefieldShader.TrySetParameter("colorB", Color.Wheat.ToVector4());
        forcefieldShader.TrySetParameter("glowIntensity", Lerp(0.75f, 3f, Convert01To010(ImpactAnimationCompletion)));
        forcefieldShader.SetTexture(StarDistanceLookup, 1, SamplerState.LinearClamp);
        forcefieldShader.Apply();

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Color color = Projectile.GetAlpha(Color.White);
        Main.spriteBatch.Draw(WhitePixel, drawPosition, null, color, Projectile.rotation, WhitePixel.Size() * 0.5f, Projectile.scale * Projectile.Size * 5f, 0, 0f);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) =>
        Utilities.RotatingHitboxCollision(Projectile, targetHitbox.TopLeft(), targetHitbox.Size());

    public override bool ShouldUpdatePosition() => false;
}
