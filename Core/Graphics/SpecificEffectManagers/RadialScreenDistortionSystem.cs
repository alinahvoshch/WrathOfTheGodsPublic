using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.SpecificEffectManagers;

[Autoload(Side = ModSide.Client)]
public class RadialScreenDistortionSystem : ModSystem
{
    public struct ScreenDistortion
    {
        public Vector2 Position;

        public float LifetimeRatio;

        public float MaxRadius;
    }

    /// <summary>
    /// The set of all active distortion effects that have been performed.
    /// </summary>
    public static ScreenDistortion[] Distortions
    {
        get;
        private set;
    } = new ScreenDistortion[15];

    public override void PostUpdatePlayers()
    {
        for (int i = 0; i < Distortions.Length; i++)
            Distortions[i].LifetimeRatio += 0.064f;
    }

    /// <summary>
    /// Attempts to create a new distortion effect at a given world position.
    /// </summary>
    /// <param name="position">The world position of the distortion effect.</param>
    /// <param name="maxRadius">The maximum radius of the distortion effect.</param>
    public static void CreateDistortion(Vector2 position, float maxRadius)
    {
        int freeIndex = -1;
        for (int i = 0; i < Distortions.Length; i++)
        {
            if (Distortions[i].LifetimeRatio >= 1f)
                freeIndex = i;
        }

        if (freeIndex >= 0)
        {
            Distortions[freeIndex] = new ScreenDistortion()
            {
                Position = position,
                MaxRadius = maxRadius
            };
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        bool anyInUse = false;
        float[] lifetimeRatios = new float[Distortions.Length];
        float[] maxRadii = new float[Distortions.Length];
        Vector2[] positions = new Vector2[Distortions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            lifetimeRatios[i] = Distortions[i].LifetimeRatio;
            maxRadii[i] = Distortions[i].MaxRadius * Main.GameViewMatrix.Zoom.X;
            positions[i] = Vector2.Transform(Distortions[i].Position - Main.screenPosition, Main.GameViewMatrix.TransformationMatrix);

            if (lifetimeRatios[i] > 0f && lifetimeRatios[i] < 1f)
                anyInUse = true;
        }

        if (!anyInUse)
            return;

        ManagedScreenFilter distortionShader = ShaderManager.GetFilter("NoxusBoss.LocalScreenDistortionShader");
        distortionShader.TrySetParameter("lifetimeRatios", lifetimeRatios);
        distortionShader.TrySetParameter("maxRadii", maxRadii);
        distortionShader.TrySetParameter("positions", positions);
        distortionShader.SetTexture(GennedAssets.Textures.Extra.Psychedelic, 1, SamplerState.LinearWrap);
        distortionShader.Activate();
    }
}
