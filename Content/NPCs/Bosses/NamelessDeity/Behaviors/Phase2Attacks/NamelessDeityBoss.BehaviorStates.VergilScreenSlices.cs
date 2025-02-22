using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.Graphics.ScreenShatter;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers.NamelessDeitySky;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    /// <summary>
    /// How long Nameless waits before creating slices during his Vergil Screen Slices state.
    /// </summary>
    public static int VergilScreenSlices_SliceShootDelay => GetAIInt("VergilScreenSlices_SliceShootDelay");

    /// <summary>
    /// The rate at which Nameless releases slices during his Vergil Screen Slices state.
    /// </summary>
    public static int VergilScreenSlices_SliceReleaseRate => (int)Clamp(GetAIInt("VergilScreenSlices_SliceReleaseRate") / Myself_DifficultyFactor, 1f, 1000f);

    /// <summary>
    /// The amount of slices Nameless releases during his Vergil Screen Slices state.
    /// </summary>
    public static int VergilScreenSlices_SliceReleaseCount => (int)Clamp(GetAIInt("VergilScreenSlices_SliceReleaseCount") + (Myself_DifficultyFactor - 1f) * 6f, 0f, 29f);

    /// <summary>
    /// How long Nameless spends releasing slices during his Vergil Screen Slices state.
    /// </summary>
    public static int VergilScreenSlices_SliceReleaseTime => VergilScreenSlices_SliceReleaseRate * VergilScreenSlices_SliceReleaseCount + 25;

    /// <summary>
    /// How long Nameless waits before attacking during his Vergil Screen Slices state.
    /// </summary>
    public static int VergilScreenSlices_AttackDelay => GetAIInt("VergilScreenSlices_AttackDelay");

    /// <summary>
    /// How long Nameless waits after attacking to transition to his next attack during his Vergil Screen Slices state.
    /// </summary>
    public static int VergilScreenSlices_AttackTransitionDelay => GetAIInt("VergilScreenSlices_AttackTransitionDelay");

    /// <summary>
    /// How long Nameless' Vergil Screen Slices state goes on for overall.
    /// </summary>
    public static int VergilScreenSlices_AttackDuration => VergilScreenSlices_SliceShootDelay + VergilScreenSlices_SliceReleaseTime + VergilScreenSlices_AttackDelay + VergilScreenSlices_AttackTransitionDelay;

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_VergilScreenSlices()
    {
        // Load the transition from VergilScreenSlices to the next in the cycle.
        StateMachine.RegisterTransition(NamelessAIType.VergilScreenSlices, null, false, () =>
        {
            return AITimer >= VergilScreenSlices_AttackDuration;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.VergilScreenSlices, DoBehavior_VergilScreenSlices);
    }

    public void DoBehavior_VergilScreenSlices()
    {
        int sliceShootDelay = VergilScreenSlices_SliceShootDelay;
        int sliceReleaseRate = VergilScreenSlices_SliceReleaseRate;
        int sliceReleaseCount = VergilScreenSlices_SliceReleaseCount;
        int sliceReleaseTime = VergilScreenSlices_SliceReleaseTime;
        int fireDelay = VergilScreenSlices_AttackDelay;
        int teleportDelay = 15;
        float sliceLength = 3200f;
        ref float sliceCounter = ref NPC.ai[2];

        // Enter the foreground.
        ZPosition *= 0.8f;

        // Flap wings.
        UpdateWings(AITimer / 48f);

        // Update universal hands.
        DefaultUniversalHandMotion();

        if (AITimer <= sliceShootDelay)
        {
            // Play mumble sounds.
            if (AITimer == 1f)
                PerformMumble();

            // Hover above the target at first.
            Vector2 hoverDestination = Target.Center - Vector2.UnitY * 442f;
            NPC.velocity = (hoverDestination - NPC.Center) * 0.1f;

            // Use teleport visuals.
            if (AITimer >= sliceShootDelay - teleportDelay)
            {
                if (AITimer == sliceShootDelay - teleportDelay + 1f)
                    SoundEngine.PlaySound(GennedAssets.Sounds.Common.TeleportIn with { Volume = 0.65f, MaxInstances = 5, PitchVariance = 0.16f }, NPC.Center);

                TeleportVisualsInterpolant = InverseLerp(sliceShootDelay - teleportDelay, sliceShootDelay - 1f, AITimer) * 0.5f;
            }

            // Teleport away after hovering.
            if (AITimer == sliceShootDelay - 1f)
            {
                RadialScreenShoveSystem.Start(EyePosition, 45);
                ImmediateTeleportTo(Target.Center + Vector2.UnitY * 2000f);
            }

            // Dim the background for suspense.
            HeavenlyBackgroundIntensity = Clamp(HeavenlyBackgroundIntensity - 0.032f, 0.5f, 1f);

            return;
        }

        // Reset the teleport visuals interpolant after the teleport has concluded.
        TeleportVisualsInterpolant = 0f;

        // Stay invisible.
        NPC.Opacity = 0f;

        // Release slice telegraphs around the player.
        if (AITimer % sliceReleaseRate == 0f && sliceCounter < sliceReleaseCount)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.SliceTelegraph with { Volume = 1.05f, MaxInstances = 20 });
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int telegraphTime = sliceReleaseTime - (int)(AITimer - sliceShootDelay) + (int)sliceCounter * 2 + fireDelay;
                Vector2 sliceSpawnCenter = Target.Center + Main.rand.NextVector2Unit() * (sliceCounter + 35f + Main.rand.NextFloat(600f)) + Target.Velocity * 8f;
                if (sliceCounter == 0f)
                    sliceSpawnCenter = Target.Center + Main.rand.NextVector2Circular(10f, 10f);

                Vector2 sliceDirection = new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-6f, 6f)).SafeNormalize(Vector2.UnitX);
                NPC.NewProjectileBetter(NPC.GetSource_FromAI(), sliceSpawnCenter - sliceDirection * sliceLength * 0.5f, sliceDirection, ModContent.ProjectileType<VergilScreenSlice>(), ScreenSliceDamage, 0f, -1, telegraphTime, sliceLength);
            }

            sliceCounter++;
        }

        // Slice the screen.
        if (AITimer == sliceShootDelay + sliceReleaseTime + fireDelay)
        {
            // Calculate the center of the slices.
            List<LineSegment> lineSegments = [];
            List<Projectile> slices = AllProjectilesByID(ModContent.ProjectileType<VergilScreenSlice>()).ToList();
            for (int i = 0; i < slices.Count; i++)
            {
                Vector2 start = slices[i].Center;
                Vector2 end = start + slices[i].velocity * slices[i].As<VergilScreenSlice>().LineLength;
                lineSegments.Add(new(start, end));
            }

            ScreenShatterSystem.CreateShatterEffect([.. lineSegments]);
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.GenericBurst with { Volume = 1.3f, PitchVariance = 0.15f });
            NamelessDeityKeyboardShader.BrightnessIntensity = 1f;
            ScreenShakeSystem.StartShake(15f);
        }

        // Make the background come back.
        if (AITimer >= sliceShootDelay + sliceReleaseTime + fireDelay)
        {
            HeavenlyBackgroundIntensity = Saturate(HeavenlyBackgroundIntensity + 0.08f);
            RadialScreenShoveSystem.Start(Target.Center - Vector2.UnitY * 400f, 20);
            NPC.Opacity = 1f;
        }

        // Stay invisible.
        NPC.Center = Target.Center + Vector2.UnitY * 2000f;
    }
}
