using Luminance.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.ScreenShake;

public class CustomScreenShakeSystem : ModSystem
{
    /// <summary>
    /// The set of all active screen shakes affecting the screen.
    /// </summary>
    internal static readonly List<ShakeData> ActiveShakes = [];

    /// <summary>
    /// The current intensity of all active shakes.
    /// </summary>
    public static float CurrentShakeIntensity
    {
        get;
        private set;
    }

    public override void ModifyScreenPosition()
    {
        CurrentShakeIntensity = 0f;

        float globalScreenShakeModifier = ModContent.GetInstance<Config>().ScreenshakeModifier * 0.01f;
        Vector2 screenShakeOffset = Vector2.Zero;
        foreach (ShakeData shake in ActiveShakes)
        {
            // Update thie screen shake, incrementing its life timer.
            // This makes screen shakes dissipate during render time rather than update time, to ensure that screen shake doesn't vibrate endlessly
            // if the player pauses the game.
            shake.Update();

            screenShakeOffset += Main.rand.NextVector2Unit() * shake.ShakeIntensity * Main.rand.NextGaussian() * shake.DirectionalBias * globalScreenShakeModifier;
            CurrentShakeIntensity += shake.ShakeIntensity;
        }

        Main.screenPosition += screenShakeOffset;

        // Remove all defunct shakes.
        ActiveShakes.RemoveAll(s => s.Time >= s.Lifetime);
    }

    /// <summary>
    /// Creates a new generic screen shake instance and adds to the set of all active screen shakes.
    /// </summary>
    /// <param name="duration">How long the shake should last for, in frames.</param>
    /// <param name="shakeIntensity">The intensity of the screen shake.</param>
    public static ShakeData Start(int duration, float shakeIntensity)
    {
        ShakeData shake = new ShakeData()
        {
            Lifetime = duration,
            MaxShake = shakeIntensity
        };

        ActiveShakes.Add(shake);

        return shake;
    }
}
