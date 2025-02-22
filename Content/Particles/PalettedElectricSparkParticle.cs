using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.DataStructures;
using Terraria;

namespace NoxusBoss.Content.Particles
{
    public class PalettedElectricSparkParticle : Particle
    {
        /// <summary>
        /// The palette this spark interpolates between as it progresses through its life.
        /// </summary>
        public Palette Palette;

        /// <summary>
        /// The bloom texture.
        /// </summary>
        public static AtlasTexture BloomTexture
        {
            get;
            private set;
        }

        public override string AtlasTextureName => "NoxusBoss.MetalSparkParticle.png";

        public override BlendState BlendState => BlendState.Additive;

        public PalettedElectricSparkParticle(Vector2 position, Vector2 velocity, Palette palette, int lifetime, Vector2 scale)
        {
            Position = position;
            Velocity = velocity;
            Palette = palette;
            Scale = scale;
            Lifetime = lifetime;
            Opacity = 1f;
            Rotation = Velocity.ToRotation() + PiOver2;
        }

        public override void Update()
        {
            Opacity = Pow(1f - LifetimeRatio, 0.6f);
            Velocity.X *= 0.95f;
            Velocity.Y += 0.3f;
            Rotation = Velocity.ToRotation() + PiOver2;

            DrawColor = Palette.SampleColor(LifetimeRatio * 0.99f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            BloomTexture ??= AtlasManager.GetTexture("NoxusBoss.BasicMetaballCircle.png");
            spriteBatch.Draw(BloomTexture, Position - Main.screenPosition, null, DrawColor * Opacity, Rotation, null, Scale * new Vector2(3f, 5f), 0);

            base.Draw(spriteBatch);
        }
    }
}
