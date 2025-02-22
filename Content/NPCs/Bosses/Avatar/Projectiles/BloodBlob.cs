using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class BloodBlob : ModProjectile, IProjOwnedByBoss<AvatarOfEmptiness>, IPixelatedPrimitiveRenderer
{
    public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.AfterProjectiles;

    /// <summary>
    /// How long this blob has existed for, in frames.
    /// </summary>
    public int Time
    {
        get;
        set;
    }

    /// <summary>
    /// Whether this blob is unaffected by gravity or not.
    /// </summary>
    public bool GravityUnaffected
    {
        get;
        set;
    }

    /// <summary>
    /// Whether this blob can do damage when moving upward or not.
    /// </summary>
    public bool CanDoDamageWhenMovingUp
    {
        get => Projectile.ai[0] == 1f;
        set => Projectile.ai[0] = value.ToInt();
    }

    /// <summary>
    /// How much the max fall speed of this blob should be boosted.
    /// </summary>
    public ref float MaxFallSpeedBoost => ref Projectile.ai[1];

    /// <summary>
    /// How much the acceleration of this blob should be boosted.
    /// </summary>
    public ref float AccelerationBoost => ref Projectile.ai[2];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
    }

    public override void SetDefaults()
    {
        Projectile.width = Main.rand?.Next(32, 93) ?? 48;
        if (AvatarOfEmptiness.Myself is not null && AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().CurrentState == AvatarOfEmptiness.AvatarAIType.BloodiedFountainBlasts_DenseBurst)
            Projectile.width /= 2;

        Projectile.height = Projectile.width;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.hide = true;
        Projectile.timeLeft = 240;

        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(Time);
        writer.Write(GravityUnaffected);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Time = reader.ReadInt32();
        GravityUnaffected = reader.ReadBoolean();
    }

    public override void AI()
    {
        if (!GravityUnaffected)
        {
            Projectile.velocity.X *= 1.005f;

            if (Abs(Projectile.velocity.X) >= 6f || Time >= 120)
                Projectile.velocity.Y += InverseLerp(120f, 240f, Time).Squared() * 0.275f + 0.06f + AccelerationBoost;

            float sizeInterpolant = InverseLerp(32f, 190f, Projectile.width);
            float maxFallSpeed = Lerp(24f, 17f, sizeInterpolant) + Time * 0.19f + MaxFallSpeedBoost;
            if (Projectile.velocity.Y > maxFallSpeed)
                Projectile.velocity.Y = maxFallSpeed;
        }

        Time++;
    }

    public override bool? CanDamage() => CanDoDamageWhenMovingUp || Projectile.velocity.Y >= 0f;

    public float BloodWidthFunction(float completionRatio)
    {
        float baseWidth = Projectile.width * 0.66f;
        float smoothTipCutoff = SmoothStep(0f, 1f, InverseLerp(0.09f, 0.3f, completionRatio));
        return smoothTipCutoff * baseWidth;
    }

    public Color BloodColorFunction(float completionRatio)
    {
        return Projectile.GetAlpha(new Color(82, 1, 23));
    }

    public override bool PreDraw(ref Color lightColor)
    {
        float scaleFactor = Projectile.width / 46f;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition + Projectile.velocity * -0.85f;
        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.DarkRed) with { A = 0 } * 0.2f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 1.2f, 0, 0f);
        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.Red) with { A = 0 } * 0.4f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 0.64f, 0, 0f);
        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.Orange) with { A = 0 } * 0.4f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 0.3f, 0, 0f);
        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        Rectangle viewBox = Projectile.Hitbox;
        Rectangle screenBox = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
        viewBox.Inflate(540, 540);
        if (!viewBox.Intersects(screenBox))
            return;

        float lifetimeRatio = Time / 240f;
        float dissolveThreshold = InverseLerp(0.67f, 1f, lifetimeRatio) * 0.5f;
        ManagedShader bloodShader = ShaderManager.GetShader("NoxusBoss.BloodBlobShader");
        bloodShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly + Projectile.identity * 72.113f);
        bloodShader.TrySetParameter("dissolveThreshold", dissolveThreshold);
        bloodShader.TrySetParameter("accentColor", new Vector4(0.6f, 0.02f, -0.1f, 0f));
        bloodShader.SetTexture(BubblyNoise, 1, SamplerState.LinearWrap);
        bloodShader.SetTexture(DendriticNoiseZoomedOut, 2, SamplerState.LinearWrap);

        PrimitiveSettings settings = new PrimitiveSettings(BloodWidthFunction, BloodColorFunction, _ => Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.56f, Pixelate: true, Shader: bloodShader);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 9);
    }
}
