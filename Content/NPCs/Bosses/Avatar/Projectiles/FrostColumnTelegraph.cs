using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.Particles;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class FrostColumnTelegraph : ModProjectile, IPixelatedPrimitiveRenderer
{
    /// <summary>
    /// How long this frost column has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// How far this column should extend.
    /// </summary>
    public ref float ColumnWidth => ref Projectile.ai[1];

    /// <summary>
    /// How high up this column should reach.
    /// </summary>
    public ref float ColumnHeight => ref Projectile.ai[2];

    /// <summary>
    /// How long this frost column should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(1.25f);

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 10;
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 7500;
    }

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
        Projectile.Opacity = 0f;
        Projectile.tileCollide = false;
        Projectile.netImportant = true;
    }

    public override void AI()
    {
        Projectile.Opacity = InverseLerp(0f, 10f, Time) * InverseLerp(0f, 24f, Projectile.timeLeft).Squared() * 0.4f;

        float lifetimeRatio = Time / Lifetime;
        ColumnHeight = Lerp(1500f, 9400f, Pow(lifetimeRatio, 8f));
        ColumnWidth = (int)(SmoothStep(0f, 1f, InverseLerp(0f, 32f, Time)) * 70f) + ColumnHeight * 0.015f;

        Time++;

        if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(Projectile.Opacity))
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 mistSpawnPosition = Projectile.Bottom + Main.rand.NextVector2Circular(ColumnWidth * 1.8f, 30f);
                Color mistColor = Color.Lerp(new(145, 187, 255), Color.LightCyan, Main.rand.NextFloat()) * Projectile.Opacity * 0.6f;
                LargeMistParticle iceMist = new LargeMistParticle(mistSpawnPosition, Main.rand.NextVector2Circular(7f, 7f) - Vector2.UnitY * Main.rand.NextFloat(8f, 80f), mistColor, Projectile.scale * 8f, 0f, Main.rand.Next(75, 105), 0.01f, true);
                iceMist.Spawn();
            }
        }
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.FrostColumnBurst);
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NewProjectileBetter(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FrostColumn>(), AvatarOfEmptiness.FrostColumnDamage, 0f);

        for (int i = 0; i < 45; i++)
        {
            Vector2 mistSpawnPosition = Projectile.Bottom + Main.rand.NextVector2Circular(200f, 20f);
            Color mistColor = Color.Lerp(new(145, 187, 255), Color.LightCyan, Main.rand.NextFloat()) * 0.28f;
            LargeMistParticle iceMist = new LargeMistParticle(mistSpawnPosition, Main.rand.NextVector2Circular(7f, 7f) - Vector2.UnitY * Main.rand.NextFloat(8f, 100f), mistColor, Projectile.scale * 8f, 0f, Main.rand.Next(35, 75), 0.01f, true);
            iceMist.Spawn();
        }
    }

    public float ColumnWidthFunction(float completionRatio) => Lerp(1f, 1.7f, Convert01To010(completionRatio)) * ColumnWidth;

    public Color ColumnColorFunction(float completionRatio) => Projectile.GetAlpha(new(35, 116, 228)) * InverseLerpBump(0.03f, 0.4f, 0.6f, 0.97f, completionRatio);

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ManagedShader telegraphShader = ShaderManager.GetShader("NoxusBoss.FrostColumnShader");

        PrimitiveSettings settings = new PrimitiveSettings(ColumnWidthFunction, ColumnColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: telegraphShader);
        PrimitiveRenderer.RenderTrail(Projectile.GetLaserControlPoints(8, ColumnHeight, -Vector2.UnitY), settings, 15);
    }
}
