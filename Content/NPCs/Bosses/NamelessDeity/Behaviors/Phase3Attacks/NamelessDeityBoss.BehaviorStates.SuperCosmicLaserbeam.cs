using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.FormPresets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.Configuration;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using NoxusBoss.Core.SoundSystems;
using NoxusBoss.Core.SoundSystems.Music;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers.NamelessDeitySky;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    /// <summary>
    /// Nameless' chant sound during his Super Cosmic Laserbeam state.
    /// </summary>
    public LoopedSoundInstance ChantSound;

    /// <summary>
    /// Nameless' laser loop sound during his Super Cosmic Laserbeam state.
    /// </summary>
    public LoopedSoundInstance CosmicLaserSound;

    /// <summary>
    /// How long Nameless waits after teleporting to begin attacking during his Super Cosmic Laserbeam state.
    /// </summary>
    public static int SuperCosmicLaserbeam_TeleportDelay => GetAIInt("SuperCosmicLaserbeam_TeleportDelay");

    /// <summary>
    /// How long Nameless waits before attacking during his Super Cosmic Laserbeam state.
    /// </summary>
    public static int SuperCosmicLaserbeam_AttackDelay => SuperCosmicLaserbeam_TeleportDelay + CosmicMagicCircle.ConvergeTime + 150;

    /// <summary>
    /// How long Nameless' laserbeam exists during his Super Cosmic Laserbeam state.
    /// </summary>
    public static int SuperCosmicLaserbeam_LaserLifetime => GetAIInt("SuperCosmicLaserbeam_LaserLifetime");

    /// <summary>
    /// How long it takes for Nameless' special background effects to fade out during his Super Cosmic Laserbeam state.
    /// </summary>
    public static int SuperCosmicLaserbeam_BackgroundEffectFadeoutTime => GetAIInt("SuperCosmicLaserbeam_BackgroundEffectFadeoutTime");

    /// <summary>
    /// How many immunity frames the playet is granted on hit by the laserbeam during Nameless' Super Cosmic Laserbeam state.
    /// </summary>
    public static int SuperCosmicLaserbeam_LaserPlayerIframes => (int)Clamp(GetAIInt("SuperCosmicLaserbeam_LaserPlayerIframes") / Myself_DifficultyFactor, 1f, 1000f);

    /// <summary>
    /// The rate at which reality tears are released during Nameless' Super Cosmic Laserbeam state.
    /// </summary>
    public static int SuperCosmicLaserbeam_RealityTearReleaseRate => (int)Clamp(GetAIInt("SuperCosmicLaserbeam_RealityTearReleaseRate") / Pow(Myself_DifficultyFactor, 0.42f), 6f, 10000f);

    /// <summary>
    /// How long Nameless' Super Cosmic Laserbeam state goes on for.
    /// </summary>
    public static int SuperCosmicLaserbeam_AttackDuration => SuperCosmicLaserbeam_AttackDelay + SuperCosmicLaserbeam_LaserLifetime + SuperCosmicLaserbeam_BackgroundEffectFadeoutTime;

    /// <summary>
    /// The general angular velocity factor for Nameless' laser during his Super Cosmic Laserbeam state. 
    /// </summary>
    public static float SuperCosmicLaserbeam_LaserAngularVelocityFactor => GetAIFloat("SuperCosmicLaserbeam_LaserAngularVelocityFactor") * Pow(Myself_DifficultyFactor, 0.58f);

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_SuperCosmicLaserbeam()
    {
        // Load the transition from SuperCosmicLaserbeam to the next in the cycle.
        StateMachine.RegisterTransition(NamelessAIType.SuperCosmicLaserbeam, null, false, () =>
        {
            // Go to the next attack state immediately if the laser is missing.
            // The initial time check delay in multiplayer is to create a grace period, given there will inevitably be latency before the beam projectile is spawned.
            int checkTimeDelay = SuperCosmicLaserbeam_AttackDelay;
            if (Main.netMode != NetmodeID.SinglePlayer)
                checkTimeDelay += 60;

            if (AITimer >= checkTimeDelay && AITimer <= SuperCosmicLaserbeam_AttackDelay + SuperCosmicLaserbeam_LaserLifetime - 30f && !AnyProjectiles(ModContent.ProjectileType<SuperCosmicBeam>()))
                return true;

            return AITimer >= SuperCosmicLaserbeam_AttackDuration;
        }, () =>
        {
            SoundMufflingSystem.MuffleFactor = 1f;
            MusicVolumeManipulationSystem.MuffleFactor = 1f;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.SuperCosmicLaserbeam, DoBehavior_SuperCosmicLaserbeam);
    }

    public void DoBehavior_SuperCosmicLaserbeam()
    {
        int attackDelay = SuperCosmicLaserbeam_AttackDelay;
        int laserShootTime = SuperCosmicLaserbeam_LaserLifetime;
        int realityTearReleaseRate = SuperCosmicLaserbeam_RealityTearReleaseRate;
        float laserAngularVelocity = Utils.Remap(NPC.Distance(Target.Center), 1150f, 1775f, 0.0161f, 0.074f) * SuperCosmicLaserbeam_LaserAngularVelocityFactor;

        ref float laserDirection = ref NPC.ai[2];
        ref float windIntensity = ref NPC.ai[3];

        Vector2 laserStart = NPC.Center + laserDirection.ToRotationVector2() * 100f;

        // Flap wings.
        UpdateWings(AITimer / 50f);

        // Teleport near the target.
        TeleportVisualsInterpolant = 0f;
        if (AITimer == 1)
        {
            StartTeleportAnimation(() =>
            {
                Vector2 teleportPosition = Target.Center + new Vector2(TargetDirection * -400f, -240f);
                if (teleportPosition.Y < 800f)
                    teleportPosition.Y = 800f;
                while (Collision.SolidCollision(teleportPosition, NPC.width, NPC.height + 250))
                    teleportPosition.Y -= 16f;

                return teleportPosition;
            }, 13, 13);
        }

        if (AITimer == 2)
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.CosmicLaserChargeUp with { Volume = 1.2f });

        if (AITimer <= attackDelay - 1)
        {
            float rumbleInterpolant = InverseLerp(0f, attackDelay - 1f, AITimer);
            ScreenShakeSystem.SetUniversalRumble(rumbleInterpolant.Cubed() * 6f, TwoPi, null, 0.35f);
        }

        // Cast the book after the teleport.
        if (Main.netMode != NetmodeID.MultiplayerClient && !AnyProjectiles(ModContent.ProjectileType<CosmicMagicCircle>()) && AITimer <= attackDelay)
            NPC.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX, ModContent.ProjectileType<CosmicMagicCircle>(), 0, 0f);

        // Make the sky more pale.
        if (AITimer <= 75f)
            DifferentStarsInterpolant = InverseLerp(0f, 60f, AITimer);
        HeavenlyBackgroundIntensity = Lerp(1f, 0.5f, DifferentStarsInterpolant);

        // Periodically fire reality tears at the starting point of the laser.
        if (AITimer >= attackDelay && AITimer <= attackDelay + laserShootTime - 60f && AITimer % realityTearReleaseRate == 0f)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 3; i++)
                {
                    float sliceAngle = Pi * i / 3f + laserDirection + PiOver2;
                    Vector2 sliceDirection = sliceAngle.ToRotationVector2();
                    NPC.NewProjectileBetter(NPC.GetSource_FromAI(), laserStart - sliceDirection * 2000f, sliceDirection, ModContent.ProjectileType<TelegraphedScreenSlice>(), 0, 0f, -1, 30f, 4000f);
                }

                if (Target.Center.WithinRange(NPC.Center, 335f))
                {
                    for (int i = 0; i < 3; i++)
                        NPC.NewProjectileBetter(NPC.GetSource_FromAI(), EyePosition, (Target.Center - EyePosition).SafeNormalize(Vector2.UnitY) * 5.6f + Main.rand.NextVector2Circular(0.9f, 0.9f), ModContent.ProjectileType<Starburst>(), StarburstDamage, 0f);
                }
            }
        }

        // Make the wind intensify as necessary.
        bool windExists = AITimer >= attackDelay && AITimer <= attackDelay + laserShootTime - 20f;
        windIntensity = Saturate(windIntensity + windExists.ToDirectionInt() * 0.075f);

        // Periodically create screen pulse effects.
        if (AITimer >= attackDelay && AITimer % 30f == 0f)
        {
            GeneralScreenEffectSystem.ChromaticAberration.Start(NPC.Center, 0.2f, 15);
            RadialScreenShoveSystem.Start(Vector2.Lerp(laserStart, Target.Center, 0.9f), 20);
        }

        // Play mumble sounds.
        if (AITimer == attackDelay - 60f && !NamelessDeityFormPresetRegistry.UsingLynelPreset)
            PerformMumble();

        if (NamelessDeityFormPresetRegistry.UsingLynelPreset && AITimer == attackDelay - 105f)
            CosmicLaserSound = LoopedSoundManager.CreateNew(GennedAssets.Sounds.NamelessDeity.JermaImKillingYou with { Volume = 1.5f }, () => !NPC.active || CurrentState != NamelessAIType.SuperCosmicLaserbeam);
        if (!NamelessDeityFormPresetRegistry.UsingLynelPreset && AITimer == attackDelay + 90f)
            CosmicLaserSound = LoopedSoundManager.CreateNew(GennedAssets.Sounds.NamelessDeity.CosmicLaserLoop with { Volume = 1.2f }, () => !NPC.active || CurrentState != NamelessAIType.SuperCosmicLaserbeam);

        // Make the music wind down and stop before the beam is fired.
        if (AITimer >= attackDelay - 120 && AITimer <= attackDelay + laserShootTime - 30)
            StopMusic = true;

        // Create the super laser.
        if (AITimer == attackDelay)
        {
            SoundEngine.StopTrackedSounds();

            // Shake the screen.
            if (!WoTGConfig.Instance.PhotosensitivityMode)
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center, NPC.SafeDirectionTo(Target.Center), 42f, 2.75f, 112));
            GeneralScreenEffectSystem.HighContrast.Start(NPC.Center, 24f, 60);

            RadialScreenShoveSystem.Start(NPC.Center, 54);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<LightWave>(), 0, 0f);
                NPC.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, laserDirection.ToRotationVector2(), ModContent.ProjectileType<SuperCosmicBeam>(), CosmicLaserbeamDamage, 0f, -1, 0f, SuperCosmicLaserbeam_LaserLifetime);
            }

            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.CosmicLaserStart with { Volume = 25f });
        }

        if (AITimer == attackDelay + 90)
        {
            // Make Nameless chant.
            ChantSound?.Stop();
            if (!NamelessDeityFormPresetRegistry.UsingLynelPreset)
                ChantSound = LoopedSoundManager.CreateNew(GennedAssets.Sounds.NamelessDeity.ChantLoop with { Volume = 1.1f }, () => !NPC.active || CurrentState != NamelessAIType.SuperCosmicLaserbeam);
        }

        float muffleInterpolant = InverseLerpBump(attackDelay + 17f, attackDelay + 56f, attackDelay + laserShootTime - 40f, attackDelay + laserShootTime + 32f, AITimer);
        float attackCompletion = InverseLerp(0f, laserShootTime - 30f, AITimer - attackDelay);
        if (AITimer >= attackDelay || NamelessDeityFormPresetRegistry.UsingLynelPreset)
        {
            float fadeIn = SmoothStep(0f, 1f, InverseLerp(90f, 300f, AITimer - attackDelay)).Cubed();
            CosmicLaserSound?.Update(Main.LocalPlayer.Center, sound =>
            {
                float fadeOut = InverseLerp(0.98f, 0.93f, attackCompletion);

                if (sound.Sound is not null)
                {
                    sound.Sound.Volume = Saturate(Main.soundVolume * fadeIn * fadeOut * 1.8f);
                    sound.Sound.Pitch = NamelessDeityFormPresetRegistry.UsingLynelPreset ? 0f : Lerp(0.01f, 0.6f, Pow(attackCompletion, 1.5f));
                }
            });
            ChantSound?.Update(Main.LocalPlayer.Center, sound =>
            {
                float fadeOut = InverseLerp(0.98f, 0.9f, attackCompletion);
                sound.Volume = fadeIn * fadeOut;
                if (sound.Sound is not null)
                    sound.Sound.Volume = Saturate(Main.soundVolume * fadeIn * fadeOut);
            });
        }

        if (AITimer >= attackDelay)
        {
            // Make all other sounds rapidly fade out.
            SoundMufflingSystem.MuffleFactor = Lerp(1f, 0.01f, muffleInterpolant);
            MusicVolumeManipulationSystem.MuffleFactor = Lerp(1f, 0.01f, muffleInterpolant);

            // Make all sounds cease.
            if (AITimer >= attackDelay + laserShootTime + 60f)
            {
                CosmicLaserSound?.Stop();
                ChantSound?.Stop();
            }
        }

        if (AITimer >= attackDelay && AITimer < attackDelay + laserShootTime)
        {
            // Keep the keyboard shader brightness at its maximum.
            NamelessDeityKeyboardShader.BrightnessIntensity = 1f;

            // Push the player away from Nameless, to prevent cheesing the beam.
            float generalPushForceIntensity = InverseLerpBump(0f, 20f, laserShootTime - 20f, laserShootTime, AITimer - attackDelay);
            foreach (Player player in Main.ActivePlayers)
            {
                float distanceFromNameless = player.Distance(NPC.Center);
                float localPushForceInterpolant = InverseLerp(350f, 100f, distanceFromNameless).Squared();
                float pushForce = generalPushForceIntensity * localPushForceInterpolant * (player.velocity.Length() + 0.1f);
                player.velocity -= player.SafeDirectionTo(NPC.Center) * pushForce;

                if (localPushForceInterpolant >= 0.15f)
                    player.mount?.Dismount(player);
            }
        }

        // Very slowly fly towards the target.
        if (NPC.WithinRange(Target.Center, 40f))
            NPC.velocity *= 0.92f;
        else
            NPC.SimpleFlyMovement(NPC.SafeDirectionTo(Target.Center) * 2f, 0.15f);

        // Zoom towards the as the attack ends.
        if (AITimer >= attackDelay + laserShootTime - 5f)
        {
            float slowdownRadius = Utils.Remap(AITimer - attackDelay - laserShootTime, 5f, 50f, 270f, 600f);
            NPC.SmoothFlyNearWithSlowdownRadius(Target.Center, 0.07f, 0.09f, slowdownRadius);
        }

        // Spin the laser towards the target. If the player runs away it locks onto them.
        float idealLaserDirection = NPC.AngleTo(Target.Center);
        laserDirection = laserDirection.AngleLerp(idealLaserDirection, laserAngularVelocity);
        float idealHandAimDirection = InverseLerpBump(attackDelay - 90f, attackDelay - 36f, SuperCosmicLaserbeam_AttackDuration - 90f, SuperCosmicLaserbeam_AttackDuration - 32f, AITimer);

        // Update universal hands.
        float verticalOffset = Sin(AITimer / 20f) * 90f;
        Vector2 leftHoverOffset = new Vector2(-1100f, verticalOffset + 160f) * TeleportVisualsAdjustedScale;
        Vector2 rightHoverOffset = new Vector2(1100f, verticalOffset + 160f) * TeleportVisualsAdjustedScale;
        leftHoverOffset = Vector2.Lerp(leftHoverOffset, new(Sin(AITimer / 6f) * 40f - 540f, verticalOffset - 440f), windIntensity);
        rightHoverOffset = Vector2.Lerp(rightHoverOffset, new(Sin(AITimer / 5f) * 40f + 540f, -verticalOffset - 440f), windIntensity);

        Hands[0].FreeCenter = NPC.Center + leftHoverOffset.RotatedBy(0f.AngleLerp(laserDirection + PiOver2, idealHandAimDirection));
        Hands[1].FreeCenter = NPC.Center + rightHoverOffset.RotatedBy(0f.AngleLerp(laserDirection + PiOver2, idealHandAimDirection));
        Hands[0].DirectionOverride = 1;
        Hands[1].DirectionOverride = -1;
        Hands[0].ArmInverseKinematicsFlipOverride = false;
        Hands[1].ArmInverseKinematicsFlipOverride = true;
        Hands[0].RotationOffset = -PiOver4;
        Hands[1].RotationOffset = PiOver4;
        Hands[0].Velocity = Vector2.Zero;
        Hands[1].Velocity = Vector2.Zero;

        // Make the stars return to normal shortly before transitioning to the next attack.
        if (AITimer >= attackDelay + laserShootTime)
            DifferentStarsInterpolant = Saturate(DifferentStarsInterpolant - 0.06f);
    }
}
