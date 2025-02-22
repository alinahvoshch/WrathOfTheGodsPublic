using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Projectiles.Visuals;

public class LotusOfCreationAppearanceVisual : ModProjectile, IPixelatedPrimitiveRenderer
{
    /// <summary>
    /// How long this visual has existed so far, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// How long this visual should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(1.7f);

    public override string Texture => GetAssetPath("Content/MiscProjectiles", Name);

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        // Fade in and out based on time.
        Projectile.scale = Convert01To010(Time / Lifetime).Squared();
        Projectile.rotation = Pi * Time / Lifetime * 1.5f + Pi / 3f;

        Time++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D gleam = BrightSpires.Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Main.EntitySpriteDraw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.Wheat) with { A = 0 }, Projectile.rotation, BloomCircleSmall.Size() * 0.5f, Projectile.scale * 1.9f, 0);
        Main.EntitySpriteDraw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.Gold) with { A = 0 } * 0.5f, Projectile.rotation, BloomCircleSmall.Size() * 0.5f, Projectile.scale * 3.3f, 0);
        Main.EntitySpriteDraw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.Orange) with { A = 0 } * 0.45f, Projectile.rotation, BloomCircleSmall.Size() * 0.5f, Projectile.scale * 5.1f, 0);
        Main.EntitySpriteDraw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.DeepPink) with { A = 0 } * 0.4f, Projectile.rotation, BloomCircleSmall.Size() * 0.5f, Projectile.scale * 7.5f, 0);
        Main.EntitySpriteDraw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.IndianRed) with { A = 0 } * 0.3f, Projectile.rotation, BloomCircleSmall.Size() * 0.5f, Projectile.scale * 12f, 0);
        Main.EntitySpriteDraw(gleam, drawPosition, null, Color.White with { A = 0 }, Projectile.rotation, gleam.Size() * 0.5f, Projectile.scale * 0.6f, 0);

        return false;
    }

    /// <summary>
    /// Draws a god ray.
    /// </summary>
    /// <param name="color">The color of the god ray.</param>
    /// <param name="start">The starting position of the god ray.</param>
    /// <param name="end">The ending position of the god ray.</param>
    /// <param name="startingWidth">The starting width of the god ray.</param>
    /// <param name="endingWidth">The ending width of the god ray</param>
    /// <param name="pixelated">Whether the god ray should be pixelated or not.</param>
    public static void DrawGodRay(Color color, Vector2 start, Vector2 end, float startingWidth, float endingWidth, bool pixelated)
    {
        PrimitiveSettings settings = new PrimitiveSettings(c => Lerp(startingWidth, endingWidth, c), c => color * (1f - c), null, Pixelate: pixelated);
        PrimitiveRenderer.RenderTrail(new Vector2[]
        {
            start,
            Vector2.Lerp(start, end, 0.2f),
            Vector2.Lerp(start, end, 0.4f),
            Vector2.Lerp(start, end, 0.6f),
            Vector2.Lerp(start, end, 0.8f),
            end
        }, settings, 38);
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ulong seed = (ulong)Projectile.identity;
        for (int i = 0; i < 16; i++)
        {
            float rayInterpolant = i / 16f;
            float godRayWidthFactor = Utils.RandomFloat(ref seed) * InverseLerp(0f, 0.24f, Time / Lifetime) * Projectile.scale;
            float godRaySpinSpeed = Lerp(0.4f, 1.2f, Utils.RandomFloat(ref seed)) * (Utils.RandomInt(ref seed, 2) == 0).ToDirectionInt();
            Vector2 godRayDirection = (Projectile.rotation * godRaySpinSpeed + TwoPi * rayInterpolant).ToRotationVector2();
            Vector2 godRayReach = godRayDirection * InverseLerp(0f, 0.32f, Time / Lifetime) * Lerp(50f, 500f, Utils.RandomFloat(ref seed));

            float hue = (rayInterpolant * 0.27f + Time / Lifetime * 0.15f - 0.13f).Modulo(1f);
            Color godRayColor = Main.hslToRgb(hue, 0.92f, 0.54f, 0) * Projectile.Opacity;
            DrawGodRay(godRayColor, Projectile.Center, Projectile.Center + godRayReach, godRayWidthFactor * 10f, godRayWidthFactor * 56f, true);
        }
    }
}
