using Luminance.Common.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.Particles;
using NoxusBoss.Content.Particles.Metaballs;
using NoxusBoss.Core.Graphics.Automators;
using NoxusBoss.Core.Physics.VerletIntergration;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class DisgustingStar : ModProjectile, IProjOwnedByBoss<AvatarOfEmptiness>, IDrawSubtractive
{
    public bool SetActiveFalseInsteadOfKill => true;

    public VerletSimulatedRope DanglingRope
    {
        get;
        set;
    }

    public int BeatDelay
    {
        get;
        set;
    } = 67;

    public float FadeOutInterpolant => InverseLerp(0f, 11f, Projectile.timeLeft);

    public ref float DangleHorizontalOffset => ref Projectile.ai[0];

    public ref float RopeOffsetAngle => ref Projectile.ai[1];

    public bool DanglingFromTop
    {
        get => Projectile.localAI[2] == 0f;
        set => Projectile.localAI[2] = 1f - value.ToInt();
    }

    public bool HasFlownPastPlayer
    {
        get => Projectile.ai[2] == 1f;
        set => Projectile.ai[2] = value.ToInt();
    }

    public ref float Time => ref Projectile.localAI[0];

    public ref float DangleVerticalOffset => ref Projectile.localAI[1];

    public override string Texture => GetAssetPath("Content/NPCs/Bosses/Avatar/Projectiles", Name);

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 2;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 15000;
    }

    public override void SetDefaults()
    {
        Projectile.width = 104;
        Projectile.height = 104;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 210;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(Time);
        writer.Write(DangleVerticalOffset);
        writer.Write((byte)DanglingFromTop.ToInt());
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Time = reader.ReadSingle();
        DangleVerticalOffset = reader.ReadSingle();
        DanglingFromTop = reader.ReadByte() != 0;
    }

    public override void AI()
    {
        // Find the player to stick to based on which player is closest.
        // If the player is dead, that means no players are present and the star should die immediately.
        Player closestPlayer = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
        if (closestPlayer.dead)
        {
            Projectile.Kill();
            return;
        }

        // Decide which frame to use.
        Projectile.frame = Projectile.identity % Main.projFrames[Type];

        // Fade in.
        Projectile.Opacity = InverseLerp(45f, 90f, Time);
        Projectile.scale = Projectile.Opacity * (Lerp(0.6f, 0.9f, Cos01(TwoPi * Time / 10f + Projectile.identity * 0.3f)) + (Time - 120f) * 0.006f);

        // Fade out.
        Projectile.Opacity *= FadeOutInterpolant;
        Projectile.scale += (1f - FadeOutInterpolant) * 2f;

        // Pulsate.
        float pulseInterpolant = Sin01(TwoPi * Time / 54f);
        if (pulseInterpolant <= 0.5f)
            Projectile.rotation = PiOver4;
        else
            Projectile.rotation = 0f;
        Projectile.scale *= Pow(pulseInterpolant, 0.1f);

        // Dangle near the player.
        float hoverOffsetFactor = Lerp(0.69f, 1.3f, Projectile.identity / 13f % 1f);
        float ropeLength = Lerp(840f, 990f, Projectile.identity / 14f % 1f);
        float verticalOffset = Utils.Remap(Time, 0f, 45f, -1750f, -ropeLength * 1.32f - DangleVerticalOffset);
        Vector2 dangleTop = closestPlayer.Center + new Vector2(0f, verticalOffset) + RopeOffsetAngle.ToRotationVector2() * new Vector2(650f, 470f) * hoverOffsetFactor;
        DanglingRope ??= new(dangleTop, Vector2.Zero, 50, ropeLength);
        DanglingRope.Update(dangleTop, Utils.Remap(closestPlayer.velocity.Y, 0f, -12f, 0.5f, -0.1f));

        // Apply velocity motion.
        if (DanglingFromTop)
            Projectile.Center = DanglingRope.EndPosition;
        else if (FadeOutInterpolant >= 1f)
        {
            float originalSpeed = Projectile.velocity.Length();
            float newSpeed = Clamp(originalSpeed + 0.6f, 2f, 18.6f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.SafeDirectionTo(closestPlayer.Center) * originalSpeed, 0.042f);
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * newSpeed;

            DangleVerticalOffset += 6f;
            DangleVerticalOffset *= 1.2f;
        }
        else
            Projectile.velocity *= 0.971f;

        // Check for player proximity.
        if (!HasFlownPastPlayer && Projectile.WithinRange(closestPlayer.Center, 210f))
        {
            HasFlownPastPlayer = true;
            Projectile.netUpdate = true;
        }

        // Explode if decently far away after being close to the player before.
        if ((HasFlownPastPlayer || Time >= 240f) && (!Projectile.WithinRange(closestPlayer.Center, 400f) && Time >= 180f) && Main.rand.NextBool(10))
        {
            StrongBloom bloom = new StrongBloom(Projectile.Center, Vector2.Zero, Color.Wheat, 2f, 27);
            bloom.Spawn();

            bloom = new(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Red, Color.Violet, Main.rand.NextFloat(0.7f)) * 0.6f, 3.5f, 40);
            bloom.Spawn();

            Projectile.Kill();
        }

        // Play heartbeat sounds.
        if (Projectile.soundDelay <= 0)
        {
            if (Time >= 60)
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.DisgustingStarBeat with { Volume = 1.3f, MaxInstances = 32 }, Projectile.Center);
            BeatDelay = Utils.Clamp(BeatDelay - 19, 26, 120);
            Projectile.soundDelay = BeatDelay;
        }

        Time++;
    }

    public float RopeWidthFunction(float completionRatio)
    {
        float widthInterpolant = InverseLerp(0f, 0.16f, completionRatio, true) * InverseLerp(1f, 0.84f, completionRatio, true);
        widthInterpolant = Pow(widthInterpolant, 8f);
        float baseWidth = Lerp(120f, 124f, widthInterpolant);
        float pulseWidth = Lerp(0f, 150f, Pow(Sin(Main.GlobalTimeWrappedHourly * -5.6f + Projectile.whoAmI * 1.3f + completionRatio * 1.4f), 22f));
        return (baseWidth + pulseWidth) * 0.03f;
    }

    public override bool? CanDamage() => !DanglingFromTop;

    public override bool PreDraw(ref Color lightColor)
    {
        // Draw the ribbon.
        DanglingRope.DrawProjectionScuffed(WhitePixel, Vector2.UnitY * 20f - Main.screenPosition, Projectile.identity % 2 == 0, _ => Color.DarkRed * Projectile.Opacity, RopeWidthFunction, lengthStretch: 0.707f);

        // Draw spires.
        float spireScale = Lerp(0.85f, 1.1f, Sin01(Main.GlobalTimeWrappedHourly * 17.5f + Projectile.identity)) * Projectile.scale * 0.46f;
        float spireOpacity = Pow(FadeOutInterpolant, 1.9f) * Projectile.Opacity;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Main.spriteBatch.Draw(ChromaticSpires, drawPosition, null, (Color.Violet with { A = 0 }) * spireOpacity, Projectile.rotation + -PiOver4, ChromaticSpires.Size() * 0.5f, spireScale, 0, 0f);
        Main.spriteBatch.Draw(ChromaticSpires, drawPosition, null, (Color.Violet with { A = 0 }) * spireOpacity, Projectile.rotation + PiOver4, ChromaticSpires.Size() * 0.5f, spireScale, 0, 0f);

        // Draw the star.
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Color starColor = Color.Lerp(Color.MediumPurple, Color.Red, Projectile.identity / 11f % 0.7f) * FadeOutInterpolant.Cubed();
        starColor = Color.Lerp(starColor, Color.White, 0.2f);
        Rectangle frame = texture.Frame(1, 2, 0, Projectile.frame);
        for (int i = 0; i < 2; i++)
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, starColor with { A = 0 }, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, 0, 0f);

        // Draw bloom.
        float bloomScaleFactor = 1f + (1f - FadeOutInterpolant) + Sin01(Main.GlobalTimeWrappedHourly * 8f) * 0.4f;
        Color bloomColor = Color.Lerp(new(210, 0, 0, 0), new(182, 24, 31, 0), Projectile.identity / 10f % 1f);
        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, bloomColor * Projectile.Opacity, 0f, BloomCircleSmall.Size() * 0.5f, Projectile.scale * bloomScaleFactor * 2f, 0, 0f);
        for (int i = 0; i < 8; i++)
            Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, new Color(255, 16, 30, 0) * Projectile.Opacity * 0.5f, 0f, BloomCircleSmall.Size() * 0.5f, Projectile.scale * bloomScaleFactor * 1.1f, 0, 0f);

        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Color.Wheat with { A = 0 } * Projectile.Opacity * (1f - FadeOutInterpolant) * 2f, 0f, BloomCircleSmall.Size() * 0.5f, Projectile.scale * bloomScaleFactor * 1.4f, 0, 0f);

        return false;
    }

    public void DrawSubtractive(SpriteBatch spriteBatch)
    {
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Color.White * Projectile.Opacity * Saturate(Projectile.scale) * 0.8f, 0f, BloomCircleSmall.Size() * 0.5f, Projectile.scale * 3f, 0, 0f);
        spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Color.White * Projectile.Opacity * Saturate(Projectile.scale) * 0.54f, 0f, BloomCircleSmall.Size() * 0.5f, Projectile.scale * 4f, 0, 0f);
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.DisgustingStarExplode with { Volume = 1.2f, MaxInstances = 10 }, Projectile.Center);

        // Explode into a bunch of gore.
        BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
        for (int i = 0; i < 50; i++)
        {
            Vector2 bloodSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
            Vector2 bloodVelocity = Main.rand.NextVector2Circular(23.5f, 8f) - Vector2.UnitY * 9f;
            if (Main.rand.NextBool(6))
                bloodVelocity *= 1.45f;
            if (Main.rand.NextBool(6))
                bloodVelocity *= 1.45f;
            bloodVelocity += Projectile.velocity * 0.85f;

            metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(30f, 50f), Main.rand.NextFloat());
        }

        StrongBloom bloom = new StrongBloom(Projectile.Center, Vector2.Zero, Color.Crimson, 1.5f, 23);
        bloom.Spawn();

        // Create a bunch of stars.
        for (int i = 0; i < 15; i++)
        {
            int starPoints = Main.rand.Next(3, 9);
            float starScaleInterpolant = Main.rand.NextFloat();
            int starLifetime = (int)Lerp(30f, 60f, starScaleInterpolant);
            float starScale = Lerp(0.67f, 0.98f, starScaleInterpolant) * Projectile.scale;
            Color starColor = Color.Lerp(Color.Red, Color.Wheat, 0.4f) * 0.4f;

            // Calculate the star velocity.
            Vector2 starVelocity = Main.rand.NextVector2Circular(25f, 14f);
            TwinkleParticle star = new TwinkleParticle(Projectile.Center, starVelocity, starColor, starLifetime, starPoints, new Vector2(Main.rand.NextFloat(0.4f, 1.6f), 1f) * starScale, starColor * 0.5f);
            star.Spawn();
        }
    }
}
