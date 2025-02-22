using Luminance.Common.DataStructures;
using Luminance.Common.Easings;
using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Content.Particles;
using NoxusBoss.Content.Particles.Metaballs;
using NoxusBoss.Core.Fixes;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    /// <summary>
    /// How long the Avatar sits and place and cries while releasing blood during his unclog attack.
    /// </summary>
    public static int Unclog_BloodSpewDuration => GetAIInt("Unclog_BloodSpewDuration");

    /// <summary>
    /// How long it takes for the Avatar's tears to fully fall during his unclog attack.
    /// </summary>
    public static int Unclog_WeepStartTime => GetAIInt("Unclog_WeepStartTime");

    /// <summary>
    /// The rate at which the Avatar creates blobs of blood during the unclog attack.
    /// </summary>
    public static int Unclog_BlobSpawnRate => GetAIInt("Unclog_BlobSpawnRate");

    /// <summary>
    /// The amount of damage the Avatar's blood blobs do.
    /// </summary>
    public static int BloodBlobDamage => GetAIInt("BloodBlobDamage");

    [AutomatedMethodInvoke]
    public void LoadState_Unclog()
    {
        StateMachine.RegisterTransition(AvatarAIType.Unclog, null, false, () =>
        {
            return AITimer >= Unclog_BloodSpewDuration;
        }, () =>
        {
            BloodyTearsAnimationStartInterpolant = 0f;
            BloodyTearsAnimationEndInterpolant = 0f;
            LeftArmPosition = NPC.Center;
            RightArmPosition = NPC.Center;

            // Kill leftover projectiles.
            IProjOwnedByBoss<AvatarOfEmptiness>.KillAll();
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(AvatarAIType.Unclog, DoBehavior_Unclog);
        AttackDimensionRelationship[AvatarAIType.Unclog] = AvatarDimensionVariants.VisceralDimension;
    }

    public void DoBehavior_Unclog()
    {
        LookAt(Target.Center);

        // Decide the distortion intensity.
        if (AITimer >= 10)
            IdealDistortionIntensity = InverseLerp(Unclog_BloodSpewDuration, Unclog_BloodSpewDuration * 0.85f, AITimer) * 2f;
        else
        {
            DistortionIntensity = 0f;
            IdealDistortionIntensity = 0f;
        }

        // Make the tears appear.
        BloodyTearsAnimationStartInterpolant = InverseLerp(0f, BloodiedWeep_WeepStartTime, AITimer);

        // Make the tears disappear before the state terminates.
        BloodyTearsAnimationEndInterpolant = InverseLerp(-40f, -5f, AITimer - Unclog_BloodSpewDuration);

        float animationInterpolant = BloodyTearsAnimationStartInterpolant * (1f - BloodyTearsAnimationEndInterpolant);

        if (AITimer == 1 && !NPC.WithinRange(Target.Center, 560f))
            StartTeleportAnimation(() => Target.Center - Vector2.UnitY * 450f);

        // Slow down.
        NPC.velocity *= 0.85f;

        // Play a sound at first.
        if (AITimer == 3)
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.BloodCryViolent);

        // Apparently this is needed after the Cryostasis attack. I don't know why.
        if (NPC.position.Y < 0f)
            StartTeleportAnimation(() => Target.Center + Vector2.UnitX * Main.rand.NextFromList(-700f, 700f));

        // Have the Avatar's arms explode into a bunch of blood.
        if (AITimer == Unclog_WeepStartTime)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ArmJutOut with { MaxInstances = 0, Volume = 0.85f }, NPC.Center);

            ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 30f, shakeStrengthDissipationIncrement: 0.97f);
            GeneralScreenEffectSystem.RadialBlur.Start(NPC.Center, 1.5f, 18);
            RadialScreenShoveSystem.Start(NPC.Center, 75);

            // Create blood burst particles.
            for (int i = 0; i < 54; i++)
            {
                float bloodScale = Main.rand.NextFloat(0.6f, 1.5f);
                Color bloodColor = Color.Lerp(new(Main.rand.Next(115, 233), 0, 0), new(76, 24, 60), Main.rand.NextFloat(0.4f));
                Vector2 bloodVelocity = Main.rand.NextVector2Circular(6f, 27f) - Vector2.UnitY * Main.rand.NextFloat(1f, 3f);
                if (Main.rand.NextBool())
                    bloodVelocity = bloodVelocity.RotatedBy(-0.95f);
                else
                    bloodVelocity = bloodVelocity.RotatedBy(1.23f);

                BloodParticle2 bloodSplatter = new BloodParticle2(NPC.Center + bloodVelocity * 20f, bloodVelocity, 36, bloodScale, bloodColor);
                bloodSplatter.Spawn();
            }
            for (int i = 0; i < 90; i++)
            {
                float bloodScale = Main.rand.NextFloat(0.6f, 2.5f);
                Color bloodColor = Color.Lerp(Color.Red, new(137, 44, 78), Main.rand.NextFloat(0.7f));
                Vector2 bloodVelocity = (Main.rand.NextVector2Circular(20f, 11f) - Vector2.UnitY * Main.rand.NextFloat(2f, 7f)) * Main.rand.NextFloat(1.3f, 3.3f);
                if (Main.rand.NextBool())
                    bloodVelocity = bloodVelocity.RotatedBy(-0.95f);
                else
                    bloodVelocity = bloodVelocity.RotatedBy(1.23f);

                BloodParticle blood = new BloodParticle(NPC.Center + bloodVelocity * 10f, bloodVelocity, 36, bloodScale, bloodColor);
                blood.Spawn();
            }

            BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
            for (int i = 0; i < 150; i++)
                metaball.CreateParticle(SpiderLilyPosition, Main.rand.NextVector2Circular(80f, 80f), Main.rand.NextFloat(30f, 130f), Main.rand.NextFloat());

            // Create a burst of blood.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 54; i++)
                {
                    Vector2 bloodShootVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(8f, 19f);
                    NewProjectileBetter(NPC.GetSource_FromAI(), SpiderLilyPosition + Vector2.UnitY * 150f + Main.rand.NextVector2Circular(40f, 40f), bloodShootVelocity, ModContent.ProjectileType<BloodBlob>(), BloodBlobDamage, 0f, -1, 1f, 0f, -0.06f);
                }
            }

            NPC.netUpdate = true;
        }

        if (CreateBloodCryVisuals)
        {
            // Release a bunch of blood particles on the Avatar's face.
            Vector2 bloodSpawnPosition = HeadPosition + Vector2.UnitY * NPC.scale * 160f;
            Color bloodColor = Color.Lerp(new(255, 0, 30), Color.Brown, Main.rand.NextFloat(0.4f, 0.8f)) * animationInterpolant * 0.45f;
            LargeMistParticle blood = new LargeMistParticle(bloodSpawnPosition, Main.rand.NextVector2Circular(8f, 6f) + Vector2.UnitY * 2.9f, bloodColor, 0.5f, 0f, 45, 0f, true);
            blood.Spawn();
            for (int i = 0; i < animationInterpolant * 3f; i++)
            {
                bloodColor = Color.Lerp(new(255, 36, 0), new(73, 10, 2), Main.rand.NextFloat(0.15f, 0.7f));
                BloodParticle blood2 = new BloodParticle(bloodSpawnPosition + Main.rand.NextVector2Circular(80f, 50f), Main.rand.NextVector2Circular(4f, 3f) - Vector2.UnitY * 2f, 30, Main.rand.NextFloat(1.25f), bloodColor);
                blood2.Spawn();
            }
        }

        // Enter the foreground.
        ZPosition = Lerp(ZPosition, 0f, 0.156f);

        // Create a blood splatter in the background if emerging from behind the active fountain.
        if (ZPosition >= 0.6f && VisceralBackgroundBloodFountainOpacity >= 0.3f)
            CreateBackgroundBloodSplatter(145, 1f, 1.25f);

        // Make the head dangle.
        PerformBasicHeadUpdates(1.3f);
        HeadPosition += Main.rand.NextVector2Circular(6f, 2.5f);

        // Make the Avatar put his hands near his head.
        if (AITimer < Unclog_WeepStartTime)
        {
            float stretchInterpolant = EasingCurves.Quintic.Evaluate(EasingType.InOut, InverseLerp(0f, Unclog_WeepStartTime, AITimer).Squared());
            float stretchOffset = Lerp(100f + Main.rand.NextFloatDirection() * 40f, 850f, stretchInterpolant);

            Vector2 leftArmDestination = HeadPosition + new Vector2(-stretchOffset, 300f) * NPC.scale * RightFrontArmScale;
            Vector2 rightArmDestination = HeadPosition + new Vector2(stretchOffset, 300f) * NPC.scale * RightFrontArmScale;
            LeftArmPosition = Vector2.Lerp(LeftArmPosition, leftArmDestination, 0.23f) + Main.rand.NextVector2Circular(6f, 2f) * animationInterpolant;
            RightArmPosition = Vector2.Lerp(RightArmPosition, rightArmDestination, 0.23f) + Main.rand.NextVector2Circular(6f, 2f) * animationInterpolant;

            // Shake in place.
            NPC.velocity += Main.rand.NextVector2CircularEdge(1f, 0.4f) * stretchInterpolant * 7f;

            // Shake the screen.
            ScreenShakeSystem.SetUniversalRumble(stretchInterpolant * 6.7f, TwoPi, null, 0.2f);
        }

        else
        {
            // Create blood from the arms.
            CreateArmBloodFountain();

            // Make the spider lily glow.
            LilyGlowIntensityBoost = Clamp(LilyGlowIntensityBoost + 0.05f, 0f, 1.1f);

            // Create a fountain of outward spewing blood.
            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % Unclog_BlobSpawnRate == 0)
            {
                Vector2 bloodShootVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(9f, 15f);
                NewProjectileBetter(NPC.GetSource_FromAI(), SpiderLilyPosition + Vector2.UnitY * 150f, bloodShootVelocity, ModContent.ProjectileType<BloodBlob>(), BloodBlobDamage, 0f, -1, 1f, 0f, -0.06f);
            }

            // Periodically release a single, non-gravity-affected blood blob towards the target, to ensures that the player cannot sit in one spot for
            // long periods of time.
            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % 60 == 59)
            {
                ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(blob =>
                {
                    blob.As<BloodBlob>().GravityUnaffected = true;
                });

                Vector2 bloodSpawnPosition = SpiderLilyPosition + Vector2.UnitY * 150f;
                Vector2 bloodShootVelocity = bloodSpawnPosition.SafeDirectionTo(Target.Center) * 13f;
                NewProjectileBetter(NPC.GetSource_FromAI(), bloodSpawnPosition, bloodShootVelocity, ModContent.ProjectileType<BloodBlob>(), BloodBlobDamage, 0f, -1, 1f, 0f, -0.06f);
            }

            PerformBasicFrontArmUpdates(0.3f, Vector2.UnitY * 500f, Vector2.UnitY * 500f);
        }
    }

    public void CreateBackgroundBloodSplatter(int particleCount = 145, float speedFactor = 1f, float splatterArc = 1.25f)
    {
        if (Main.netMode == NetmodeID.Server || !AvatarOfEmptinessSky.SkyTarget.DownscaledTarget.TryGetTarget(0, out RenderTarget2D? skyTarget) || skyTarget is null)
            return;

        Vector2 screenSize = skyTarget.Size();
        for (int i = 0; i < particleCount; i++)
        {
            Color splatterColor = Color.Lerp(Color.DarkRed, Color.Crimson, Main.rand.NextFloat(0.5f));
            Vector2 splatterPosition = new Vector2(screenSize.X * 0.5f + Main.rand.NextFloatDirection() * 65f, screenSize.Y * 0.54f + Main.rand.NextFloatDirection() * 80f);
            Vector2 splatterVelocity = -Vector2.UnitY.RotatedByRandom(splatterArc) * Main.rand.NextFloat(16f, 100f) * speedFactor;
            splatterVelocity.Y = -Abs(splatterVelocity.Y);
            splatterVelocity *= Sqrt(ZPositionScale) * 1.67f;

            VisceraDimensionParticleSystem.CreateNew(splatterPosition, splatterVelocity, splatterColor, Main.rand.Next(11, 30));
        }
    }

    public void CreateArmBloodFountain()
    {
        for (int i = 0; i < 12; i++)
        {
            float bloodScale = Main.rand.NextFloat(0.6f, 1.5f);
            Color bloodColor = Color.Lerp(new(Main.rand.Next(115, 233), 0, 0), new(76, 24, 60), Main.rand.NextFloat(0.4f));
            Vector2 bloodVelocity = new Vector2(Main.rand.NextFloat(7f, 37.5f), -Main.rand.NextFloat(6f, 7f));
            if (Main.rand.NextBool())
                bloodVelocity = Vector2.Reflect(bloodVelocity, -Vector2.UnitX);
            Vector2 bloodSpawnPosition = SpiderLilyPosition + bloodVelocity * 4f;

            if (Main.rand.NextBool(3))
            {
                bloodVelocity += Main.rand.NextVector2Circular(10f, 30f);

                BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
                metaball.CreateParticle(SpiderLilyPosition, bloodVelocity * 0.85f, Main.rand.NextFloat(30f, 124f), Main.rand.NextFloat());
            }
            else
            {
                BloodParticle blood = new BloodParticle(bloodSpawnPosition, bloodVelocity, 56, bloodScale * Main.rand.NextFloat(1f, 2f), bloodColor);
                blood.Spawn();
            }
        }
    }
}
