using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    /// <summary>
    /// How long Nameless waits before creating stars during his Inward Star Patterned Explosions state.
    /// </summary>
    public static int InwardStarPatternedExplosions_StarCreationDelay => GetAIInt("InwardStarPatternedExplosions_StarCreationDelay");

    /// <summary>
    /// How long Nameless spends creating stars during his Inward Star Patterned Explosions state.
    /// </summary>
    public static int InwardStarPatternedExplosions_StarCreationTime => GetAIInt("InwardStarPatternedExplosions_StarCreationTime");

    /// <summary>
    /// The rate at which Nameless creates stars during his Inward Star Patterned Explosions state.
    /// </summary>
    public static int InwardStarPatternedExplosions_StarCreationRate => (int)Clamp(GetAIInt("InwardStarPatternedExplosions_StarCreationRate") / Myself_DifficultyFactor, 1f, 100f);

    /// <summary>
    /// How long Nameless waits before transitioning to the next attack during Inward Star Patterned Explosions state.
    /// </summary>
    public static int InwardStarPatternedExplosions_AttackTransitionDelay => (int)Clamp(GetAIInt("InwardStarPatternedExplosions_AttackTransitionDelay") / Myself_DifficultyFactor, 2f, 1000f);

    /// <summary>
    /// Nameless' desired spin radius during his Inward Star Patterned Explosions state.
    /// </summary>
    public static float InwardStarPatternedExplosions_SpinRadius => GetAIFloat("InwardStarPatternedExplosions_SpinRadius");

    /// <summary>
    /// How long Nameless' Inward Star Patterned Explosions state goes on for overall.
    /// </summary>
    public static int InwardStarPatternedExplosions_AttackDuration => InwardStarPatternedExplosions_StarCreationDelay + InwardStarPatternedExplosions_StarCreationTime + InwardStarPatternedExplosions_AttackTransitionDelay;

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_InwardStarPatternedExplosions()
    {
        // Load the transition from InwardStarPatternedExplosions to the next in the cycle.
        StateMachine.RegisterTransition(NamelessAIType.InwardStarPatternedExplosions, null, false, () =>
        {
            return AITimer >= InwardStarPatternedExplosions_AttackDuration;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.InwardStarPatternedExplosions, DoBehavior_InwardStarPatternedExplosions);
    }

    public void DoBehavior_InwardStarPatternedExplosions()
    {
        int starCreationDelay = InwardStarPatternedExplosions_StarCreationDelay;
        int starCreationTime = InwardStarPatternedExplosions_StarCreationTime;
        int attackTransitionDelay = InwardStarPatternedExplosions_AttackTransitionDelay;
        float spinRadius = InwardStarPatternedExplosions_SpinRadius;
        float handMoveSpeedFactor = 3.7f;
        ref float spinDirection = ref NPC.ai[2];
        ref float starOffset = ref NPC.ai[3];
        Vector2 leftHandHoverDestination = NPC.Center + new Vector2(-660f - ZPosition * 40f, 118f);
        Vector2 rightHandHoverDestination = NPC.Center + new Vector2(660f + ZPosition * 40f, 118f);

        // Flap wings.
        UpdateWings(AITimer / 48f);

        // Hover above the player at first.
        if (AITimer <= starCreationDelay)
        {
            Vector2 hoverDestination = Target.Center - Vector2.UnitY * spinRadius;
            NPC.SmoothFlyNear(hoverDestination, 0.16f, 0.88f);

            // Begin a teleport.
            if (AITimer == 1)
                StartTeleportAnimation(() => Target.Center - Vector2.UnitY * spinRadius, 11, 11);
        }

        // Spin around the player and conjure the star.
        else if (AITimer <= starCreationDelay + starCreationTime)
        {
            // Decide the spin direction if that hasn't happened yet, based on which side of the player Nameless is.
            // This is done so that the spin continues moving in the direction the hover made Nameless move.
            if (spinDirection == 0f)
            {
                spinDirection = (Target.Center.X < NPC.Center.X).ToDirectionInt();
                NPC.netUpdate = true;
            }

            int movementDelay = starCreationDelay + starCreationTime - AITimer + 17;
            movementDelay = (int)Clamp(movementDelay / Pow(DifficultyFactor, 0.51f), 1f, movementDelay);

            float spinCompletionRatio = InverseLerp(0f, starCreationTime, AITimer - starCreationDelay);
            float spinOffsetAngle = SmoothStep(0f, TwoPi, InverseLerp(0f, starCreationTime, AITimer - starCreationDelay)) * spinDirection;
            float hoverSnapInterpolant = InverseLerp(0f, 5f, AITimer - starCreationDelay) * 0.48f;
            Vector2 spinOffset = -Vector2.UnitY.RotatedBy(spinOffsetAngle) * spinRadius;
            Vector2 spinDestination = Target.Center + spinOffset;

            // Spin around the target.
            NPC.Center = Vector2.Lerp(NPC.Center, spinDestination, hoverSnapInterpolant);
            NPC.velocity = spinOffset.SafeNormalize(Vector2.UnitY).RotatedBy(PiOver2 * spinDirection) * 25f;

            // Enter the background.
            ZPosition = Pow(spinCompletionRatio, 2f) * 10f;

            if (AITimer % InwardStarPatternedExplosions_StarCreationRate == 0f)
            {
                if (AITimer % (InwardStarPatternedExplosions_StarCreationRate * 2) == 0)
                    SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.SunFireballShootSound with { Volume = 1.05f, MaxInstances = 5 });

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.NewProjectileBetter(NPC.GetSource_FromAI(), CensorPosition, (TwoPi * spinCompletionRatio).ToRotationVector2() * 8f, ModContent.ProjectileType<StarPatternedStarburst>(), StarburstDamage, 0f, -1, 0f, movementDelay + 5);

                    int star = NPC.NewProjectileBetter(NPC.GetSource_FromAI(), CensorPosition, (TwoPi * spinCompletionRatio + Pi / 5f).ToRotationVector2() * 8f, ModContent.ProjectileType<StarPatternedStarburst>(), StarburstDamage, 0f, -1, 0f, movementDelay + 9);
                    if (Main.projectile.IndexInRange(star))
                    {
                        Main.projectile[star].As<StarPatternedStarburst>().RadiusOffset = 400f / DifficultyFactor;
                        Main.projectile[star].As<StarPatternedStarburst>().ConvergenceAngleOffset = Pi / 5f;
                    }

                    star = NPC.NewProjectileBetter(NPC.GetSource_FromAI(), CensorPosition, (TwoPi * spinCompletionRatio + TwoPi / 5f).ToRotationVector2() * 8f, ModContent.ProjectileType<StarPatternedStarburst>(), StarburstDamage, 0f, -1, 0f, movementDelay + 16);
                    if (Main.projectile.IndexInRange(star))
                    {
                        Main.projectile[star].As<StarPatternedStarburst>().RadiusOffset = 900f / DifficultyFactor;
                        Main.projectile[star].As<StarPatternedStarburst>().ConvergenceAngleOffset = TwoPi / 5f;
                    }
                }
            }
        }

        // Continue arcing as the stars do their thing.
        else
        {
            if (AITimer == starCreationDelay + starCreationTime + 16f)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.Supernova with { Volume = 0.8f, MaxInstances = 20 });
                GeneralScreenEffectSystem.RadialBlur.Start(NPC.Center, 1f, 13);
            }

            // Accelerate and continue arcing until very, very fast.
            if (NPC.velocity.Length() <= 105f)
                NPC.velocity = NPC.velocity.RotatedBy(TwoPi * spinDirection / 210f) * 1.08f;

            // Keep entering the background.
            ZPosition *= 1.02f;

            // Fade out while accelerating.
            NPC.Opacity = Saturate(NPC.Opacity - 0.02f);

            // Silently hover above the player when completely invisible.
            // This has no effect on the aesthetics and the player will not notice this, but it helps significantly in ensuring that Nameless isn't very far from the player when the next attack begins.
            if (NPC.Opacity <= 0f)
            {
                NPC.velocity = Vector2.Zero;
                NPC.Center = Target.Center - Vector2.UnitY * 560f;
            }

            if (AITimer == starCreationDelay + starCreationTime + attackTransitionDelay - 1)
                DestroyAllHands();
        }

        if (Hands.Count >= 2)
        {
            Hands[0].DirectionOverride = 0;
            Hands[1].DirectionOverride = 0;
            Hands[0].RotationOffset = 0f;
            Hands[1].RotationOffset = 0f;
            DefaultHandDrift(Hands[0], rightHandHoverDestination, handMoveSpeedFactor);
            DefaultHandDrift(Hands[1], leftHandHoverDestination, handMoveSpeedFactor);
        }
    }
}
