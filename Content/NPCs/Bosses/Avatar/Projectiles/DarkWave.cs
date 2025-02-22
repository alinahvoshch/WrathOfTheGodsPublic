using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Core.Graphics.Automators;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class DarkWave : ModProjectile, IProjOwnedByBoss<AvatarOfEmptiness>, IDrawsWithShader
{
    public const float MinScale = 1.2f;

    public const float MaxScale = 5f;

    public const float MaxRadius = 2000f;

    public const float RadiusExpandRateInterpolant = 0.08f;

    /// <summary>
    /// How long, in frames, this shockwave projectile should exist before disappearing.
    /// </summary>
    public static readonly int Lifetime = SecondsToFrames(1f);

    /// <summary>
    /// Whether this shockwave should be forced to its red variant or not.
    /// </summary>
    public bool ForceRed => Projectile.ai[2] == 1f;

    /// <summary>
    /// Whether this shockwave should be forced to its cyan variant or not.
    /// </summary>
    public bool ForceCyan => Projectile.ai[2] == 2f;

    /// <summary>
    /// The current radius of this shockwave.
    /// </summary>
    public ref float Radius => ref Projectile.ai[0];

    /// <summary>
    /// The opacity override of this shockwave.
    /// </summary>
    public ref float OpacityOverride => ref Projectile.ai[1];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 25000;

    public override void SetDefaults()
    {
        Projectile.width = 72;
        Projectile.height = 72;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = Lifetime;
        Projectile.scale = 0.001f;
    }

    public override void AI()
    {
        // Do screen shake effects.
        if (Projectile.localAI[0] == 0f)
        {
            ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, 7f);
            Projectile.localAI[0] = 1f;
        }

        // Cause the wave to expand outward, along with its hitbox.
        Radius = Lerp(Radius, MaxRadius, RadiusExpandRateInterpolant);
        Projectile.scale = Lerp(MinScale, MaxScale, InverseLerp(Lifetime, 0f, Projectile.timeLeft));

        // Override the opacity if necessary.
        if (OpacityOverride != 0f)
            Projectile.Opacity = OpacityOverride;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return CircularHitboxCollision(Projectile.Center, Radius * 0.4f, targetHitbox);
    }

    public void DrawWithShader(SpriteBatch spriteBatch)
    {
        DrawData explosionDrawData = new DrawData(DendriticNoise, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * Projectile.Opacity);
        Vector3 shockwaveColor = new Vector3(1f, 0.02f, 0.15f);
        if ((Projectile.identity % 2 == 0 && !ForceRed) || ForceCyan)
            shockwaveColor = new(0.063f, 1f, 1f);

        ManagedShader shockwaveShader = ShaderManager.GetShader("NoxusBoss.ShockwaveShader");
        shockwaveShader.TrySetParameter("shockwaveColor", shockwaveColor);
        shockwaveShader.TrySetParameter("screenSize", Main.ScreenSize.ToVector2());
        shockwaveShader.TrySetParameter("explosionDistance", Radius * Projectile.scale * 0.5f);
        shockwaveShader.TrySetParameter("projectilePosition", Projectile.Center - Main.screenPosition);
        shockwaveShader.TrySetParameter("shockwaveOpacityFactor", Projectile.Opacity);
        shockwaveShader.Apply();
        explosionDrawData.Draw(Main.spriteBatch);
    }
}
