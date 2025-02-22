using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Luminance.Core.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.Graphics.ArbitraryScreenDistortion;
using NoxusBoss.Core.Graphics.Automators;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles;

public class UnstableMatter : ModProjectile, IDrawsWithShader, IProjOwnedByBoss<MarsBody>
{
    /// <summary>
    /// The looped sound this unstable matter plays.
    /// </summary>
    public LoopedSoundInstance LoopSound
    {
        get;
        private set;
    }

    /// <summary>
    /// How long this unstable matter has existed, in frames.
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
        Projectile.width = 190;
        Projectile.height = 190;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.timeLeft = SecondsToFrames(6f);
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        LoopSound ??= LoopedSoundManager.CreateNew(GennedAssets.Sounds.Mars.UnstableMatterLoop with { Volume = 0.45f }, () => !Projectile.active);
        LoopSound.Update(Projectile.Center);

        Projectile.scale = EasingCurves.Elastic.Evaluate(EasingType.Out, InverseLerp(0f, 90f, Time));
        Projectile.Opacity = InverseLerp(0f, 15f, Time);

        Color sparkColor = Color.Lerp(Color.Red, Color.Magenta, Main.rand.NextFloat(0.45f));
        Vector2 sparkSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) * Projectile.scale * 0.45f;
        Vector2 sparkVelocity = Projectile.SafeDirectionTo(sparkSpawnPosition) * Main.rand.NextFloat(9f, 16f) - Vector2.One * 8f;
        ElectricSparkParticle spark = new ElectricSparkParticle(sparkSpawnPosition, sparkVelocity + Projectile.velocity, sparkColor, Color.White * 0.2f, 12, Vector2.One * Main.rand.NextFloat(0.2f, 0.27f));
        spark.Spawn();

        for (int i = 0; i < 4; i++)
        {
            if (Main.rand.NextBool() && Projectile.timeLeft >= 45)
            {
                float scale = Main.rand.NextFloat(0.24f, 0.36f);
                Color color = Color.White;
                Color bloom = Color.Red;
                if (Main.rand.NextBool())
                {
                    scale *= 0.5f;
                    color = Color.Crimson;
                }

                Vector2 particleSpawnPosition = Projectile.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(150f, 210f) - Vector2.One * 10f + Projectile.velocity * 6f;
                CircularSuctionParticle particle = new CircularSuctionParticle(particleSpawnPosition, Main.rand.NextVector2Circular(12f, 12f), color, bloom, scale, Projectile, 240);
                particle.Spawn();
            }
        }

        if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(3))
        {
            int sparkLifetime = Main.rand.Next(7, 14);
            sparkSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
            Vector2 sparkReach = Main.rand.NextVector2Unit() * Main.rand.NextFloat(105f, 280f);
            NewProjectileBetter(Projectile.GetSource_FromAI(), sparkSpawnPosition, sparkReach, ModContent.ProjectileType<SmallTeslaArc>(), 0, 0f, -1, sparkLifetime, 1f);
        }

        // Fly towards the nearest player at first.
        float flyToTargetInterpolant = InverseLerp(60f, 0f, Time);
        Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
        Projectile.velocity += Projectile.SafeDirectionTo(target.Center) * flyToTargetInterpolant * 0.94f;
        Projectile.velocity = Projectile.velocity.RotateTowards(Projectile.AngleTo(target.Center), flyToTargetInterpolant * 0.021f);

        Projectile.scale *= InverseLerp(0f, 18f, Projectile.timeLeft);

        Time++;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) =>
        CircularHitboxCollision(Projectile.Center, Projectile.width * 0.4f, targetHitbox);

    public void DrawWithShader(SpriteBatch spriteBatch)
    {
        DrawSelf();

        ArbitraryScreenDistortionSystem.QueueDistortionExclusionAction(() =>
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Projectile.scale *= 1.3f;
            DrawSelf();
            Projectile.scale /= 1.3f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        });
        ArbitraryScreenDistortionSystem.QueueDistortionAction(() =>
        {
            Texture2D distortion = GennedAssets.Textures.Extra.RadialDistortion.Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Main.spriteBatch.Draw(distortion, drawPosition, null, Color.White, 0f, distortion.Size() * 0.5f, Projectile.Size * Projectile.scale * 1.9f / distortion.Size(), 0, 0f);
        });
    }

    public void DrawSelf()
    {
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Texture2D glow = BloomCircleSmall.Value;
        Main.spriteBatch.Draw(glow, drawPosition, null, Projectile.GetAlpha(new(255, 255, 255, 0)) * 0.4f, 0f, glow.Size() * 0.5f, Projectile.scale * 1.1f, 0, 0f);
        Main.spriteBatch.Draw(glow, drawPosition, null, Projectile.GetAlpha(new(255, 0, 50, 0)), 0f, glow.Size() * 0.5f, Projectile.scale * 1.2f, 0, 0f);

        Vector3[] palette = new Vector3[]
        {
            Color.Black.ToVector3(),
            new Color(80, 39, 56).ToVector3(),
            new Color(251, 40, 73).HueShift(Projectile.identity % 6f / -96f).ToVector3(),
            new Color(255, 255, 255).ToVector3(),
        };

        ManagedShader matterShader = ShaderManager.GetShader("NoxusBoss.UnstableMatterShader");
        matterShader.TrySetParameter("gradient", palette);
        matterShader.TrySetParameter("gradientCount", palette.Length);
        matterShader.TrySetParameter("warpSmoothness", 1.75f);
        matterShader.TrySetParameter("swirlRingQuantityFactor", 5.1f);
        matterShader.TrySetParameter("swirlAnimationSpeed", 0.5f);
        matterShader.TrySetParameter("swirlProminence", 2.4f);
        matterShader.TrySetParameter("innerGlowIntensity", Lerp(0.03f, 0.06f, Cos01(Main.GlobalTimeWrappedHourly * 40f + Projectile.identity * 13f)));
        matterShader.TrySetParameter("centerDarkeningHarshness", 0.72f);
        matterShader.TrySetParameter("edgeWarpIntensity", 0.12f);
        matterShader.TrySetParameter("edgeWarpAnimationSpeed", 0.8f);
        matterShader.TrySetParameter("size", Projectile.Size);
        matterShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        matterShader.Apply();

        Main.spriteBatch.Draw(WhitePixel, drawPosition, null, Color.White, Projectile.rotation, WhitePixel.Size() * 0.5f, Projectile.scale * Projectile.Size * 1.3f, 0, 0f);
    }
}
