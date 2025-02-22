using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Graphics.Automators;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles.SolynProjectiles;

public class SolynImpact : ModProjectile, IProjOwnedByBoss<BattleSolyn>, IDrawsWithShader
{
    /// <summary>
    /// How long this impact effect has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// How long this impact effect should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(0.42f);

    /// <summary>
    /// The palette that this impact can cycle through based on its lifetime.
    /// </summary>
    public static readonly Palette ImpactPalette = new Palette().
        AddColor(Color.Wheat).
        AddColor(Color.HotPink).
        AddColor(Color.Wheat);

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.width = 150;
        Projectile.height = 150;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = Lifetime;
        Projectile.penetrate = -1;
    }

    public override void AI()
    {
        float lifetimeRatio = Time / Lifetime;
        Projectile.Opacity = InverseLerp(1f, 0.84f, lifetimeRatio);
        Projectile.scale = EasingCurves.Cubic.Evaluate(EasingType.InOut, 0.3f, 3f, lifetimeRatio) * InverseLerp(0f, 0.15f, lifetimeRatio) * Projectile.Opacity;

        Time++;
    }

    public void DrawWithShader(SpriteBatch spriteBatch)
    {
        ManagedShader impactShader = ShaderManager.GetShader("NoxusBoss.StarImpactShader");
        impactShader.SetTexture(StarDistanceLookup, 1, SamplerState.LinearClamp);
        impactShader.Apply();

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Color color = Projectile.GetAlpha(ImpactPalette.SampleColor(Time / Lifetime * 0.9f));
        Main.spriteBatch.Draw(WhitePixel, drawPosition, null, color, Projectile.rotation, WhitePixel.Size() * 0.5f, Projectile.scale * Projectile.Size / 0.06f, 0, 0f);
    }
}
