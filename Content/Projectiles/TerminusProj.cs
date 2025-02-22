using CalamityMod.Events;
using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.SoundSystems.Music;
using Terraria;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Projectiles;

public class TerminusProj : ModProjectile
{
    public enum TerminusAIState
    {
        RiseUpward,
        PerformEyeGleamEffect,
        Explode,
        CreateBeam
    }

    public bool HasInitialized
    {
        get => Projectile.localAI[0] == 1f;
        set => Projectile.localAI[0] = value.ToInt();
    }

    public TerminusAIState CurrentState
    {
        get => (TerminusAIState)Projectile.ai[0];
        set => Projectile.ai[0] = (int)value;
    }

    public static readonly SoundStyle ActivateSound = new SoundStyle("NoxusBoss/Assets/Sounds/Item/TerminusActivate");

    public static readonly PiecewiseCurve RiseMotionCurve = new PiecewiseCurve().
        Add(EasingCurves.Quadratic, EasingType.In, -4f, 0.36f). // Ascend motion.
        Add(EasingCurves.Linear, EasingType.In, -4f, 0.72f). // Rise without change.
        Add(EasingCurves.MakePoly(1.5f), EasingType.Out, 0f, 1f); // Slowdown.

    // These first three times are roughly synced to the duration of the Terminus chargeup sound, which is around 5.813 seconds (348 frames).
    public static int RiseUpwardTime => 92;

    public static int DimnessAppearTime => 96;

    public ref float Time => ref Projectile.ai[1];

    public ref float DarknessIntensity => ref Projectile.ai[2];

    public Player Owner => Main.player[Projectile.owner];

    public override string Texture => GetAssetPath("Content/Items/SummonItems", "Terminus");

    public override void SetDefaults()
    {
        Projectile.width = 58;
        Projectile.height = 70;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 90000;
        Projectile.Opacity = 0f;
        Projectile.tileCollide = false;
        Projectile.netImportant = true;
    }

    public override void AI()
    {
        // The Terminus refuses to exist if Nameless is around.
        if (NamelessDeityBoss.Myself is not null)
        {
            Projectile.active = false;
            return;
        }

        // Disallow entering the subworld if the world is saved on the cloud because apparently that's a common reason of why people's games are freezing.
        if (Main.ActiveWorldFileData.IsCloudSave)
        {
            BroadcastText(Language.GetTextValue("Mods.NoxusBoss.Dialog.CloudSaveMessage"), Color.Red);
            Projectile.active = false;
            return;
        }

        // Perform initialization effects on the first frame.
        if (!HasInitialized)
        {
            Projectile.position.X += Owner.direction * 38f;

            // Create the particle effects.
            ExpandingGreyscaleCircleParticle circle = new ExpandingGreyscaleCircleParticle(Projectile.Center, Vector2.Zero, new(219, 194, 229), 10, 0.28f);
            VerticalLightStreakParticle bigLightStreak = new VerticalLightStreakParticle(Projectile.Center, Vector2.Zero, new(228, 215, 239), 10, new(2.4f, 3f));
            MagicBurstParticle magicBurst = new MagicBurstParticle(Projectile.Center, Vector2.Zero, new(150, 109, 219), 12, 0.1f);
            for (int i = 0; i < 30; i++)
            {
                Vector2 smallLightStreakSpawnPosition = Projectile.Center + Main.rand.NextVector2Square(-Projectile.width, Projectile.width) * new Vector2(0.4f, 0.2f);
                Vector2 smallLightStreakVelocity = Vector2.UnitY * Main.rand.NextFloat(-4f, 4f);
                VerticalLightStreakParticle smallLightStreak = new VerticalLightStreakParticle(smallLightStreakSpawnPosition, smallLightStreakVelocity, Color.White, 10, new(0.1f, 0.3f));
                smallLightStreak.Spawn();
            }

            circle.Spawn();
            bigLightStreak.Spawn();
            magicBurst.Spawn();

            // Shake the screen a little bit.
            ScreenShakeSystem.SetUniversalRumble(5.6f, TwoPi, null, 0.2f);

            // Start playing the Terminus chargeup sound.
            SoundEngine.PlaySound(ActivateSound);

            HasInitialized = true;
        }

        // Quickly fade in.
        Projectile.Opacity = Saturate(Projectile.Opacity + 0.1667f);

        // Disable music.
        MusicVolumeManipulationSystem.MuffleFactor = 0f;

        // Perform AI states.
        switch (CurrentState)
        {
            case TerminusAIState.RiseUpward:
                DoBehavior_RiseUpward();
                break;
            case TerminusAIState.PerformEyeGleamEffect:
                DoBehavior_PerformEyeGleamEffect();
                break;
            case TerminusAIState.Explode:
                DoBehavior_Explode();
                break;
            case TerminusAIState.CreateBeam:
                DoBehavior_CreateBeam();
                break;
        }

        // Increment the general timer.
        Time++;
    }

