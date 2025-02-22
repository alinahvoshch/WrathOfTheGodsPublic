using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Dusts;

public class FlowerPieceDust : ModDust
{
    public override string Texture => "NoxusBoss/Assets/Textures/Content/Dust/FlowerPieceDust";

    public override void OnSpawn(Dust dust)
    {
        dust.noGravity = true;
        dust.noLight = true;
        dust.alpha = 50;
        dust.customData = 0;
    }

    public override bool Update(Dust dust)
    {
        dust.position += dust.velocity;
        dust.customData = (int)dust.customData + 2;
        if (Collision.SolidCollision(dust.position, 1, 1))
            dust.customData = (int)dust.customData + 7;

        if ((int)dust.customData >= 90)
        {
            dust.color *= 0.97f;
            dust.scale *= 0.96f;
        }
        if ((int)dust.customData >= 150)
            dust.active = false;

        return false;
    }

    public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(255, 255, 255, dust.alpha).MultiplyRGBA(dust.color) * InverseLerp(0f, 9f, (int)dust.customData);
}
