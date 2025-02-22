using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;

public class BigNamelessPunchImpact : ModProjectile, IProjOwnedByBoss<NamelessDeityBoss>
{
    /// <summary>
    /// How long this impact has existed for so far, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// How strong visuals for this impact are.
    /// </summary>
    public ref float VisualsIntensity => ref Projectile.localAI[0];

    /// <summary>
    /// How long the visuals for this impact should last.
    /// </summary>
    public static int Lifetime => SecondsToFrames(3f);

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 50000;

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = Lifetime;
    }

    public override void AI()
    {
        float intensityInterpolant = InverseLerp(0f, 30f, Time);
        VisualsIntensity = Pow(SmoothStep(0f, 1f, intensityInterpolant), 1.4f);

        float shakeIntensity = InverseLerp(0.4f, 0f, VisualsIntensity) * 20f + 2f;
        ScreenShakeSystem.StartShake(shakeIntensity, shakeStrengthDissipationIncrement: 0.5f);

        if (Main.netMode != NetmodeID.MultiplayerClient && Time % 28f == 27f && Projectile.timeLeft >= 90)
        {
            int crackCount = 15;
            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            Vector2 directionToTarget = Projectile.SafeDirectionTo(target.Center);
            for (int i = 0; i < crackCount; i++)
            {
                float crackOffsetAngle = TwoPi * i / crackCount;
                float randomAngleOffset = Main.rand.NextFloatDirection() * InverseLerp(0.95f, 0.84f, Cos(crackOffsetAngle)) * 0.51f;
                Vector2 crackVelocity = directionToTarget.RotatedBy(crackOffsetAngle + randomAngleOffset) * 0.95f;
                Projectile.NewProjectileBetter_InheritedOwner(Projectile.GetSource_FromThis(), Projectile.Center, crackVelocity, ModContent.ProjectileType<CodeCrack>(), 400, 0f);
            }
        }

        Time++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        ManagedScreenFilter punchOverlayShader = ShaderManager.GetFilter("NoxusBoss.StrongRealityPunchOverlayShader");
        punchOverlayShader.TrySetParameter("intensity", VisualsIntensity);
        punchOverlayShader.TrySetParameter("fadeOut", InverseLerp(45f, 5f, Projectile.timeLeft));
        punchOverlayShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        punchOverlayShader.TrySetParameter("impactPosition", Vector2.Transform(Projectile.Center - Main.screenPosition, Main.GameViewMatrix.TransformationMatrix));
        punchOverlayShader.TrySetParameter("codeAppearanceInterpolant", InverseLerp(0.25f, 0.6f, VisualsIntensity));
        punchOverlayShader.TrySetParameter("negativeZoneRadius", Pow(VisualsIntensity, 2.75f) * 3200f);
        punchOverlayShader.TrySetParameter("crackBaseRadius", 500f);
        punchOverlayShader.TrySetParameter("innerGlowRadius", 350f);
        punchOverlayShader.TrySetParameter("redCodeInterpolant", InverseLerp(30f, 60f, Time));
        punchOverlayShader.SetTexture(CrackedNoiseB, 1, SamplerState.LinearWrap);
        punchOverlayShader.SetTexture(WavyBlotchNoise, 2, SamplerState.LinearWrap);
        punchOverlayShader.SetTexture(CodeBackgroundManager.CodeBackgroundTarget, 3, SamplerState.LinearWrap);
        punchOverlayShader.Activate();

        return false;
    }
}
