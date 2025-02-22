using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Waters;

public class RiftEclipseFogWater : ModWaterStyle
{
    public override string Texture => GetAssetPath("Content/Waters", Name);

    public override int ChooseWaterfallStyle()
    {
        return ModContent.Find<ModWaterfallStyle>($"{Mod.Name}/RiftEclipseFogWaterflow").Slot;
    }

    public override int GetSplashDust()
    {
        return 33;
    }

    public override int GetDropletGore()
    {
        return 713;
    }

    public override Color BiomeHairColor()
    {
        return Color.Lerp(Color.MediumPurple, Color.Black, 0.75f);
    }

    public override Asset<Texture2D> GetRainTexture() =>
        ModContent.Request<Texture2D>(GetAssetPath("Content/Waters", "RiftEclipseFogRain"));
}
