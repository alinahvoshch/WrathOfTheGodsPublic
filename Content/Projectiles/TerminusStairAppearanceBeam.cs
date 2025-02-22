using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.Players;
using Terraria;
using Terraria.GameContent.RGB;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Projectiles;

public class TerminusStairAppearanceBeam : ModProjectile
{
    /// <summary>
    /// The owner player of this beam.
    /// </summary>
    public Player Owner => Main.player[Projectile.owner];

    /// <summary>
    /// How long this beam has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// How strong the current player disintegration effect is.
    /// </summary>
    public float DisintegrationInterpolant => Pow(InverseLerp(DisintegrationTime, 0f, Time), 1.5f);

    /// <summary>
    /// How long the player spends appearing from their disintegration animation.
    /// </summary>
    public static int DisintegrationTime => SecondsToFrames(1f);

    /// <summary>
    /// How long the laserbeam spends disappearing after the player has fully appeared.
    /// </summary>
    public static int DisappearTime => SecondsToFrames(0.19f);

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 3200;

    public override void SetDefaults()
    {
        Projectile.width = 72;
        Projectile.height = 72;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 90000;
        Projectile.tileCollide = false;
        Projectile.netImportant = true;
        Projectile.hide = true;
    }

    public override void AI()
    {
        Projectile.Bottom = Owner.Bottom + Vector2.UnitY * 1150f;

        Projectile.scale = SmoothStep(0f, 1f, InverseLerp(DisappearTime, 0f, Time - DisintegrationTime));

        if (Time == 1f && Main.myPlayer == Projectile.owner)
            BlockerSystem.Start(true, false, () => Time <= DisintegrationTime + DisappearTime - 5f);

        if (Projectile.scale <= 0f)
            Projectile.Kill();

        Time++;
    }

    public float BeamWidthFunction(float completionRatio)
    {
        float rapidPulsation = Lerp(0.9f, 1.1f, Cos01(Main.GlobalTimeWrappedHourly * 40f + Projectile.identity * 9f + completionRatio * 3f) * DisintegrationInterpolant);
        float scale = Projectile.scale * rapidPulsation * 0.5f;

        return Projectile.width * scale;
    }

    public Color BeamColorFunction(float completionRatio)
    {
        float erasureNoise = NoiseHelper.GetStaticNoise(Vector2.UnitY * completionRatio * 5f) * NoiseHelper.GetStaticNoise(Vector2.UnitY * completionRatio * 3.3f);
        bool erase = erasureNoise - (1f - InverseLerp(0f, 0.2f, Projectile.scale)) <= 0f;

        return Projectile.GetAlpha(new(252, 44, 79)) * (1f - erase.ToInt());
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        // Render the beam.
        RenderLaserbeam();

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        RenderPlayerDisintegrationEffect();
        Main.spriteBatch.ResetToDefault();

        return false;
    }

    private void RenderLaserbeam()
    {
        float laserHeight = 2500f;
        float bulgeStart = InverseLerp(Projectile.Bottom.Y, Projectile.Bottom.Y - laserHeight, Main.LocalPlayer.Center.Y + DisintegrationInterpolant.Squared() * 900f + 150f);

        ManagedShader beamShader = ShaderManager.GetShader("NoxusBoss.TerminusBeamShader");
        beamShader.TrySetParameter("bulgeVerticalOffset", bulgeStart);
        beamShader.TrySetParameter("bulgeVerticalReach", 0.19f);
        beamShader.TrySetParameter("bulgeHorizontalReach", DisintegrationInterpolant * 39f);
        beamShader.TrySetParameter("redEdgeThreshold", InverseLerp(0.12f, 0.4f, Projectile.scale) * 0.6f);

        List<Vector2> laserPoints = Projectile.GetLaserControlPoints(12, laserHeight, -Vector2.UnitY);
        PrimitiveSettings settings = new PrimitiveSettings(BeamWidthFunction, BeamColorFunction, Shader: beamShader, Pixelate: false);
        PrimitiveRenderer.RenderTrail(laserPoints, settings, 200);
    }

    private void RenderPlayerDisintegrationEffect()
    {
        float immediateDisintegrationInterpolant = InverseLerp(0f, 0.85f, DisintegrationInterpolant);

        PlayerPostProcessingShaderSystem.ApplyPostProcessingEffect(Owner, new(player =>
        {
            ManagedShader animeShader = ShaderManager.GetShader("NoxusBoss.AnimeObliterationShader");
            animeShader.TrySetParameter("scatterDirectionBias", -Vector2.UnitY * SmoothStep(0f, 180f, Pow(immediateDisintegrationInterpolant, 1.9f)));
            animeShader.TrySetParameter("pixelationFactor", Vector2.One * 1.5f / Main.ScreenSize.ToVector2());
            animeShader.TrySetParameter("disintegrationFactor", Pow(immediateDisintegrationInterpolant, 1.6f) * 2f);
            animeShader.TrySetParameter("opacity", InverseLerp(0.75f, 0.5f, DisintegrationInterpolant));
            animeShader.TrySetParameter("silhouetteInterpolant", Projectile.scale);
            animeShader.SetTexture(WavyBlotchNoise, 1);
            animeShader.Apply();
        }, false));

        ManagedRenderTarget? texture = PlayerPostProcessingShaderSystem.FinalPlayerTargets[Owner.whoAmI];
        if (texture is null)
            return;

        Vector2 scale = new Vector2(1f, 1f + DisintegrationInterpolant * 5f);
        Vector2 drawPosition = Owner.Center - Main.screenPosition + Vector2.UnitY * texture.Height * scale.Y * 0.5f;
        Main.spriteBatch.Draw(texture, drawPosition, null, Color.Lerp(Color.White, Color.Black, Projectile.scale), 0f, texture.Size() * new Vector2(0.5f, 1f), scale, 0, 0f);
    }
}