    public void DoBehavior_RiseUpward()
    {
        // Rise upward. At the end of the animation the upward motion ceases.
        float animationCompletion = InverseLerp(0f, RiseUpwardTime, Time);
        Projectile.velocity = Vector2.UnitY * RiseMotionCurve.Evaluate(animationCompletion);

        // Periodically create chromatic aberration effects.
        if (Time % 30f == 29f)
        {
            float aberrationIntensity = Lerp(0.6f, 1.1f, animationCompletion);
            GeneralScreenEffectSystem.ChromaticAberration.Start(Projectile.Center, aberrationIntensity, 24);
        }

        // Bring the player to the stairs once the rise animation is completed.
        if (animationCompletion >= 1f)
        {
            Time = 0f;
            CurrentState = TerminusAIState.PerformEyeGleamEffect;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;
        }
    }

    public void DoBehavior_PerformEyeGleamEffect()
    {
        // Create a glitch sound on the first frame.
        if (Time == 1f)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.Glitch with { PitchVariance = 0.2f, Volume = 1.3f, MaxInstances = 8 });
            ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, 10.5f);
        }

        // Make everything go dim.
        DarknessIntensity = InverseLerp(0f, DimnessAppearTime, Time);

        // Go to the next AI state once the dimness animation is completed.
        if (DarknessIntensity >= 1f)
        {
            Time = 0f;
            CurrentState = TerminusAIState.Explode;
            Projectile.netUpdate = true;
        }

        ScreenShakeSystem.SetUniversalRumble(DarknessIntensity.Squared() * 9.5f, TwoPi, null, 0.2f);
    }

    public void DoBehavior_Explode()
    {
        if (Time == 59f)
            SoundEngine.PlaySound(BossRushEvent.Tier5TransitionSound with { MaxInstances = 0 });

        if (Time >= 90f)
        {
            CurrentState = TerminusAIState.CreateBeam;
            Projectile.netUpdate = true;
        }
    }

    public void DoBehavior_CreateBeam()
    {
        if (Main.myPlayer == Projectile.owner && Time == 91f)
            NewProjectileBetter(Projectile.GetSource_FromThis(), Main.LocalPlayer.Center, Vector2.Zero, ModContent.ProjectileType<TerminusProjectiveBeam>(), 0, 0f, Projectile.owner);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D terminus = GennedAssets.Textures.SummonItems.Terminus.Value;
        if (CurrentState == TerminusAIState.RiseUpward || CurrentState == TerminusAIState.PerformEyeGleamEffect)
            Main.EntitySpriteDraw(terminus, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation, terminus.Size() * 0.5f, Projectile.scale, 0, 0);
        RenderGleam();
        return false;
    }

    private void RenderGleam()
    {
        float glimmerInterpolant = 1f;
        if (CurrentState == TerminusAIState.PerformEyeGleamEffect)
            glimmerInterpolant = InverseLerp(0f, 90f, Time);
        if (CurrentState == TerminusAIState.RiseUpward)
            glimmerInterpolant = 0f;

        Texture2D flare = Luminance.Assets.MiscTexturesRegistry.ShineFlareTexture.Value;
        Texture2D bloom = BloomCircleSmall.Value;
        Texture2D backglowRing = GennedAssets.Textures.NamelessDeity.GlowRing.Value;

        float flareOpacity = InverseLerp(0f, 0.12f, glimmerInterpolant);
        float flareScale = Pow(Convert01To010(glimmerInterpolant), 0.3f) * 1.6f + 0.1f;
        if (float.IsNaN(flareScale))
            flareScale = 0f;
        if (CurrentState == TerminusAIState.Explode || CurrentState == TerminusAIState.CreateBeam)
            flareScale += Time * 0.03f;

        if (glimmerInterpolant >= 0.5f && flareScale < 1.5f)
            flareScale = 1.5f;

        flareScale *= InverseLerp(0f, 0.09f, glimmerInterpolant);

        float bloomScale = flareScale * (1f + Sin01(Main.GlobalTimeWrappedHourly * 50f) * 0.15f);

        float flareRotation = SmoothStep(0f, TwoPi, Pow(glimmerInterpolant, 0.2f));
        Color flareColorA = new Color(255, 28, 74, 0);
        Color flareColorB = new Color(255, 124, 127, 0);
        Color flareColorC = new Color(245, 22, 54, 0);
        Vector2 drawPosition = Projectile.Center - Vector2.UnitY.RotatedBy(Projectile.rotation) * Projectile.scale * 10f - Main.screenPosition;

        Main.spriteBatch.Draw(bloom, drawPosition, null, new(1f, 1f, 1f, 0f), 0f, bloom.Size() * 0.5f, bloomScale * 0.6f, 0, 0f);
        Main.spriteBatch.Draw(bloom, drawPosition, null, flareColorA * flareOpacity * 0.32f, 0f, bloom.Size() * 0.5f, bloomScale * 1.9f, 0, 0f);
        Main.spriteBatch.Draw(bloom, drawPosition, null, flareColorB * flareOpacity * 0.56f, 0f, bloom.Size() * 0.5f, bloomScale, 0, 0f);

        float verticalSquish = InverseLerp(0.4f, 0.75f, glimmerInterpolant) * 0.4f;
        Main.spriteBatch.Draw(flare, drawPosition, null, flareColorC * flareOpacity, flareRotation, flare.Size() * 0.5f, flareScale * new Vector2(1f - verticalSquish, 1f), 0, 0f);

        Main.spriteBatch.Draw(backglowRing, drawPosition, null, new Color(255, 227, 72, 0) * Sqrt(flareOpacity), flareRotation, backglowRing.Size() * 0.5f, 0.16f, 0, 0f);
    }
}
