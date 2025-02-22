using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Fixes;
using NoxusBoss.Core.Graphics.Players;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using NoxusBoss.Core.World.GameScenes.TerminusStairway;
using NoxusBoss.Core.World.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Projectiles;

public class TerminusProjectiveBeam : ModProjectile
{
    /// <summary>
    /// The owner player of this beam.
    /// </summary>
    public Player Owner => Main.player[Projectile.owner];

    /// <summary>
    /// How long this beam has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    public float DisintegrationInterpolant => Pow(InverseLerp(DisintegrationDelay, DisintegrationTime, Time), 1.5f);

    public static int DisintegrationDelay => SecondsToFrames(0.166f);

    public static int DisintegrationTime => SecondsToFrames(1.6f);

    public static int ScaleGrowthTime => SecondsToFrames(0.18f);

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.width = 72;
        Projectile.height = 72;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 90000;
        Projectile.tileCollide = false;
        Projectile.netImportant = true;
        Projectile.Opacity = 0f;
        Projectile.hide = true;
    }

    public override void AI()
    {
        Projectile.Bottom = Owner.Bottom + Vector2.UnitY * (Owner.gfxOffY + Owner.height - 2f);

        Projectile.scale = SmoothStep(0f, 1f, InverseLerp(0f, ScaleGrowthTime, Time).Squared()) * 0.7f;
        Projectile.Opacity = InverseLerp(1f, 4f, Time);

        if (Time == 1f && Main.myPlayer == Projectile.owner)
            BlockerSystem.Start(true, false, () => !TerminusStairwaySystem.Enabled);

        float panInterpolant = SmoothStep(0f, 1f, InverseLerp(0.25f, 1f, DisintegrationInterpolant));
        CameraPanSystem.PanTowards(Projectile.Bottom - Vector2.UnitY * 800f, panInterpolant);

        TotalScreenOverlaySystem.OverlayInterpolant = InverseLerp(DisintegrationTime - 20f, DisintegrationTime + 20f, Time) * 1.1f;
        TotalScreenOverlaySystem.OverlayColor = Color.White;

        if (Time >= DisintegrationTime + 20f)
        {
            if (EternalGardenUpdateSystem.WasInSubworldLastUpdateFrame)
            {
                EternalGardenNew.ClientWorldDataTag = EternalGardenNew.SafeWorldDataToTag("Client", false);
                EternalGardenIntroBackgroundFix.ShouldDrawWhite = true;
                SubworldSystem.Exit();

                return;
            }

            int terminusID = ModContent.ProjectileType<TerminusProj>();
            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (projectile.type == terminusID)
                    projectile.Kill();
            }

            Projectile.Kill();
            TerminusStairwaySystem.Start();

            // Undo the camera pan effect immediately.
            CameraPanSystem.PanTowards(Main.LocalPlayer.Center, 0f);
        }

        Time++;
    }

    public float BeamWidthFunction(float completionRatio)
    {
        float rapidPulsation = Lerp(0.9f, 1.1f, Cos01(Main.GlobalTimeWrappedHourly * 60f + Projectile.identity * 9f + completionRatio * 3f) * DisintegrationInterpolant);
        float scale = Projectile.scale * rapidPulsation * 0.5f;
        float bottomPinch = Pow(InverseLerp(0.007f, 0.02f, completionRatio), 0.5f);

        return Projectile.width * scale * bottomPinch;
    }

    public Color BeamColorFunction(float completionRatio)
    {
        return Projectile.GetAlpha(new(252, 44, 79));
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        // Failsafe to ensure that the player doesn't get softlocked via the blocker system.
        Main.gamePaused = false;

        // Render the beam.
        float immediateDisintegrationInterpolant = InverseLerp(0f, 0.85f, DisintegrationInterpolant);
        RenderLaserbeam(immediateDisintegrationInterpolant);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        RenderPlayerDisintegrationEffect(immediateDisintegrationInterpolant);
        Main.spriteBatch.ResetToDefault();

        return false;
    }

    private void RenderLaserbeam(float immediateDisintegrationInterpolant)
    {
        ManagedShader beamShader = ShaderManager.GetShader("NoxusBoss.TerminusBeamShader");
        beamShader.TrySetParameter("bulgeVerticalOffset", Pow(immediateDisintegrationInterpolant, 1.6f) * 0.5f - 0.045f);
        beamShader.TrySetParameter("bulgeVerticalReach", Lerp(0.12f, 0.35f, immediateDisintegrationInterpolant));
        beamShader.TrySetParameter("bulgeHorizontalReach", Lerp(54f, 15f, immediateDisintegrationInterpolant));
        beamShader.TrySetParameter("redEdgeThreshold", 0.6f);

        List<Vector2> laserPoints = Projectile.GetLaserControlPoints(12, 2500f, -Vector2.UnitY);
        PrimitiveSettings settings = new PrimitiveSettings(BeamWidthFunction, BeamColorFunction, Shader: beamShader, Pixelate: false);
        PrimitiveRenderer.RenderTrail(laserPoints, settings, 200);
    }

    private void RenderPlayerDisintegrationEffect(float immediateDisintegrationInterpolant)
    {
        PlayerPostProcessingShaderSystem.ApplyPostProcessingEffect(Owner, new(player =>
        {
            ManagedShader animeShader = ShaderManager.GetShader("NoxusBoss.AnimeObliterationShader");
            animeShader.TrySetParameter("scatterDirectionBias", Vector2.UnitY * Pow(immediateDisintegrationInterpolant, 2f) * 200f);
            animeShader.TrySetParameter("pixelationFactor", Vector2.One * 1.5f / Main.ScreenSize.ToVector2());
            animeShader.TrySetParameter("disintegrationFactor", Pow(immediateDisintegrationInterpolant, 1.6f) * 2f);
            animeShader.TrySetParameter("opacity", InverseLerp(0.75f, 0.5f, DisintegrationInterpolant));
            animeShader.TrySetParameter("silhouetteInterpolant", 1f);
            animeShader.SetTexture(WavyBlotchNoise, 1);
            animeShader.Apply();
        }, false));

        ManagedRenderTarget? texture = PlayerPostProcessingShaderSystem.FinalPlayerTargets[Owner.whoAmI];
        if (texture is null)
            return;

        Vector2 scale = new Vector2(1f, 1f + DisintegrationInterpolant * 5f);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * (14.5f - Owner.gfxOffY - Owner.height + 2f + texture.Height * scale.Y * 0.5f);
        Main.spriteBatch.Draw(texture, drawPosition, null, Color.Black, 0f, texture.Size() * new Vector2(0.5f, 1f), scale, 0, 0f);
    }
}
