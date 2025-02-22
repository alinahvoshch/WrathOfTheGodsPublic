using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.Configuration;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.SpecificEffectManagers;

[Autoload(Side = ModSide.Client)]
public class RadialScreenShoveSystem : ModSystem
{
    public static Vector2 DistortionCenter
    {
        get;
        set;
    }

    public static float DistortionPower
    {
        get;
        set;
    }

    public static int DistortionTimer
    {
        get;
        set;
    }

    public static int DistortionLifetime
    {
        get;
        set;
    }

    public static float DistortionCompletionRatio => DistortionTimer / (float)DistortionLifetime;

    public override void PostUpdateProjectiles()
    {
        // Increment the distortion timer if it's active. Once its reaches its natural maximum the effect ceases.
        if (DistortionTimer >= 1)
        {
            DistortionTimer++;
            if (DistortionTimer >= DistortionLifetime)
                DistortionTimer = 0;
        }

        DistortionPower = Convert01To010(DistortionCompletionRatio) * (1f - DistortionCompletionRatio);

        if (DistortionTimer <= 0)
            return;

        float distortionPowerForShader = DistortionPower * WoTGConfig.Instance.VisualOverlayIntensity * 0.11f;
        if (Main.gamePaused)
            distortionPowerForShader = 0f;

        ManagedScreenFilter shoveShader = ShaderManager.GetFilter("NoxusBoss.RadialScreenShoveShader");
        shoveShader.TrySetParameter("blurPower", WoTGConfig.Instance.VisualOverlayIntensity * 0.5f);
        shoveShader.TrySetParameter("pulseTimer", Main.GlobalTimeWrappedHourly * 21f);
        shoveShader.TrySetParameter("distortionPower", distortionPowerForShader);
        shoveShader.TrySetParameter("distortionCenter", WorldSpaceToScreenUV(DistortionCenter));
        shoveShader.Activate();
    }

    public static void Start(Vector2 distortionCenter, int distortionTime)
    {
        if (WoTGConfig.Instance.PhotosensitivityMode)
            return;

        DistortionCenter = distortionCenter;
        DistortionTimer = 1;
        DistortionLifetime = distortionTime;
    }
}
