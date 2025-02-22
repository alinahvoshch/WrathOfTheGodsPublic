using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Dusts;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class Snowflake : ModProjectile, IProjOwnedByBoss<AvatarOfEmptiness>
{
    public float RotationX;

    public float RotationY;

    public float RotationZ;

    public bool AdheresToWind => Projectile.ai[0] == 0f;

    public ref float Time => ref Projectile.ai[1];

    public override string Texture => GetAssetPath("Content/NPCs/Bosses/Avatar/Projectiles", Name);

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 360;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        if (AvatarOfEmptiness.Myself is null || AvatarOfEmptinessSky.Dimension != AvatarDimensionVariants.CryonicDimension)
        {
            Projectile.Kill();
            return;
        }

        RotationX += Projectile.velocity.Length() * 0.005f;
        RotationZ -= Projectile.velocity.Length() * 0.02f;

        Vector2 windPosition = Projectile.Center * new Vector2(0.0029f, 0.0016f) + Vector2.UnitX * (float)Main.time * 0.02f;
        Vector3 windField = new Vector3(Cos(windPosition.X * 1.2f - windPosition.Y * 1.9f), -Cos(windPosition.X * 2.8f + windPosition.Y * 1.05f), Cos01(windPosition.X * 1.3f + windPosition.Y * 1.76f));
        Vector2 windForce = new Vector2(windField.X, windField.Y * 0.15f) * 0.2f;
        windForce = Vector2.Lerp(windForce, Projectile.SafeDirectionTo(AvatarOfEmptiness.Myself.Center) * 0.2f, AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().LilyFreezeInterpolant);

        if (AdheresToWind)
            Projectile.velocity += windForce;

        Projectile.Opacity = InverseLerp(0f, 15f, Time) * InverseLerp(0f, 60f, Projectile.timeLeft);

        if (Main.rand.NextBool(3))
        {
            Color mistColor = Color.Lerp(new(145, 187, 255), Color.LightCyan, Main.rand.NextFloat()) * Projectile.Opacity * 0.08f;
            LargeMistParticle iceMist = new LargeMistParticle(Projectile.Center, Main.rand.NextVector2Circular(7f, 7f), mistColor, Projectile.scale * 8f, 0f, Main.rand.Next(75, 105), 0.01f, true);
            iceMist.Spawn();

            Dust twinkle = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(40f, 40f), ModContent.DustType<TwinkleDust>(), Vector2.Zero);
            twinkle.scale = Main.rand.NextFloat(0.4f);
            twinkle.color = Color.Lerp(Color.White, Color.DeepSkyBlue, Main.rand.NextFloat(0.4f)) * 0.7f;
            twinkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
            twinkle.customData = 0;
        }

        Projectile.scale = Lerp(0.04f, 0.206f, Projectile.identity / 17f % 1f);
        Projectile.scale *= 1f / (windField.Z * 1.2f + 1f);
        Projectile.Opacity *= Utils.Remap(Projectile.scale, 0.04f, 0.16f, 1f, 0.45f);
        Projectile.Opacity *= Utils.Remap(Projectile.velocity.X, -2f, 2f, 1f, 0.3f);

        Time++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Matrix rotation = Matrix.CreateRotationX(RotationX) * Matrix.CreateRotationY(-RotationX) * Matrix.CreateRotationZ(RotationZ);
        Vector3 lightDirection = Vector3.UnitZ;
        Vector3 snowflakeDirection = Vector3.Transform(Vector3.UnitZ, rotation);
        float lightDot = Vector3.Dot(lightDirection, snowflakeDirection);
        float glimmerInterpolant = InverseLerp(0.4f, 0.93f, Abs(lightDot));

        Vector2 lightSource = Main.dayTime ? SunMoonPositionRecorder.SunPosition : SunMoonPositionRecorder.MoonPosition;
        Vector2 directionToLightSource = Vector2.Transform((lightSource - Projectile.Center + Main.screenPosition).SafeNormalize(Vector2.Zero), Matrix.Invert(rotation));

        Texture2D snowflakeTexture = TextureAssets.Projectile[Type].Value;
        ManagedShader snowflakeShader = ShaderManager.GetShader("NoxusBoss.SnowflakeShader");
        snowflakeShader.TrySetParameter("direction", directionToLightSource);
        snowflakeShader.TrySetParameter("glimmerInterpolant", glimmerInterpolant);
        snowflakeShader.SetTexture(snowflakeTexture, 0, SamplerState.LinearClamp);

        Vector2 size = snowflakeTexture.Size() * Projectile.scale;
        Vector2 drawOffset = Vector2.Transform(Vector2.One * size * new Vector2(-0.5f, 0.5f), rotation);
        PrimitiveRenderer.RenderQuad(snowflakeTexture, Projectile.Center + drawOffset, Projectile.scale, 0f, new Color(215, 255, 255, 0) * Projectile.Opacity, snowflakeShader, Quaternion.CreateFromRotationMatrix(rotation));

        return false;
    }
}
