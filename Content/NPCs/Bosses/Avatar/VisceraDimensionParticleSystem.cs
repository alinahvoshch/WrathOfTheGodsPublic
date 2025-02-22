using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using ReLogic.Threading;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar;

public static class VisceraDimensionParticleSystem
{
    public struct SplatterParticle
    {
        public int Time;

        public int Lifetime;

        public bool Active;

        public Color DrawColor;

        public Vector2 Position;

        public Vector2 Velocity;
    }

    private static int nextParticleIndex;

    /// <summary>
    /// The set of all blood splatter particles.
    /// </summary>
    public static readonly SplatterParticle[] Particles = new SplatterParticle[ParticleCount];

    /// <summary>
    /// The maximum amount of blood splatter particles that can be in use at once.
    /// </summary>
    public const int ParticleCount = 1024;

    /// <summary>
    /// Creates a new blood splatter particle.
    /// </summary>
    /// <param name="position">The particle's position.</param>
    /// <param name="velocity">The particle's velocity.</param>
    /// <param name="color">The particle's color.</param>
    /// <param name="lifetime">The particle's lifetime, in frames.</param>
    public static void CreateNew(Vector2 position, Vector2 velocity, Color color, int lifetime)
    {
        nextParticleIndex = (nextParticleIndex + 1) % ParticleCount;

        ref SplatterParticle particleRef = ref Particles[nextParticleIndex];
        particleRef.Active = true;
        particleRef.Time = 0;
        particleRef.Lifetime = lifetime;
        particleRef.Position = position;
        particleRef.DrawColor = color;
        particleRef.Velocity = velocity;
    }

    /// <summary>
    /// Update all active particles.
    /// </summary>
    public static void Update()
    {
        if (Main.gamePaused)
            return;

        FastParallel.For(0, ParticleCount, (from, to, _) =>
        {
            for (int i = from; i < to; i++)
            {
                if (!Particles[i].Active)
                    continue;

                Particles[i].Time++;
                Particles[i].Velocity.X *= 1.04f;
                Particles[i].Velocity.Y *= 0.97f;
                if (Particles[i].Velocity.Y < 5f)
                    Particles[i].Velocity.Y += 0.4f;

                Particles[i].Position += Particles[i].Velocity * InverseLerp(-1f, 6f, Particles[i].Time);

                if (Particles[i].Time >= Particles[i].Lifetime)
                    Particles[i].Active = false;
            }
        });
    }

    /// <summary>
    /// Renders all active particles.
    /// </summary>
    public static void Render()
    {
        Main.spriteBatch.PrepareForShaders(BlendState.NonPremultiplied);

        ManagedShader dissolveShader = ShaderManager.GetShader("NoxusBoss.DissolveShader");

        Texture2D splatter = GennedAssets.Textures.AvatarOfEmptiness.BloodSplatterParticle;
        for (int i = 0; i < ParticleCount; i++)
        {
            if (!Particles[i].Active)
                continue;

            SplatterParticle particle = Particles[i];
            float scaleFactor = InverseLerp(0f, 2f, particle.Time) * Lerp(0.3f, 0.5f, i / 19f % 1f) + particle.Time * 0.012f;
            Rectangle frame = splatter.Frame(1, 3, 0, i % 3);
            Color color = particle.DrawColor * InverseLerp(0f, 3.5f, particle.Time);
            color = color.MultiplyRGB(Color.White * Utils.Remap(particle.Time, 2f, 8f, 0.35f, 1f));

            dissolveShader.TrySetParameter("dissolveIntensity", Pow(particle.Time / (float)particle.Lifetime, 1.6f));
            dissolveShader.TrySetParameter("distortCoordOffset", i * 0.167f);
            dissolveShader.TrySetParameter("distortOffsetFactor", 0.8f);
            dissolveShader.TrySetParameter("imageSize", splatter.Size());
            dissolveShader.TrySetParameter("frame", new Vector4(frame.X, frame.Y, frame.Width, frame.Height));
            dissolveShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
            dissolveShader.Apply();

            Vector2 drawPosition = particle.Position;

            Main.spriteBatch.Draw(splatter, drawPosition, frame, color, particle.Velocity.ToRotation(), frame.Size() * 0.5f, scaleFactor * new Vector2(1.8f, 1f), 0, 0f);
        }

        Main.spriteBatch.PrepareForShaders();
    }
}
