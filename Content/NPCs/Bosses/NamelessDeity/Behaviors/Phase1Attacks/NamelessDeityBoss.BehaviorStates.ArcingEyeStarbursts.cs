using Luminance.Common.Easings;
using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.Graphics.ScreenShake;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    /// <summary>
    /// How long Nameless waits before firing projectiles during his Arcing Eye Starbursts state.
    /// </summary>
    public static int ArcingEyeStarbursts_ShootDelay => GetAIInt("ArcingEyeStarbursts_ShootDelay");

    /// <summary>
    /// How many starburst projectiles Nameless conjures during his Arcing Eye Starbursts state.
    /// </summary>
    public static int ArcingEyeStarbursts_StarburstCount => GetAIInt("ArcingEyeStarbursts_StarburstCount");

    /// <summary>
    /// How long Nameless waits after shooting starbursts to transition to his next attack during his Arcing Eye Starbursts state.
    /// </summary>
    public static int ArcingEyeStarbursts_AttackTransitionDelay => GetAIInt("ArcingEyeStarbursts_AttackTransitionDelay");

    /// <summary>
    /// How angular spread of Nameless' starbursts during his Arcing Eye Starbursts state.
    /// </summary>
    public static float ArcingEyeStarbursts_StarburstArc => GetAIFloat("ArcingEyeStarbursts_StarburstArc");

    /// <summary>
    /// The speed of Nameless' starbursts during his Arcing Eye Starbursts state.
    /// </summary>
    public static float ArcingEyeStarbursts_StarburstShootSpeed => GetAIFloat("ArcingEyeStarbursts_StarburstShootSpeed");

    /// <summary>
    /// How long Nameless' Arcing Eye Starbursts state goes on for overall.
    /// </summary>
    public static int ArcingEyeStarbursts_TotalDuration => ArcingEyeStarbursts_ShootDelay + ArcingEyeStarbursts_StarburstCount + ArcingEyeStarbursts_AttackTransitionDelay;

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_ArcingEyeStarbursts()
    {
        // Load the transition from ArcingEyeStarbursts to the next in the cycle.
        StateMachine.RegisterTransition(NamelessAIType.ArcingEyeStarbursts, null, false, () =>
        {
            return AITimer >= ArcingEyeStarbursts_TotalDuration;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.ArcingEyeStarbursts, DoBehavior_ArcingEyeStarbursts);
    }

    public void DoBehavior_ArcingEyeStarbursts()
    {
        int shootDelay = ArcingEyeStarbursts_ShootDelay;
        int starburstCount = (int)(ArcingEyeStarbursts_StarburstCount * Clamp(DifficultyFactor, 1f, 3.3f));
        int attackTransitionDelay = ArcingEyeStarbursts_AttackTransitionDelay;

        // Flap wings.
        UpdateWings(AITimer / 45f);

        // Apparently needed after black hole attack??? The fuck????????
        DestroyAllHands();

        // Teleport above the target at first.
        Vector2 startingTeleportPosition = Target.Center - Vector2.UnitY * 372f;
        if (AITimer == 1 && !NPC.WithinRange(startingTeleportPosition, 500f))
            StartTeleportAnimation(() => startingTeleportPosition, 12, 12);

        // NOTE: Cool fun facts, this line used to be the following:
        // InverseLerp(shootDelay * 0.5f, shootDelay - 5f, AITimer);
        // But what if I told you shootDelay is 10 in GFB.
        // Reduce the numbers and you get the following expression:
        // lookAtTargetInterpolant = InverseLerp(5f, 5f, AITimer);

        // A perfect fucking storm to cause a division by 0 error, sending Nameless' position into the realm of the NaNs and
        // causing the world's most insane execution engine exception downstream. Probably due to faulty rendering.

        // What can we learn from this? Simple!
        // DON'T MIX SUBTRACTION AND MULTIPLICATION IN THE SAME INVERSELERP EXPRESSION YOU DUMMY!!

        // Quickly hover above the player, zipping back and forth, before firing.
        float lookAtTargetInterpolant = InverseLerp(shootDelay * 0.5f, shootDelay * 0.96f, AITimer);
        Vector2 sideOfPlayer = Target.Center + (Target.Center.X < NPC.Center.X).ToDirectionInt() * Vector2.UnitX * 800f;
        Vector2 hoverDestination = Vector2.Lerp(Target.Center + GeneralHoverOffset, sideOfPlayer, Pow(lookAtTargetInterpolant, 0.5f));
        Vector2 idealVelocity = (hoverDestination - NPC.Center) * Sqrt(1f - lookAtTargetInterpolant) * 0.14f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, 0.17f);

        // Decide a hover offset if it's unitialized or has been reached.
        if (GeneralHoverOffset == Vector2.Zero || (NPC.WithinRange(Target.Center + GeneralHoverOffset, Target.Velocity.Length() * 2f + 90f) && AITimer % 20f == 0f))
        {
            float horizontalOffsetSign = GeneralHoverOffset.X == 0f ? Main.rand.NextFromList(-1f, 1f) : -Sign(GeneralHoverOffset.X);
            GeneralHoverOffset = new Vector2(horizontalOffsetSign * Main.rand.NextFloat(500f, 700f), Main.rand.NextFloat(-550f, -340f));
            NPC.netUpdate = true;
        }

        // Shoot the redirecting starbursts.
        if (AITimer >= shootDelay && AITimer <= shootDelay + starburstCount)
        {
            // Create an initial spread of starbursts on the first frame.
            if (AITimer == shootDelay)
            {
                CustomScreenShakeSystem.Start(30, 14.5f).
                    WithDissipationCurve(EasingCurves.Quintic).
                    WithDistanceFadeoff(EyePosition);

                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.GenericBurst with { Volume = 1.3f, PitchVariance = 0.15f });
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int projectileCount = (int)Clamp(DifficultyFactor * 21f, 21f, 93f);
                    for (int i = 0; i < projectileCount; i++)
                    {
                        Vector2 starburstVelocity = (Target.Center - EyePosition).SafeNormalize(Vector2.UnitY).RotatedBy(TwoPi * i / projectileCount) * ArcingEyeStarbursts_StarburstShootSpeed * 0.08f;
                        NPC.NewProjectileBetter(NPC.GetSource_FromAI(), EyePosition, starburstVelocity * DifficultyFactor, ModContent.ProjectileType<Starburst>(), StarburstDamage, 0f);
                    }
                }

                NamelessDeityKeyboardShader.BrightnessIntensity += 0.67f;
                GeneralScreenEffectSystem.RadialBlur.Start(EyePosition, 0.6f, 24);
            }

            // Release the projectiles.
            float starburstInterpolant = InverseLerp(0f, starburstCount, AITimer - shootDelay);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                float starburstShootOffsetAngle = Lerp(-ArcingEyeStarbursts_StarburstArc, ArcingEyeStarbursts_StarburstArc, starburstInterpolant);
                Vector2 starburstVelocity = (Target.Center - EyePosition).SafeNormalize(Vector2.UnitY).RotatedBy(starburstShootOffsetAngle) * ArcingEyeStarbursts_StarburstShootSpeed;
                NPC.NewProjectileBetter(NPC.GetSource_FromAI(), EyePosition, starburstVelocity, ModContent.ProjectileType<ArcingStarburst>(), StarburstDamage, 0f, -1, 0f, DifficultyFactor);
            }

            // Create sound and screen effects.
            GeneralScreenEffectSystem.ChromaticAberration.Start(EyePosition, starburstInterpolant * 3f, 10);

            // Play fireball sounds.
            if (Main.rand.NextBool(3))
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.SunFireballShootSound with
                {
                    MaxInstances = 100,
                    Volume = 0.5f
                });
            }
        }

        // Update universal hands.
        float handHoverOffset = Utils.Remap(AITimer - shootDelay - starburstCount, 0f, starburstCount + 12f, 900f, 1400f);
        DefaultUniversalHandMotion(handHoverOffset);
    }
}
