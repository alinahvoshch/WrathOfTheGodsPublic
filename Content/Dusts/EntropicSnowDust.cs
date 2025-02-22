using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Dusts;

public class EntropicSnowDust : ModDust
{
    public override string Texture => "NoxusBoss/Assets/Textures/Content/Dust/EntropicSnowDust";

    public override void OnSpawn(Dust dust) { }

    public override bool Update(Dust dust)
    {
        if (AvatarRift.Myself is not null && AvatarRift.Myself.As<AvatarRift>().SuckOpacity >= 0.01f)
        {
            if (dust.velocity.Length() <= 24f)
                dust.velocity += dust.position.SafeDirectionTo(AvatarRift.Myself.Center) * AvatarRift.Myself.As<AvatarRift>().SuckOpacity * 0.54f;
            else
                dust.velocity = dust.velocity.RotateTowards(dust.position.AngleTo(AvatarRift.Myself.Center), 0.067f);

            if (dust.position.WithinRange(AvatarRift.Myself.Center, 100f))
                dust.active = false;
        }
        if (dust.customData is not int)
            dust.customData = 0;

        dust.position += dust.velocity;
        dust.scale *= 0.99f;
        if (dust.velocity.Length() <= 0.2f)
        {
            dust.velocity *= 0.2f;
            dust.scale *= 0.95f;
            dust.color *= 0.8f;
        }
        else
            dust.position.Y += Cos(dust.position.X * 0.0048f + dust.position.Y * 0.0039f) * 2f;

        dust.customData = (int)dust.customData + 1;

        if (Collision.SolidCollision(dust.position - Vector2.One * 2f, 4, 4) || (int)dust.customData >= 180)
            dust.active = false;

        return false;
    }

    public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(98, 98, 98, dust.alpha).MultiplyRGBA(dust.color);
}
