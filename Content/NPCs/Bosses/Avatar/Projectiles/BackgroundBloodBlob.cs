using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class BackgroundBloodBlob : ModProjectile, IProjOwnedByBoss<AvatarOfEmptiness>, IPixelatedPrimitiveRenderer
{
    public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.BeforeProjectiles;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
    }

    public override void SetDefaults()
    {
        Projectile.width = Main.rand?.Next(5, 9) ?? 5;
        Projectile.height = Projectile.width;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 210;
        Projectile.Opacity = 0f;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        Projectile.velocity *= 1.025f;
        Projectile.Opacity = InverseLerp(210f, 192f, Projectile.timeLeft);

        if (Main.netMode == NetmodeID.SinglePlayer)
            Projectile.position += Main.LocalPlayer.position - Main.LocalPlayer.oldPosition;
    }

    public float BloodWidthFunction(float completionRatio)
    {
        float baseWidth = Projectile.width * 0.5f;
        float smoothTipCutoff = SmoothStep(0f, 1f, InverseLerp(0.04f, 0.2f, completionRatio));
        return smoothTipCutoff * baseWidth;
    }

    public Color BloodColorFunction(float completionRatio)
    {
        return Projectile.GetAlpha(new Color(82, 1, 23));
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ManagedShader bloodShader = ShaderManager.GetShader("NoxusBoss.BloodBlobShader");
        bloodShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly + Projectile.identity * 72.113f);
        bloodShader.TrySetParameter("dissolveThreshold", 0f);
        bloodShader.TrySetParameter("accentColor", new Vector4(0.6f, 0.02f, -0.1f, 0f));
        bloodShader.SetTexture(BubblyNoise, 1, SamplerState.LinearWrap);
        bloodShader.SetTexture(DendriticNoiseZoomedOut, 2, SamplerState.LinearWrap);

        PrimitiveSettings settings = new PrimitiveSettings(BloodWidthFunction, BloodColorFunction, _ => Projectile.Size * 0.5f + Projectile.velocity, Pixelate: true, Shader: bloodShader);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 7);
    }
}
