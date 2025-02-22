using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    /// <summary>
    /// How long Nameless spends rising upward and entering the background during his Reality Tear Daggers state.
    /// </summary>
    public static int RealityTearDaggers_RiseTime => GetAIInt("RealityTearDaggers_RiseTime");

    /// <summary>
    /// How long Nameless spends telegraphing his screen slices during his Reality Tear Daggers state.
    /// </summary>
    public static int RealityTearDaggers_SliceTelegraphTime => GetAIInt("RealityTearDaggers_SliceTelegraphTime");

    /// <summary>
    /// How many horizonal screen slices should be performed during Nameless' Reality Tear Daggers state.
    /// </summary>
    public static int RealityTearDaggers_TotalHorizontalSlices => GetAIInt("RealityTearDaggers_TotalHorizontalSlices");

    /// <summary>
    /// How many radial screen slices should be performed during Nameless' Reality Tear Daggers state.
    /// </summary>
    /// 
    /// <remarks>
    /// These occur after all horizontal slices have finished.
    /// </remarks>
    public static int RealityTearDaggers_TotalRadialSlices => GetAIInt("RealityTearDaggers_TotalRadialSlices");

    /// <summary>
    /// How long Nameless spends reorienting his hands during his Reality Tear Daggers state.
    /// </summary>
    public static int RealityTearDaggers_HandMoveTime => GetAIInt("RealityTearDaggers_HandMoveTime");

    /// <summary>
    /// The amount of hands Nameless uses during his Reality Tear Daggers state.
    /// </summary>
    public static int RealityTearDaggers_TotalHands => GetAIInt("RealityTearDaggers_TotalHands");

    /// <summary>
    /// How long Nameless waits after attacking to transition to his next attack during his Reality Tear Daggers state.
    /// </summary>
    public static int RealityTearDaggers_AttackTransitionDelay => GetAIInt("RealityTearDaggers_AttackTransitionDelay");

    /// <summary>
    /// The length of the screen slice telegraph during Nameless' Reality Tear Daggers state.
    /// </summary>
    public static float RealityTearDaggers_SliceTelegraphLength => GetAIFloat("RealityTearDaggers_SliceTelegraphLength");

    /// <summary>
    /// The amount of screen slices performed so far during Nameless' Reality Tear Daggers state.
    /// </summary>
    public ref float RealityTearDaggers_SliceCounter => ref NPC.ai[2];

    /// <summary>
    /// The timer used during Nameless' Reality Tear Daggers attack concludes.
    /// </summary>
    public ref float RealityTearDaggers_AttackTransitionTimer => ref NPC.ai[3];

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_RealityTearDaggers()
    {
        // Load the transition from RealityTearDaggers to the next in the cycle.
        StateMachine.RegisterTransition(NamelessAIType.RealityTearDaggers, null, false, () =>
        {
            return RealityTearDaggers_AttackTransitionTimer >= RealityTearDaggers_AttackTransitionDelay / DifficultyFactor;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.RealityTearDaggers, DoBehavior_RealityTearDaggers);
    }

    public void DoBehavior_RealityTearDaggers()
    {
        int riseTime = RealityTearDaggers_RiseTime;
        int sliceTelegraphTime = (int)Clamp(RealityTearDaggers_SliceTelegraphTime / DifficultyFactor, 5f, 1000f);
        int totalHorizontalSlices = RealityTearDaggers_TotalHorizontalSlices;
        int totalRadialSlices = RealityTearDaggers_TotalRadialSlices;
        int handMoveTime = RealityTearDaggers_HandMoveTime;
        int totalHands = RealityTearDaggers_TotalHands;
        int screenSliceRate = sliceTelegraphTime + 11;
        int totalSlices = totalHorizontalSlices + totalRadialSlices;
        float sliceTelegraphLength = RealityTearDaggers_SliceTelegraphLength;
        float wrappedAttackTimer = AITimer % screenSliceRate;
        ref float sliceCounter = ref RealityTearDaggers_SliceCounter;

        // Calculate slice information.
        Vector2 sliceDirection = Vector2.UnitX;
        Vector2 sliceSpawnOffset = Vector2.Zero;
        if (sliceCounter % totalSlices >= totalHorizontalSlices)
        {
            sliceDirection = sliceDirection.RotatedBy(TwoPi * (sliceCounter - totalHorizontalSlices) / totalRadialSlices);
            sliceSpawnOffset += sliceDirection.RotatedBy(PiOver2) * 400f;
        }

        // Disallow arm variant 4 for this attack because the angles look weird.
        if ((RenderComposite.Find<ArmsStep>().HandTexture?.TextureName ?? string.Empty) == "Hand4")
            RenderComposite.Find<ArmsStep>().HandTexture?.Swap();

        // Flap wings.
        UpdateWings(AITimer / 45f);

        // Move into the background.
        if (AITimer <= riseTime)
            ZPosition = Pow(AITimer / (float)riseTime, 1.6f) * 2.4f;

        // Calculate the background hover position.
        float hoverHorizontalWaveSine = Sin(TwoPi * AITimer / 96f);
        float hoverVerticalWaveSine = Sin(TwoPi * AITimer / 120f);
        Vector2 hoverDestination = Target.Center + new Vector2(Target.Velocity.X * 14.5f, ZPosition * -40f - 200f);
        hoverDestination.X += hoverHorizontalWaveSine * ZPosition * 40f;
        hoverDestination.Y -= hoverVerticalWaveSine * ZPosition * 8f;
        if (Main.zenithWorld)
            hoverDestination += Main.rand.NextVector2Circular(1200f, 500f);

        // Stay above the target while in the background.
        NPC.SmoothFlyNear(hoverDestination, 0.07f, 0.94f);

        // Create hands.
        if (AITimer == 1f)
        {
            for (int i = TotalUniversalHands; i < totalHands; i++)
            {
                Vector2 handOffset = (TwoPi * i / totalHands).ToRotationVector2() * NPC.scale * 400f;
                if (Abs(handOffset.X) <= 0.001f)
                    handOffset.X = 0f;

                ConjureHandsAtPosition(NPC.Center + handOffset, sliceDirection * 3f);
            }

            // Play mumble sounds.
            PerformMumble();
        }

        // Operate hands that move in the direction of the slice.
        if (Hands.Count != 0)
        {
            // Update the hands.
            for (int i = 0; i < Hands.Count; i++)
            {
                // Hands extend outwards shortly before a barrage of daggers is fired.
                float handExtendInterpolant = InverseLerp(14f, 24f, wrappedAttackTimer - sliceTelegraphTime);
                if (wrappedAttackTimer - sliceTelegraphTime <= -7f && sliceCounter >= 1f)
                    handExtendInterpolant = 1f;

                float handExtendFactor = Lerp(2.6f, 1.46f, handExtendInterpolant);
                float handDriftSpeed = Utils.Remap(ZPosition, 1.1f, 0.36f, 1.9f, 0.8f);

                NamelessDeityHand hand = Hands[i];

                // Group hands such that they prefer being to the sides before moving upward in a commanding pose when daggers are about to fire.
                Vector2 hoverOffset = new Vector2((i % 2 == 0).ToDirectionInt() * 500f, 30f) * handExtendFactor;
                hoverOffset.X = Lerp(hoverOffset.X, hoverOffset.X.NonZeroSign() * 150f, handExtendInterpolant * 0.48f);
                hoverOffset.Y -= handExtendInterpolant * 750f - 60f;

                int sideIndependentIndex = i / 2;
                if (sideIndependentIndex != (int)(sliceCounter % (totalHands / 2)))
                {
                    float verticalOffset = Cos01(AITimer / 12f) * (sideIndependentIndex * 80f + 150f);
                    hoverOffset = new Vector2((i % 2 == 0).ToDirectionInt() * (900f - sideIndependentIndex * 90f), 50f - sideIndependentIndex * 150f - verticalOffset);
                    hoverOffset = Vector2.Lerp(hoverOffset, -Vector2.UnitY * 600f, sideIndependentIndex * 0.1f);
                }

                // Moves the hands to their end position.
                Vector2 handDestination = NPC.Center + hoverOffset;
                hand.FreeCenter = Vector2.Lerp(hand.FreeCenter, handDestination, wrappedAttackTimer / handMoveTime * Sqrt(handExtendInterpolant) * 0.6f + 0.02f);

                // Perform update code.
                DefaultHandDrift(hand, handDestination, handDriftSpeed);
            }
        }

        // Create slices.
        if (wrappedAttackTimer == Math.Min(handMoveTime, screenSliceRate - 1) && AITimer >= riseTime + 1f && sliceCounter < totalSlices)
        {
            // Create a reality tear.
            float sliceOffset = 28f;
            if (sliceCounter >= totalHorizontalSlices)
            {
                sliceDirection = sliceDirection.RotatedBy(Pi / totalRadialSlices);
                sliceOffset = 0f;
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 telegraphSpawnPosition = Target.Center - sliceDirection * sliceTelegraphLength * 0.5f + sliceSpawnOffset;
                NewProjectileBetter(NPC.GetSource_FromAI(), telegraphSpawnPosition, sliceDirection, ModContent.ProjectileType<TelegraphedScreenSlice>(), ScreenSliceDamage, 0f, -1, sliceTelegraphTime, sliceTelegraphLength, sliceOffset);
            }

            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.RealityTear with { Volume = 0.56f, PitchVariance = 0.24f, MaxInstances = 10 });
            NamelessDeityKeyboardShader.BrightnessIntensity += 0.6f;

            sliceCounter++;
            NPC.netUpdate = true;
        }

        // Return to the foreground and destroy hands after doing the slices.
        if (sliceCounter >= totalSlices)
        {
            ZPosition = Lerp(ZPosition, 0f, DifficultyFactor * 0.06f);

            // Destroy the hands after enough time has passed.
            if (RealityTearDaggers_AttackTransitionTimer == (int)(25f / DifficultyFactor))
                DestroyAllHands();

            RealityTearDaggers_AttackTransitionTimer++;
        }
    }
}
