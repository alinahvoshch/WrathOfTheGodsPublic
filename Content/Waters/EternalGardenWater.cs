using Microsoft.Xna.Framework;
using NoxusBoss.Core.Configuration;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Waters;

public class EternalGardenWater : ModWaterStyle
{
    public static bool FancyWaterEnabled => WoTGConfig.Instance.VisualOverlayIntensity >= 0.4f && !ModLoader.HasMod("Nitrate");

    public static bool DrewWaterThisFrame
    {
        get;
        set;
    }

    public override string Texture => GetAssetPath("Content/Waters", Name);

    public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("NoxusBoss/EternalGardenWaterflow").Slot;

    public override int GetSplashDust() => 33;

    public override int GetDropletGore() => 713;

    public override Color BiomeHairColor() => Color.ForestGreen;

    public override void LightColorMultiplier(ref float r, ref float g, ref float b)
    {
        r = 1.06f;
        g = 1.071f;
        b = 1.075f;
    }
}
