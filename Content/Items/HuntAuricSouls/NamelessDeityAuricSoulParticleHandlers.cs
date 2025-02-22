using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.Blossoms;
using NoxusBoss.Core.Graphics.FastParticleSystems;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.HuntAuricSouls;

[Autoload(Side = ModSide.Client)]
public class NamelessDeityAuricSoulParticleHandlers : ModSystem
{
    /// <summary>
    /// The particle system responsible for the rendering of leaves.
    /// </summary>
    public static LeafParticleSystem LeafParticleSystem
    {
        get;
        private set;
    }

    /// <summary>
    /// The particle system responsible for the rendering of blososms.
    /// </summary>
    public static BlossomParticleSystem BlossomParticleSystem
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        LeafParticleSystem = new LeafParticleSystem(8192, PrepareLeafParticleRendering, ExtraParticleUpdates);
        BlossomParticleSystem = new BlossomParticleSystem(8192, PrepareBlossomParticleRendering, ExtraParticleUpdates);
        On_Main.DrawRain += RenderLeavesAndBlossoms;
    }

    public override void PostUpdateDusts()
    {
        LeafParticleSystem.UpdateAll();
        BlossomParticleSystem.UpdateAll();
    }

    private void RenderLeavesAndBlossoms(On_Main.orig_DrawRain orig, Main self)
    {
        orig(self);

        if (LeafParticleSystem.particles.Any(p => p.Active))
            LeafParticleSystem.RenderAll();
        if (BlossomParticleSystem.particles.Any(p => p.Active))
            BlossomParticleSystem.RenderAll();
    }

    private static void PrepareLeafParticleRendering()
    {
        Matrix world = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f);
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -100f, 100f);

        Texture2D leaf = LeafVisualsSystem.LeafTexture.Value;
        ManagedShader overlayShader = ShaderManager.GetShader("NoxusBoss.BasicPrimitiveOverlayShader");
        overlayShader.TrySetParameter("uWorldViewProjection", world * Main.GameViewMatrix.TransformationMatrix * projection);
        overlayShader.SetTexture(leaf, 1, SamplerState.LinearClamp);
        overlayShader.Apply();
    }

    private static void PrepareBlossomParticleRendering()
    {
        Matrix world = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f);
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -100f, 100f);

        ManagedShader overlayShader = ShaderManager.GetShader("NoxusBoss.BasicPrimitiveOverlayShader");
        overlayShader.TrySetParameter("uWorldViewProjection", world * Main.GameViewMatrix.TransformationMatrix * projection);
        overlayShader.SetTexture(GennedAssets.Textures.Particles.Blossom.Value, 1, SamplerState.LinearClamp);
        overlayShader.Apply();
    }

    private static void ExtraParticleUpdates(ref FastParticle particle)
    {
        float wave = TwoPi / 240f;
        particle.Velocity = particle.Velocity.RotatedBy(wave) * 0.986f;
        particle.Rotation = particle.Velocity.ToRotation() - PiOver2;

        float opacity = InverseLerpBump(16f, 40f, 140f, 180f, particle.Time);
        particle.Color = Color.White * opacity;

        if (Collision.SolidCollision(particle.Position, 1, 1))
        {
            particle.Velocity *= 0.9f;
            particle.Time += 3;
        }
        if (particle.Time >= 180)
            particle.Active = false;
    }
}
