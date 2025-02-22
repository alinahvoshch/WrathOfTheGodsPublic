using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.FastParticleSystems;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;

[Autoload(Side = ModSide.Client)]
public class NamelessFireParticleSystemManager : ModSystem
{
    /// <summary>
    /// The particle system used to render Nameless' fire.
    /// </summary>
    public static FireParticleSystem ParticleSystem
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        ParticleSystem = new FireParticleSystem(GennedAssets.Textures.Particles.FireParticleA, 34, 1024, PrepareShader, UpdateParticle);
        On_Main.DrawDust += RenderParticles;
    }

    private static void PrepareShader()
    {
        Matrix world = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f);
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -100f, 100f);

        Main.instance.GraphicsDevice.BlendState = BlendState.Additive;

        ManagedShader overlayShader = ShaderManager.GetShader("NoxusBoss.FireParticleDissolveShader");
        overlayShader.TrySetParameter("pixelationLevel", 3000f);
        overlayShader.TrySetParameter("turbulence", 0.023f);
        overlayShader.TrySetParameter("screenPosition", Main.screenPosition);
        overlayShader.TrySetParameter("uWorldViewProjection", world * Main.GameViewMatrix.TransformationMatrix * projection);
        overlayShader.TrySetParameter("imageSize", GennedAssets.Textures.Particles.FireParticleA.Value.Size());
        overlayShader.TrySetParameter("initialGlowIntensity", 0.81f);
        overlayShader.TrySetParameter("initialGlowDuration", 0.285f);
        overlayShader.SetTexture(GennedAssets.Textures.Particles.FireParticleA, 1, SamplerState.LinearClamp);
        overlayShader.SetTexture(GennedAssets.Textures.Particles.FireParticleB, 2, SamplerState.LinearClamp);
        overlayShader.SetTexture(PerlinNoise, 3, SamplerState.LinearWrap);
        overlayShader.Apply();
    }

    private static void UpdateParticle(ref FastParticle particle)
    {
        float growthRate = 0.02f;
        particle.Size.X *= 1f + growthRate * 0.85f;
        particle.Size.Y *= 1f + growthRate;

        Vector3 hslVector = Main.rgbToHsl(particle.Color);
        Color lowValueColor = Main.hslToRgb(hslVector.X - 0.06f, hslVector.Y, 0.2f);
        particle.Color = Color.Lerp(particle.Color, lowValueColor, 0.07f);
        particle.Velocity *= 0.7f;
        particle.Rotation = particle.Velocity.ToRotation() + PiOver2;

        if (particle.Time >= ParticleSystem.ParticleLifetime + 15)
            particle.Active = false;
    }

    public override void PreUpdateEntities()
    {
        /*
        Vector2 mousePrevious = new Vector2(PlayerInput.MouseInfoOld.X, PlayerInput.MouseInfoOld.Y);
        Vector2 mouse = new Vector2(PlayerInput.MouseInfo.X, PlayerInput.MouseInfo.Y);
        Vector2 directionalForce = (mouse - mousePrevious) * 0.1f;
        float scaleFactor = Utils.Remap(mouse.Distance(mousePrevious), 30f, 65f, 1.2f, 0.79f);
        Color fireColor = new Color(255, 150, 3).HueShift(Main.rand.NextFloatDirection() * 0.036f);

        ParticleSystem.CreateNew(Main.MouseWorld, Main.rand.NextVector2Circular(7f, 7f) + directionalForce, Vector2.One * Main.rand.NextFloat(100f, 175f) * scaleFactor, fireColor);
        if (!mouse.WithinRange(mousePrevious, 40f))
        {
            int steps = (int)(mouse.Distance(mousePrevious) / 21f);
            for (float i = 0; i < steps; i++)
            {
                Vector2 position = Vector2.Transform(Vector2.Lerp(mousePrevious, mouse, i / steps), Matrix.Invert(Main.GameViewMatrix.TransformationMatrix)) + Main.screenPosition;
                ParticleSystem.CreateNew(position, Main.rand.NextVector2Circular(27f, 27f) + directionalForce * 0.45f, Vector2.One * Main.rand.NextFloat(100f, 175f) * scaleFactor, fireColor);
            }
        }
        */

        ParticleSystem.UpdateAll();
    }

    private static void RenderParticles(On_Main.orig_DrawDust orig, Main self)
    {
        orig(self);
        if (NamelessDeityBoss.Myself is null && ParticleSystem.particles.Any(p => p.Active))
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            ParticleSystem.RenderAll();
            Main.spriteBatch.End();
        }
    }
}
