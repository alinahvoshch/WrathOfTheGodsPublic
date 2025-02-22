using Luminance.Common.Easings;
using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using NoxusBoss.Core.DataStructures;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    /// <summary>
    /// The amount of time the Avatar spends waiting after his pent up energy is released during the erasure attack.
    /// </summary>
    public static int Erasure_WaitTime => GetAIInt("Erasure_WaitTime");

    /// <summary>
    /// The distortion intensity during the erasure attack. The greater this is, the more restrictions applied to the player in terms of free movement.
    /// </summary>
    public static DifficultyValue<float> Erasure_IdealDistortionIntensity => new(0.99f);

    /// <summary>
    /// The amount of time the Avatar spends charging up energy during the erasure attack.
    /// </summary>
    public static int Erasure_ChargeUpTime => GetAIInt("Erasure_ChargeUpTime");

    /// <summary>
    /// The amount of damage the Avatar's void blots do.
    /// </summary>
    public static int VoidBlotDamage => GetAIInt("VoidBlotDamage");

    [AutomatedMethodInvoke]
    public void LoadState_Erasure()
    {
        StateMachine.RegisterTransition(AvatarAIType.Erasure, null, false, () =>
        {
            return AITimer >= Erasure_ChargeUpTime + Erasure_WaitTime;
        }, () => DesiredDistortionCenterOverride = null);

        // Load the AI state behaviors.
        StateMachine.RegisterStateBehavior(AvatarAIType.Erasure, DoBehavior_Erasure);

        StatesToNotStartTeleportDuring.Add(AvatarAIType.Erasure);
    }

    public void DoBehavior_Erasure()
    {
        LookAt(Target.Center);

        float startingHeadVerticalOffset = 400f;
        float endingHeadVerticalOffset = 343f;
        float maxArmHorizontalOffset = 250f;

        // Calculate body part variables.
        float headVerticalOffset = endingHeadVerticalOffset;
        float shadowHandRotationOffset = 0f;
        float shadowArmJitterMaxAngle = 0.25f;
        Vector2 leftArmHoverOffset = Vector2.Zero;
        Vector2 rightArmHoverOffset = Vector2.Zero;
        Vector2 shadowArmHoverOffset = Vector2.Zero;

        // Decide the distortion intensity.
        IdealDistortionIntensity = Erasure_IdealDistortionIntensity;

        // Teleport before doing anything else if far frrom the target.
        if (AITimer == 2 && !NPC.WithinRange(Target.Center, 780f))
        {
            StartTeleportAnimation(() =>
            {
                return Target.Center - Vector2.UnitY * 500f;
            });
        }

        if (AITimer <= 15 && ShadowArms.Count < 4)
            ShadowArms.Add(new(NPC.Center, Vector2.Zero));

        SolynAction = s => StandardSolynBehavior_FlyNearPlayer(s, NPC);

        // Set the distortion center on the first frame.
        if (AITimer == 3)
        {
            DesiredDistortionCenterOverride = NPC.Center;
            NPC.netUpdate = true;
        }

        // Perform charge up effects.
        if (AITimer < Erasure_ChargeUpTime)
        {
            // Calculate the charge up interpolant.
            float chargeUpCompletion = InverseLerp(0f, Erasure_ChargeUpTime, AITimer);

            // Update head motion.
            headVerticalOffset = Lerp(startingHeadVerticalOffset, endingHeadVerticalOffset, chargeUpCompletion.Squared());

            // Apply horizontal offsets to the arms.
            float armHorizontalOffset = Pow(chargeUpCompletion, 1.5f) * maxArmHorizontalOffset;
            leftArmHoverOffset.X += armHorizontalOffset;
            rightArmHoverOffset.X -= armHorizontalOffset;

            // Shake in place in anticipation of the release of erasure projectiles.
            NPC.position.Y -= (1f - chargeUpCompletion) * 4f;
            NPC.Center += Main.rand.NextVector2Circular(6f, 4f) * chargeUpCompletion.Cubed();
            NPC.velocity *= 0.82f;

            // Shake the screen.
            ScreenShakeSystem.SetUniversalRumble(chargeUpCompletion * 5f, TwoPi, null, 0.2f);

            // Enter the background.
            ZPosition = MathF.Max(ZPosition, EasingCurves.Quadratic.Evaluate(EasingType.InOut, chargeUpCompletion));
            if (ZPosition > 1f)
                ZPosition = Lerp(ZPosition, 1f, 0.14f);
        }

        // Explode and release erasure blobs.
        else
        {
            // Create explosion visuals and sounds.
            if (AITimer == Erasure_ChargeUpTime + 1)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Angry with { Volume = 0.7f });
                ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 12f);
            }

            // Summon blots around target.
            if (AITimer < Erasure_ChargeUpTime + 26)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 blotFocusPoint = Vector2.Lerp(NPC.Center, Target.Center, 0.9f);
                        Vector2 blotSpawnDirection = Vector2.Lerp(Main.rand.NextVector2Unit(), Target.Velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFromList(-1f, 1f), 0.36f).SafeNormalize(Vector2.UnitY);
                        Vector2 blotSpawnPosition = blotFocusPoint + blotSpawnDirection * Main.rand.NextFloat(200f, 1796f);
                        if (AITimer < Erasure_ChargeUpTime + 4)
                            blotSpawnPosition = Target.Center + Main.rand.NextVector2Circular(400f, 400f) + Target.Velocity * 25f;

                        NewProjectileBetter(NPC.GetSource_FromAI(), blotSpawnPosition, Vector2.Zero, ModContent.ProjectileType<VoidBlot>(), VoidBlotDamage, 0f, -1, -13 - Main.rand.Next(20));
                    }
                }
            }

            leftArmHoverOffset.X -= maxArmHorizontalOffset * 0.5f + Main.rand.NextFloatDirection() * 10f;
            rightArmHoverOffset.X += maxArmHorizontalOffset * 0.5f + Main.rand.NextFloatDirection() * 10f;
            leftArmHoverOffset.Y -= 150f;
            rightArmHoverOffset.Y += 450f;

            shadowArmHoverOffset.X -= 600f;
            shadowArmHoverOffset.Y += 750f;
            shadowHandRotationOffset += 0.9f;
            shadowArmJitterMaxAngle = 0f;

            // Slow down.
            NPC.velocity *= 0.7f;
        }

        for (int i = 0; i < ShadowArms.Count; i++)
        {
            AvatarShadowArm arm = ShadowArms[i];
            float angleTime = arm.RandomID % 100 + arm.Time * 0.06f;
            float angleOffset = Sin(angleTime) * 0.94f;
            float handOffset = 1180f;
            bool right = i % 2 == 0;
            Vector2 baseDirection = Vector2.UnitX.RotatedBy(AperiodicSin(arm.Time / 5f) * shadowArmJitterMaxAngle + i * 0.2f);

            Vector2 armDestination = NPC.Center + baseDirection * right.ToDirectionInt() * handOffset + new Vector2(right.ToDirectionInt(), 1f) * shadowArmHoverOffset;
            arm.Center = Vector2.Lerp(arm.Center, armDestination, 0.15f) + Main.rand.NextVector2Circular(10f, 10f);
            arm.AnchorOffset = Vector2.UnitX * right.ToDirectionInt() * 200f;
            arm.Scale = InverseLerp(-3f, -18f, AITimer - Erasure_ChargeUpTime - Erasure_WaitTime) * InverseLerp(0f, 8f, arm.Time) * 1.2f;
            arm.VerticalFlip = arm.Center.X < NPC.Center.X;
            arm.FlipHandDirection = true;
            arm.HandRotationAngularOffset = Pi + right.ToDirectionInt() * shadowHandRotationOffset;
            arm.Time++;

            if (arm.Scale <= 0f && arm.Time >= 10)
                ShadowArms.RemoveAt(0);
        }

        // Update limbs.
        HeadPosition = Vector2.Lerp(HeadPosition, NPC.Center + new Vector2(2f, headVerticalOffset) * HeadScale * NeckAppearInterpolant, 0.14f);
        PerformBasicFrontArmUpdates(1.44f, leftArmHoverOffset, rightArmHoverOffset);
    }
}
