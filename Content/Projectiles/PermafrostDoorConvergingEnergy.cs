using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Projectiles;

public class PermafrostDoorConvergingEnergy : ModProjectile
{
    /// <summary>
    /// How long this energy has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// How long this energy should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(1.25f);

    public override string Texture => GetAssetPath("Content/MiscProjectiles", Name);

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 600;
    }

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.timeLeft = Lifetime;
        Projectile.Opacity = 0f;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        Time++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.PrepareForShaders();

        float lifetimeRatio = Time / Lifetime;
        float materializeInterpolant = SmoothStep(0f, 1f, Pow(InverseLerp(0f, 0.5f, lifetimeRatio), 0.9f));
        float fadeToWhite = InverseLerp(0.95f, 0.55f, lifetimeRatio).Squared();

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        ManagedShader lightShader = ShaderManager.GetShader("NoxusBoss.LightMaterializationShader");
        lightShader.TrySetParameter("materializeInterpolant", materializeInterpolant);
        lightShader.TrySetParameter("fadeToWhite", fadeToWhite);
        lightShader.TrySetParameter("baseTextureSize", texture.Size());
        lightShader.SetTexture(DendriticNoise, 1, SamplerState.PointWrap);
        lightShader.Apply();

        Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White * InverseLerp(0.75f, 0.7f, lifetimeRatio), 0f, texture.Size() * 0.5f, 1f, 0, 0f);

        Main.spriteBatch.ResetToDefault();
        return false;
    }
}
