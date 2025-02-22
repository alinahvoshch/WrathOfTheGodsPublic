using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers.CosmicBackgroundSystem;
using static NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers.NamelessDeitySky;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    /// <summary>
    /// Whether Nameless has performed his Moment of Creation attack yet or not. If he hasn't, he will perform it before he is defeated.
    /// </summary>
    public bool HasExperiencedFinalAttack
    {
        get;
        set;
    }

    /// <summary>
    /// How long spends entering the background during his Moment of Creation state.
    /// </summary>
    public static int MomentOfCreation_BackgroundEnterTime => GetAIInt("MomentOfCreation_BackgroundEnterTime");

    /// <summary>
    /// How long Nameless waits before releasing galaxies during his Moment of Creation state.
    /// </summary>
    public static int MomentOfCreation_GalaxySpawnDelay => GetAIInt("MomentOfCreation_GalaxySpawnDelay");

    /// <summary>
    /// How long Nameless spends releasing galaxies during his Moment of Creation state.
    /// </summary>
    public static int MomentOfCreation_GalaxyShootTime => GetAIInt("MomentOfCreation_GalaxyShootTime");

    /// <summary>
    /// How long it takes for Nameless' flower to disappear during his Moment of Creation state.
    /// </summary>
    public static int MomentOfCreation_FlowerDisappearDelay => GetAIInt("MomentOfCreation_FlowerDisappearDelay");

    /// <summary>
    /// The baseline galaxy release rate at the start of Nameless' Moment of Creation state.
    /// </summary>
    public static int MomentOfCreation_StartingGalaxyReleaseRate => (int)Clamp(GetAIInt("MomentOfCreation_StartingGalaxyReleaseRate") / Myself_DifficultyFactor, 6f, 1000f);

    /// <summary>
    /// The lowest amount that Nameless' galaxy shoot rate can reach during his Moment of Creation state.
    /// </summary>
    public static int MomentOfCreation_MinGalaxyReleaseRate => (int)Clamp(GetAIInt("MomentOfCreation_MinGalaxyReleaseRate") / Myself_DifficultyFactor, 1f, 1000f);

    /// <summary>
    /// How long Nameless' Moment of Creation state goes on for overall.
    /// </summary>
    public static int MomentOfCreation_AttackDuration => DivineRoseSystem.ExplosionDelay + MomentOfCreation_GalaxySpawnDelay + MomentOfCreation_GalaxyShootTime + MomentOfCreation_FlowerDisappearDelay + 75;

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_MomentOfCreation()
    {
        // Load the transition from MomentOfCreation to the next in the cycle.
        StateMachine.RegisterTransition(NamelessAIType.MomentOfCreation, null, false, () =>
        {
            return AITimer >= MomentOfCreation_AttackDuration;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.MomentOfCreation, DoBehavior_MomentOfCreation);
    }

    public void DoBehavior_MomentOfCreation()
    {
        int backgroundEnterTime = MomentOfCreation_BackgroundEnterTime;
        int starRecedeTime = DivineRoseSystem.BlackOverlayStartTime - 10;
        int fingerSnapDelay = DivineRoseSystem.AttackDelay;
        int explosionDelay = DivineRoseSystem.ExplosionDelay;
        int galaxySpawnDelay = MomentOfCreation_GalaxySpawnDelay;
        int galaxyShootTime = MomentOfCreation_GalaxyShootTime;
        float idealZPosition = DivineRoseSystem.NamelessDeityZPosition;
        float maxStarZoomout = 0.5f;

        ref float galaxyReleaseTimer = ref NPC.ai[2];
        ref float galaxyReleaseCounter = ref NPC.ai[3];
        int galaxyReleaseRate = (int)Clamp(MomentOfCreation_StartingGalaxyReleaseRate - galaxyReleaseCounter * 3f, MomentOfCreation_MinGalaxyReleaseRate, MomentOfCreation_StartingGalaxyReleaseRate);

        if (Hands.Count < 4)
            ConjureHandsAtPosition(NPC.Center, Vector2.Zero);

        // Flap wings.
        UpdateWings(AITimer / 45f);

        // Make the stars return.
        KaleidoscopeInterpolant = 1f - InverseLerpBump(0f, 20f, explosionDelay + galaxySpawnDelay + galaxyShootTime + MomentOfCreation_FlowerDisappearDelay, explosionDelay + galaxySpawnDelay + galaxyShootTime + MomentOfCreation_FlowerDisappearDelay + 25f, AITimer);

        // Make the background stars recede.
        float starRecedeInterpolant = InverseLerp(0f, starRecedeTime, AITimer);
        if (starRecedeInterpolant < 1f)
        {
            StarZoomIncrement = Pow(starRecedeInterpolant, 7f) * maxStarZoomout;
            HeavenlyBackgroundIntensity = Lerp(1f, 0.04f, Pow(starRecedeInterpolant, 2f));
        }
        else
        {
            StarZoomIncrement = maxStarZoomout;

            if (TotalScreenOverlaySystem.OverlayInterpolant <= 0f)
                HeavenlyBackgroundIntensity *= 0.9f;
            if (HeavenlyBackgroundIntensity < 0.004f)
            {
                StarZoomIncrement = 0f;
                HeavenlyBackgroundIntensity = 0.00001f;
            }
        }

        // Enter the background and fly around the rose.
        float movementSharpness = 0.1f;
        float movementSmoothness = 0.82f;
        Vector2 spinOffset = Vector2.UnitY.RotatedBy(TwoPi * ZPosition * AITimer / 800f) * new Vector2(400f, 300f) * (0.6f + ZPosition * 0.06f);
        Vector2 hoverDestination = Target.Center + DivineRoseSystem.RoseOffsetFromScreenCenter + spinOffset;
        if (AITimer >= fingerSnapDelay - 30f)
        {
            hoverDestination = Target.Center + DivineRoseSystem.RoseOffsetFromScreenCenter + Vector2.UnitY * 120f;
            movementSharpness = 0.3f;
            movementSmoothness = 0.5f;
        }

        // Perform Z position and hover movement.
        if (AITimer < explosionDelay + galaxySpawnDelay + galaxyShootTime + MomentOfCreation_FlowerDisappearDelay)
        {
            ZPosition = Pow(Saturate(AITimer / (float)backgroundEnterTime), 1.6f) * idealZPosition;
            NPC.SmoothFlyNear(hoverDestination, movementSharpness, movementSmoothness);
        }

        // Darken the screen when ready.
        if (AITimer == DivineRoseSystem.BlackOverlayStartTime - 4)
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.Glitch with { PitchVariance = 0.2f, Volume = 1.3f, MaxInstances = 8 });
        if (AITimer >= DivineRoseSystem.BlackOverlayStartTime - 4 && AITimer <= DivineRoseSystem.BlackOverlayStartTime + 6)
        {
            TotalScreenOverlaySystem.OverlayInterpolant = 1.6f;
            TotalScreenOverlaySystem.OverlayColor = Color.Black;
        }

        // Make the rose explode into a bunch of galaxies.
        if (AITimer == explosionDelay)
        {
            Vector2 censorPosition = Target.Center + DivineRoseSystem.RoseOffsetFromScreenCenter + DivineRoseSystem.BaseCensorOffset;

            ScreenShakeSystem.StartShake(20f);
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.MomentOfCreation with { Volume = 2f });
            GeneralScreenEffectSystem.ChromaticAberration.Start(censorPosition, 1f, 120);
            NamelessDeityKeyboardShader.BrightnessIntensity = 1f;

            if (Main.netMode == NetmodeID.SinglePlayer)
                NewProjectileBetter(NPC.GetSource_FromAI(), censorPosition, Vector2.Zero, ModContent.ProjectileType<LightWave>(), 0, 0f);
        }

        // Perform galaxy spawning and timing behaviors after the explosion has happened.
        if (AITimer >= explosionDelay + galaxySpawnDelay && AITimer < explosionDelay + galaxySpawnDelay + galaxyShootTime)
        {
            galaxyReleaseTimer++;

            // Create galaxies that fall from above.
            if (galaxyReleaseTimer >= galaxyReleaseRate)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.GalaxyTelegraph with { MaxInstances = 10, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew });
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float horizontalOffset = Main.rand.NextFloatDirection() * 900f;
                    if (Main.rand.NextBool(8))
                        horizontalOffset = 0f;

                    Vector2 telegraphSpawnPosition = Target.Center + Vector2.UnitX * (horizontalOffset + Target.Velocity.X * Main.rand.NextFloat(30f, 45f));
                    NewProjectileBetter(NPC.GetSource_FromAI(), telegraphSpawnPosition, Vector2.UnitY, ModContent.ProjectileType<FallingGalaxy>(), GalaxyDamage, 0f);
                }

                galaxyReleaseTimer = 0f;
                galaxyReleaseCounter++;
                NPC.netUpdate = true;
            }
        }

        // Make the flower disappear when done shooting.
        if (AITimer == explosionDelay + galaxySpawnDelay + galaxyShootTime + 75f)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.Glitch with { PitchVariance = 0.2f, Volume = 1.3f, MaxInstances = 8 });
            TotalScreenOverlaySystem.OverlayInterpolant = 1.7f;
            TotalScreenOverlaySystem.OverlayColor = Color.Black;
            ZPosition = 0f;
            NPC.Center = Target.Center - Vector2.UnitY * 350f;
            NPC.velocity = Vector2.Zero;
            NPC.netUpdate = true;
        }

        if (AITimer >= explosionDelay + galaxySpawnDelay + galaxyShootTime + MomentOfCreation_FlowerDisappearDelay)
        {
            HeavenlyBackgroundIntensity = InverseLerp(75f, 145f, AITimer - explosionDelay - galaxySpawnDelay - galaxyShootTime);
            StarZoomIncrement = 0f;
            KaleidoscopeInterpolant = Saturate(KaleidoscopeInterpolant + 0.05f);
        }

        // Mumble before snapping fingers.
        if (AITimer == fingerSnapDelay - 50f)
            PerformMumble();

        // Update hands.
        if (Hands.Count >= 4)
        {
            Vector2[] handOffsets =
            [
                new(-1080f, -500f),
                new(1080f, -500f),
                new(900f, 120f),
                new(-900f, 120f)
            ];

            for (int i = 0; i < handOffsets.Length; i++)
            {
                // Apply randomness to the hand offsets.
                ulong seed = (ulong)(i + NPC.whoAmI * 717);
                float randomOffset = Lerp(30f, 108f, Utils.RandomFloat(ref seed));
                Vector2 randomDirection = (Utils.RandomFloat(ref seed) * TwoPi * 3f + FightTimer * (i - 3.4f) / 7f).ToRotationVector2() * new Vector2(1.61f, 1.1f);
                handOffsets[i] += randomDirection * randomOffset;

                // Move hands.
                DefaultHandDrift(Hands[i], NPC.Center + handOffsets[i], 6f);
            }

            // Snap fingers and make the screen shake.
            if (AITimer == fingerSnapDelay)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.FingerSnap with
                {
                    Volume = 4f
                });
                ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 9f);
            }
        }
    }
}
