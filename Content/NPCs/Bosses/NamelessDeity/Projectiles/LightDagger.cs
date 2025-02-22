using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Graphics.Automators;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;

public class LightDagger : ModProjectile, IProjOwnedByBoss<NamelessDeityBoss>, IDrawsWithShader
{
    public float DaggerAppearInterpolant => InverseLerp(0f, TelegraphTime - 3f, Time);

    public ref float Time => ref Projectile.localAI[0];

    public ref float TelegraphTime => ref Projectile.ai[0];

    public ref float HueInterpolant => ref Projectile.ai[1];

    public ref float Index => ref Projectile.ai[2];

    public static readonly Palette IridescencePalette = new Palette(Color.White, new Color(193, 255, 244), new Color(162, 224, 245), new Color(108, 255, 188), new Color(87, 184, 226), new Color(207, 172, 252));

    public const int DefaultGrazeDelay = 150;

    public const string GrazeEchoFieldName = "GrazeEchoSoundDelay";

    public override string Texture => GetAssetPath("Content/NPCs/Bosses/NamelessDeity/Projectiles", Name);

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 8;
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;

        PlayerDataManager.PostUpdateEvent += DecrementGrazeSoundDelay;
    }

    private void DecrementGrazeSoundDelay(PlayerDataManager p)
    {
        p.GetValueRef<int>(GrazeEchoFieldName).Value--;
    }

    public override void SetDefaults()
    {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;

        // Increased so that the graze checks are more precise.
        Projectile.MaxUpdates = 2;

        Projectile.timeLeft = Projectile.MaxUpdates * 120;
        Projectile.Opacity = 0f;
    }

    public override void SendExtraAI(BinaryWriter writer) => writer.Write(Time);

    public override void ReceiveExtraAI(BinaryReader reader) => Time = reader.ReadSingle();

    public override void AI()
    {
        if (!Projectile.TryGetGenericOwner(out NPC nameless) || nameless.ModNPC is not NamelessDeityBoss namelessModNPC)
        {
            Projectile.Kill();
            return;
        }

        // Sharply fade in.
        Projectile.Opacity = InverseLerp(0f, 12f, Time);

        // Decide rotation based on direction.
        Projectile.rotation = Projectile.velocity.ToRotation();

        // Accelerate after the telegraph dissipates.
        bool canPlaySounds = namelessModNPC.CurrentState != NamelessDeityBoss.NamelessAIType.SuperCosmicLaserbeam;
        if (Time >= TelegraphTime)
        {
            float newSpeed = Clamp(Projectile.velocity.Length() + 5f / Projectile.MaxUpdates, 14f, 90f / Projectile.MaxUpdates);
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * newSpeed;

            // Play a graze sound if a player was very, very close to being hit.
            int closestIndex = Player.FindClosest(Projectile.Center, 1, 1);
            Player closest = Main.player[closestIndex];
            float playerDirectionAngle = Projectile.velocity.AngleBetween(closest.SafeDirectionTo(Projectile.Center));
            bool aimedTowardsClosest = playerDirectionAngle >= ToRadians(16f) && playerDirectionAngle < PiOver2;
            bool dangerouslyCloseToHit = Projectile.WithinRange(closest.Center, 57f) && aimedTowardsClosest;
            if (canPlaySounds && newSpeed >= 40f && dangerouslyCloseToHit && Main.myPlayer == closestIndex && closest.GetValueRef<int>(GrazeEchoFieldName) <= 0 && Projectile.Opacity >= 1f)
            {
                if (namelessModNPC.CurrentState != NamelessDeityBoss.NamelessAIType.EnterPhase2)
                    ScreenShakeSystem.StartShake(10f, Pi / 3f, Projectile.velocity, 0.15f);

                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.RealityTear with { Volume = 0.14f, PitchVariance = 0.24f, MaxInstances = 10 });
                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.DaggerGrazeEcho with { Volume = 2.4f });
                RadialScreenShoveSystem.Start(Projectile.Center, 24);
                GeneralScreenEffectSystem.ChromaticAberration.Start(Projectile.Center, 0.5f, 54);

                closest.GetValueRef<int>(GrazeEchoFieldName).Value = DefaultGrazeDelay;
            }
        }
        else
            Projectile.rotation += Pi;

        // Play the ordinary graze slice sound.
        if (Time == TelegraphTime + 9f && Index == 0 && canPlaySounds)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.DaggerGraze with { Volume = 0.6f, MaxInstances = 20 });
            ScreenShakeSystem.StartShake(4f);
        }

        if (Projectile.IsFinalExtraUpdate())
            Time++;
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        // Play a custom sound when hitting the player.        
        modifiers.DisableSound();
        SoundEngine.PlaySound(target.Male ? GennedAssets.Sounds.NamelessDeity.PlayerSliceMale : GennedAssets.Sounds.NamelessDeity.PlayerSliceFemale, target.Center);
    }

    public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

    public override bool PreDraw(ref Color lightColor)
    {
        if (Time <= TelegraphTime)
            DrawTelegraph();

        return false;
    }

    public void DrawTelegraph()
    {
        Vector2 start = Projectile.Center;
        Vector2 end = start + Projectile.velocity.SafeNormalize(Vector2.UnitY) * 1850f;
        Color telegraphColor = IridescencePalette.SampleColor(Projectile.identity / 11f % 1f) * Sqrt(1f - DaggerAppearInterpolant);
        telegraphColor.A = 0;

        Main.spriteBatch.DrawBloomLine(start, end, telegraphColor, Projectile.Opacity * 40f);
    }

    public void DrawWithShader(SpriteBatch spriteBatch)
    {
        Color daggerColor = Color.White * DaggerAppearInterpolant;
        daggerColor.A = 0;

        ManagedShader appearShader = ShaderManager.GetShader("NoxusBoss.DaggerAppearShader");
        appearShader.TrySetParameter("warpOffset", Pow(1f - DaggerAppearInterpolant, 1.4f));
        appearShader.TrySetParameter("justWarp", false);
        appearShader.SetTexture(GennedAssets.Textures.Extra.Iridescence, 1, SamplerState.LinearWrap);
        appearShader.Apply();

        Texture2D daggerTexture = GennedAssets.Textures.Projectiles.LightDagger.Value;
        Texture2D bloomTexture = GennedAssets.Textures.Projectiles.LightDaggerBloom.Value;
        Main.EntitySpriteDraw(bloomTexture, Projectile.Center - Main.screenPosition, null, daggerColor, Projectile.rotation, bloomTexture.Size() * 0.5f, Projectile.scale, 0, 0);
        Main.EntitySpriteDraw(daggerTexture, Projectile.Center - Main.screenPosition, null, daggerColor, Projectile.rotation, daggerTexture.Size() * 0.5f, Projectile.scale, 0, 0);

        appearShader.TrySetParameter("justWarp", true);
        appearShader.Apply();

        Texture2D afterimageTexture = GennedAssets.Textures.Projectiles.LightDaggerAfterimage.Value;
        for (int i = 6; i >= 0; i--)
        {
            Color afterimageColor = Color.White * DaggerAppearInterpolant * (1f - i / 7f);
            Vector2 afterimageOffset = Projectile.velocity * i * ShouldUpdatePosition().ToInt() * -0.56f;
            Main.EntitySpriteDraw(afterimageTexture, Projectile.Center - Main.screenPosition + afterimageOffset, null, afterimageColor, Projectile.rotation, afterimageTexture.Size() * 0.5f, Projectile.scale, 0, 0);
        }

        Texture2D glowmaskTexture = GennedAssets.Textures.Projectiles.LightDaggerGlowmask.Value;
        Main.EntitySpriteDraw(glowmaskTexture, Projectile.Center - Main.screenPosition, null, Color.White * DaggerAppearInterpolant, Projectile.rotation, glowmaskTexture.Size() * 0.5f, Projectile.scale, 0, 0);
    }

    public override bool? CanDamage() => Time >= TelegraphTime;

    public override bool ShouldUpdatePosition() => Time >= TelegraphTime;
}
