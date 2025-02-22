using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class RedirectingRubble : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<AvatarOfEmptiness>
{
    public enum RubbleAIState
    {
        FlyUpward,
        HoverInPlace,
        AccelerateForward
    }

    /// <summary>
    /// The state of this rubble.
    /// </summary>
    public RubbleAIState CurrentState
    {
        get => (RubbleAIState)Projectile.ai[0];
        set => Projectile.ai[0] = (int)value;
    }

    /// <summary>
    /// The intensity of telekinesis effects on this rubble.
    /// </summary>
    public float TelekinesisInterpolant
    {
        get
        {
            if (CurrentState != RubbleAIState.AccelerateForward)
                return 0f;

            return InverseLerp(13.5f, 17f, Projectile.velocity.Length());
        }
    }

    /// <summary>
    /// Whether this rubble is big or not.
    /// </summary>
    public bool Big
    {
        get => Projectile.ai[2] == 1f;
        set => Projectile.ai[2] = value.ToInt();
    }

    /// <summary>
    /// How long this rubble has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[1];

    public override string Texture => GetAssetPath("Content/NPCs/Bosses/Avatar/Projectiles", Name);

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 3;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
    }

    public override void SetDefaults()
    {
        Projectile.width = 46;
        Projectile.height = 46;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 900;
        Projectile.scale = Main.rand?.NextFloat(0.8f, 1.2f) ?? 1f;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        if (Time == 1f && CurrentState == RubbleAIState.FlyUpward)
        {
            Projectile.frame = Main.rand.NextFromList(0, 2);
            Tile ground = Framing.GetTileSafely(FindGroundVertical(Projectile.Bottom.ToTileCoordinates()));

            if (ground.TileType == TileID.SnowBlock || ground.TileType == TileID.IceBlock)
                Projectile.frame = 1;

            Big = Main.rand.NextBool(5);
            Projectile.netUpdate = true;
        }

        if (Big)
        {
            if (Projectile.frame == 1f)
                Projectile.scale = Lerp(0.9f, 1.1f, Projectile.identity / 11f % 1f) * 1.36f;

            Projectile.Resize(68, 68);
        }
        else
            Projectile.Resize(46, 46);

        switch (CurrentState)
        {
            case RubbleAIState.FlyUpward:
                DoBehavior_FlyUpward();
                break;
            case RubbleAIState.HoverInPlace:
                DoBehavior_HoverInPlace();
                break;
            case RubbleAIState.AccelerateForward:
                DoBehavior_AccelerateForward();
                break;
        }

        Projectile.rotation += Projectile.velocity.X * 0.912f / Projectile.scale / Projectile.width;
        Time++;
    }

    public void DoBehavior_FlyUpward()
    {
        float maxFlySpeed = Lerp(10f, 47f, Projectile.identity / 13f % 1f);
        float currentFlySpeed = Sqrt(InverseLerp(25f, 0f, Time)) * maxFlySpeed;
        Projectile.velocity = Vector2.UnitY * -currentFlySpeed;
        Projectile.rotation += Projectile.velocity.Y * 0.014f;

        if (currentFlySpeed <= 0f)
        {
            CurrentState = RubbleAIState.HoverInPlace;
            Time = 0f;
            Projectile.netUpdate = true;
        }
    }

    public void DoBehavior_HoverInPlace()
    {
        Vector2 idealSinusoidalVelocity = Vector2.UnitY * Cos(Projectile.identity + Time / 12f) * 5f;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealSinusoidalVelocity, 0.2f);
    }

    public void DoBehavior_AccelerateForward()
    {
        float flyAcceleration = 1f + 0.48f / Projectile.scale / Projectile.width;
        if (Projectile.velocity.Length() <= 12f)
            Projectile.velocity *= 1.02f;
        Projectile.velocity = (Projectile.velocity * flyAcceleration).ClampLength(0f, 51f);
        Projectile.tileCollide = Projectile.velocity.Length() >= 31f;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        SoundEngine.PlaySound(SoundID.Item50, Projectile.Center);

        // Create dust.
        for (int i = 0; i < 25; i++)
        {
            int dustID = DustID.Stone;

            switch (Projectile.frame)
            {
                case 0:
                    dustID = DustID.Dirt;
                    break;
                case 2:
                    dustID = DustID.Ice;
                    break;
            }

            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f), dustID);
            dust.velocity = -Vector2.UnitY.RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 10f);
            dust.scale = Main.rand.NextFloat(0.75f, 2.1f);
            dust.noGravity = dust.velocity.Length() >= 7f;
        }

        return true;
    }

    public override bool? CanDamage() => Projectile.velocity.Length() >= 15f;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frame = texture.Frame(Main.projFrames[Type], 2, Projectile.frame, Projectile.frame == 1 ? 0 : Big.ToInt());
        Vector2 origin = frame.Size() * 0.5f;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        for (int i = 0; i < 20; i++)
        {
            Vector2 drawOffset = (TwoPi * i / 20f).ToRotationVector2() * TelekinesisInterpolant * 2f;
            Main.spriteBatch.Draw(texture, drawPosition + drawOffset, frame, Projectile.GetAlpha(Color.White with { A = 0 }), Projectile.rotation, origin, Projectile.scale, 0, 0f);
        }
        Main.spriteBatch.Draw(texture, drawPosition, frame, Projectile.GetAlpha(Color.White), Projectile.rotation, origin, Projectile.scale, 0, 0f);

        return false;
    }

    public float TelekinesisWidthFunction(float completionRatio)
    {
        float tipSmoothenFactor = Pow(InverseLerp(0f, 0.2f, completionRatio), 0.5f);
        float endSquishFactor = Utils.Remap(completionRatio, 0.3f, 1f, 1f, 0.1f);
        return Projectile.scale * tipSmoothenFactor * endSquishFactor * (Big ? 40f : 28f);
    }

    public Color TelekinesisColorFunction(float completionRatio)
    {
        float opacity = TelekinesisInterpolant;
        bool blue = Projectile.identity % 2 == 0 || Projectile.frame == 1;

        return Projectile.GetAlpha(blue ? Color.DeepSkyBlue : new Color(1f, 0.02f, 0.19f)) * opacity;
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        if (CurrentState == RubbleAIState.FlyUpward)
            return;

        ManagedShader telekinesisShader = ShaderManager.GetShader("NoxusBoss.RubbleTelekinesisTrailShader");
        telekinesisShader.TrySetParameter("noiseOffset", Projectile.identity * 11.61f);
        telekinesisShader.SetTexture(MoltenNoise, 1, SamplerState.PointWrap);

        Vector2 forwardOffset = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.scale * 28f;
        if (Big)
            forwardOffset *= 1.4f;

        PrimitiveSettings settings = new PrimitiveSettings(TelekinesisWidthFunction, TelekinesisColorFunction, _ => Projectile.Size * 0.5f + forwardOffset, Pixelate: true, Shader: telekinesisShader);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos.Take(6).ToArray(), settings, 26);
    }
}
