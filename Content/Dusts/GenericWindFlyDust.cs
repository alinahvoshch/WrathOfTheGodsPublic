using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Dusts;

public class GenericWindFlyDust : ModDust
{
    public override string Texture => "NoxusBoss/Assets/Textures/Content/Dust/GenericWindFlyDust";

    public override void OnSpawn(Dust dust) { }

    public override bool Update(Dust dust)
    {
        dust.position += dust.velocity;
        dust.scale *= 0.985f;
        if (dust.velocity.Length() <= 0.2f)
        {
            dust.velocity *= 0.2f;
            dust.scale *= 0.91f;
            dust.color *= 0.8f;
        }
        else
            dust.position.Y += Sin(dust.position.X * 0.0053f + dust.position.Y * 0.0037f + dust.dustIndex) * 2.4f;

        if (Collision.SolidCollision(dust.position - Vector2.One * 2f, 4, 4) || dust.scale <= 0.3f)
            dust.active = false;

        return false;
    }

    public override Color? GetAlpha(Dust dust, Color lightColor) => dust.color;
}
