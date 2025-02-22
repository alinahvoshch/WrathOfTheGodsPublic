using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Luminance.Core.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class BloodTorrent : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<AvatarOfEmptiness>
{
    private LoopedSoundInstance torrentSound;

    /// <summary>
    /// How long this torrent has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// The starting position of this blood torrent.
    /// </summary>
    public static Vector2 Start => AvatarOfEmptiness.Myself?.Center + Vector2.UnitY * 140f ?? Vector2.Zero;

    /// <summary>
    /// The ending position of this blood torrent.
    /// </summary>
    public Vector2 End => Projectile.Center;

    /// <summary>
    /// How long this torrent is.
    /// </summary>
    public float TorrentLength => Vector2.Distance(Start, End);

    /// <summary>
    /// The ideal length of this torrent.
    /// </summary>
    public float IdealBloodTorrentLength => Clamp(Time * 120f, 0f, MaxTorrentLength);

    /// <summary>
    /// The maximum length that this torrent can have.
    /// </summary>
    public static float MaxTorrentLength => 5200f;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 900000;
        Projectile.hide = true;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        // Ensure the Avatar is present. If he isn't, die immediately.
        if (AvatarOfEmptiness.Myself is null)
        {
            Projectile.Kill();
            return;
        }

        torrentSound ??= LoopedSoundManager.CreateNew(GennedAssets.Sounds.Avatar.HeavyBloodStreamLoop with { Volume = 0.9f }, () => !Projectile.active);
        torrentSound?.Update(Start, sound =>
        {
            sound.Volume = InverseLerp(0f, 30f, Time);
        });

        Projectile.Center = Vector2.Lerp(Projectile.Center, Start + Vector2.UnitY * IdealBloodTorrentLength, 0.035f);
        Time++;
    }

    public float BloodWidthFunction(float completionRatio)
    {
        float baseWidth = 4f;
        float sinusoidalWidth = Cos01(TwoPi * Time / -6.5f + completionRatio * 40f) * 30f;
        float downwardExpandInterpolant = InverseLerp(0.6f, 1f, TorrentLength / MaxTorrentLength);
        float downwardExpandWidth = Pow(completionRatio, 1.1f) * TorrentLength / MaxTorrentLength * downwardExpandInterpolant * 600f;
        return baseWidth + sinusoidalWidth + downwardExpandWidth;
    }

    public Color BloodColorFunction(float completionRatio) => Projectile.GetAlpha(Color.Red);

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        // Measure how far along the blood's length the target is.
        // If the signed distance is negative (a.k.a. they're behind the blood) or above the blood length (a.k.a. they're beyond the blood), terminate this
        // method immediately.
        Vector2 directionToEnd = Start.SafeDirectionTo(End);
        float signedDistanceAlongTorrent = SignedDistanceToLine(targetHitbox.Center(), Start, directionToEnd);
        if (signedDistanceAlongTorrent < 0f || signedDistanceAlongTorrent >= TorrentLength)
            return false;

        // Now that the point on the blood is known from the distance, evaluate the exact width of the blood at said point for use with a AABB/line collision check.
        float bloodWidth = BloodWidthFunction(signedDistanceAlongTorrent / TorrentLength);
        Vector2 perpendicular = new Vector2(-directionToEnd.Y, directionToEnd.X);
        Vector2 checkPoint = Start + directionToEnd * signedDistanceAlongTorrent;
        Vector2 left = checkPoint - perpendicular * bloodWidth;
        Vector2 right = checkPoint + perpendicular * bloodWidth;

        float _ = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), left, right, 32f, ref _);
    }

    public override bool ShouldUpdatePosition() => false;

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ManagedShader bloodShader = ShaderManager.GetShader("NoxusBoss.AvatarBloodTorrentShader");
        bloodShader.SetTexture(WavyBlotchNoise, 1, SamplerState.PointWrap);

        List<Vector2> bloodDrawPositions = [];
        for (int i = 0; i < 10; i++)
            bloodDrawPositions.Add(Vector2.Lerp(Start, End, i / 9f));

        PrimitiveSettings settings = new PrimitiveSettings(BloodWidthFunction, BloodColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: bloodShader);
        PrimitiveRenderer.RenderTrail(bloodDrawPositions, settings, 45);
    }
}
