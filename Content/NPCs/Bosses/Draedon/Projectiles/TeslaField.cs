using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.Automators;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles;

public class TeslaField : ModProjectile, IDrawsWithShader, IProjOwnedByBoss<MarsBody>
{
    /// <summary>
    /// How long this field should exist, in frames.
    /// </summary>
    public ref float Lifetime => ref Projectile.ai[0];

    /// <summary>
    /// How scale factor of this tesla field.
    /// </summary>
    public ref float ScaleFactor => ref Projectile.ai[1];

    /// <summary>
    /// How long this field has existed, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[2];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 12;
    }

    public override void SetDefaults()
    {
        Projectile.width = 150;
        Projectile.height = 150;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.timeLeft = 9000;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        if (ScaleFactor <= 0f)
            ScaleFactor = 1f;

        // Idly emit tesla arcs.
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            float arcReachInterpolant = Main.rand.NextFloat();
            int arcLifetime = Main.rand.Next(5, 12);
            Vector2 arcSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(0.4f, 0.4f) * Projectile.Size * Projectile.scale;
            Vector2 arcOffset = Main.rand.NextVector2Unit() * Lerp(50f, 300f, Pow(arcReachInterpolant, 8f) * ScaleFactor);
            NewProjectileBetter(Projectile.GetSource_FromAI(), arcSpawnPosition, arcOffset, ModContent.ProjectileType<SmallTeslaArc>(), 0, 0f, -1, arcLifetime, 1f, Main.rand.NextFloat(1f, 2f));
        }

        Projectile.Opacity = InverseLerp(Lifetime, Lifetime - 15f, Time);
        Projectile.scale = SmoothStep(0f, 1f, InverseLerp(0f, 15f, Time)) + (1f - Projectile.Opacity) * ScaleFactor;

        Time++;
        if (Time >= Lifetime)
            Projectile.Kill();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) =>
        CircularHitboxCollision(Projectile.Center, Projectile.width * 0.42f, targetHitbox);

    public void DrawWithShader(SpriteBatch spriteBatch)
    {
        ManagedShader fieldShader = ShaderManager.GetShader("NoxusBoss.TeslaFieldShader");
        fieldShader.TrySetParameter("size", Projectile.Size * Projectile.scale * 1.7f);
        fieldShader.TrySetParameter("posterizationDetail", 8f);
        fieldShader.SetTexture(TechyNoise.Value, 1, SamplerState.LinearWrap);
        fieldShader.Apply();

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Color color = Color.Lerp(new(255, 35, 26), new(255, 105, 31), Projectile.Center.Length() * 0.28f % 1f);
        if (ScaleFactor >= 6f)
            color = Color.White;

        Main.spriteBatch.Draw(WhitePixel, drawPosition, null, Projectile.GetAlpha(color), 0f, WhitePixel.Size() * 0.5f, Projectile.Size * Projectile.scale * 1.75f, 0, 0f);
    }
}
