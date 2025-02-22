using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Dusts;

public class TwinkleDust : ModDust
{
    public override string Texture => "NoxusBoss/Assets/Textures/Content/Dust/TwinkleDust";

    public override void OnSpawn(Dust dust) { }

    public override bool Update(Dust dust)
    {
        if (dust.customData is not int)
            dust.customData = 0;

        int time = (int)dust.customData;

        dust.position += dust.velocity;
        dust.velocity *= 0.97f;
        dust.scale *= Lerp(0.93f, 1.04f, Cos01(time / 11f + dust.dustIndex));
        dust.customData = (int)dust.customData + 1;

        if (time >= 90 && dust.scale <= 0.09f)
            dust.active = false;

        return false;
    }

    public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(255, 255, 255, dust.alpha).MultiplyRGBA(dust.color) * InverseLerp(0.2f, 0.5f, dust.scale);
}
