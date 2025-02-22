using System.ComponentModel;
using Luminance.Core;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace NoxusBoss.Core.Configuration;

[BackgroundColor(94, 30, 64, 216)]
[LegacyName("NoxusBossConfig")]
public class WoTGConfig : ModConfig
{
    public static WoTGConfig Instance => ModContent.GetInstance<WoTGConfig>();

    public override ConfigScope Mode => ConfigScope.ClientSide;

    private bool photosensitivityMode;

    [BackgroundColor(234, 46, 68, 192)]
    [DefaultValue(true)]
    public bool DisplayConfigMessage { get; set; }

    [BackgroundColor(234, 46, 68, 192)]
    [DefaultValue(false)]
    public bool PhotosensitivityMode
    {
        get => photosensitivityMode;
        set
        {
            photosensitivityMode = value;

            // Reset other config options too if this property was just enabled.
            if (photosensitivityMode)
            {
                ScreenShatterEffects = false;
                VisualOverlayIntensity = 0f;
                ModContent.GetInstance<Config>().ScreenshakeModifier = 0;
            }
        }
    }

    [BackgroundColor(234, 46, 68, 192)]
    [DefaultValue(true)]
    public bool ScreenShatterEffects { get; set; }

    [BackgroundColor(234, 46, 68, 192)]
    [DefaultValue(0.5f)]
    [Range(0f, 1f)]
    public float VisualOverlayIntensity { get; set; }

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => false;
}
