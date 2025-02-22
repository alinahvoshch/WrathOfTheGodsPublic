using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    public List<Vector2> StarSpawnOffsets = [];

    /// <summary>
    /// How long Nameless spends redirecting to get near his target during his Conjure Exploding Stars state.
    /// </summary>
    public static int ConjureExplodingStars_RedirectTime => GetAIInt("ConjureExplodingStars_RedirectTime");

    /// <summary>
    /// How long Nameless spends hovering place near his target during his Conjure Exploding Stars state.
    /// </summary>
    public static int ConjureExplodingStars_HoverTime => GetAIInt("ConjureExplodingStars_HoverTime");

    /// <summary>
    /// How many star explosions that Nameless should conjure during his Conjure Exploding Stars state.
    /// </summary>
    public static int ConjureExplodingStars_StarConjureCount => GetAIInt("ConjureExplodingStars_StarConjureCount");

    /// <summary>
    /// The rate at which Nameless conjures stars during his Conjure Exploding Stars state.
    /// </summary>
    public static int ConjureExplodingStars_StarConjureRate => GetAIInt("ConjureExplodingStars_StarConjureRate");

    /// <summary>
    /// How long it takes for stars to explode during Nameless' Conjure Exploding Stars state.
    /// </summary>
    public static int ConjureExplodingStars_StarBlastDelay => GetAIInt("ConjureExplodingStars_StarBlastDelay");

    /// <summary>
    /// How long Nameless waits after conjuring star explosions to transition to his next attack during his Conjure Exploding Stars state.
    /// </summary>
    public static int ConjureExplodingStars_AttackTransitionDelay => GetAIInt("ConjureExplodingStars_AttackTransitionDelay");

    /// <summary>
    /// How many sets of explosive stars as a collective Nameless should conjure during his Conjure Exploding Stars state.
    /// </summary>
    public int ConjureExplodingStars_ExplosionSetCount => (int)(GetAIInt("ConjureExplodingStars_ExplosionSetCount") * DifficultyFactor);

    /// <summary>
    /// The offset radius of star explosions during Nameless' Conjure Exploding Stars state.
    /// </summary>
    public static float ConjureExplodingStars_StarOffsetRadius => GetAIFloat("ConjureExplodingStars_StarOffsetRadius");

    /// <summary>
    /// How many explosive star sets Nameless has conjured so far for his Conjure Exploding Stars state.
    /// </summary>
    public ref float ConjureExplodingStars_ExplosionSetCounter => ref NPC.ai[2];

    /// <summary>
    /// The angular offset of stars for the current explosion set orientation in Nameless' Conjure Exploding Stars state.
    /// </summary>
    public ref float ConjureExplodingStars_ExplosionAngularOffset => ref NPC.ai[3];

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_ConjureExplodingStars()
    {
        // Load the transition from ConjureExplodingStars to the next in the cycle.
        StateMachine.RegisterTransition(NamelessAIType.ConjureExplodingStars, null, false, () =>
        {
            return ConjureExplodingStars_ExplosionSetCounter >= ConjureExplodingStars_ExplosionSetCount;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.ConjureExplodingStars, DoBehavior_ConjureExplodingStars);
    }

    public void DoBehavior_ConjureExplodingStars()
    {
        int redirectTime = ConjureExplodingStars_RedirectTime;
        int hoverTime = ConjureExplodingStars_HoverTime;
        int starConjureCount = (int)(ConjureExplodingStars_StarConjureCount * Lerp(DifficultyFactor, 1f, 4f));
        int starCreateRate = (int)Clamp(ConjureExplodingStars_StarConjureRate / DifficultyFactor, 2f, ConjureExplodingStars_StarConjureRate);
        int starBlastDelay = ConjureExplodingStars_StarBlastDelay;
        int attackTransitionDelay = ConjureExplodingStars_AttackTransitionDelay;
        int explosionCount = ConjureExplodingStars_ExplosionSetCount;
        int starTelegraphTime = starConjureCount * starCreateRate;
        float starOffsetRadius = ConjureExplodingStars_StarOffsetRadius / DifficultyFactor;

        // Initialize the explosion's angular offset on the first frame.
        if (AITimer == 1)
        {
            ConjureExplodingStars_ExplosionAngularOffset = TwoPi * Main.rand.NextFloat() / ConjureExplodingStars_StarConjureCount * 0.37f;
            NPC.netUpdate = true;
        }

        // Fly towards above the target at first. After the redirect and hover time has concluded, however, Nameless becomes comfortable with his current position and slows down rapidly.
        float slowdownRadius = 230f;
        Vector2 hoverDestination = Target.Center - Vector2.UnitY * 380f;
        if (AITimer >= redirectTime + hoverTime)
            hoverDestination = NPC.Center;
        NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, 0.17f, 0.89f, slowdownRadius);

        // Slow down rapidly if flying past the hover destination. If this happens when Nameless is moving really, really fast a sonic boom of sorts is created.
        if (Vector2.Dot(NPC.velocity, NPC.SafeDirectionTo(hoverDestination)) < 0f)
        {
            // Create the sonic boom if necessary.
            if (NPC.velocity.Length() >= 75f)
            {
                NPC.velocity = NPC.velocity.ClampLength(0f, 74f);
                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.SunFireballShootSound with { Volume = 1.05f, MaxInstances = 5 }, Target.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<LightWave>(), 0, 0f);
            }

            NPC.velocity *= 0.67f;
        }

        // Flap wings.
        UpdateWings(AITimer / 54f);

        // Update hands.
        if (Hands.Count >= 2)
        {
            int raiseHandTimer = AITimer - redirectTime - hoverTime - starTelegraphTime - starBlastDelay;
            float raiseHandInterpolant = InverseLerp(0f, 10f, raiseHandTimer);
            float handHoverOffset = Lerp(950f, 660f, raiseHandInterpolant);
            DefaultHandDrift(Hands[0], NPC.Center + new Vector2(-handHoverOffset, 100f + raiseHandInterpolant * 120f) * TeleportVisualsAdjustedScale, 2.5f);
            DefaultHandDrift(Hands[1], NPC.Center + new Vector2(handHoverOffset, 100f - raiseHandInterpolant * 900f) * TeleportVisualsAdjustedScale, 2.5f);
            Hands[0].HasArms = true;
            Hands[1].HasArms = true;

            // Snap fingers and make the screen shake.
            if (AITimer == redirectTime + hoverTime - 10f)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.FingerSnap with
                {
                    Volume = 4f
                });
                ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 6f);
            }
        }

        // Create star telegraphs.
        if (AITimer >= redirectTime + hoverTime && AITimer <= redirectTime + hoverTime + starTelegraphTime && AITimer % starCreateRate == 1f)
        {
            float starSpawnOffsetAngle = TwoPi * (AITimer - redirectTime - hoverTime) / starTelegraphTime - PiOver2 + ConjureExplodingStars_ExplosionAngularOffset;
            Vector2 starSpawnOffset = starSpawnOffsetAngle.ToRotationVector2() * starOffsetRadius;
            StarSpawnOffsets.Add(starSpawnOffset);

            Color bloomColor = Color.Lerp(Color.Orange, Color.DarkRed, Main.rand.NextFloat(0.25f, 0.9f));
            CreateTwinkle(Target.Center + starSpawnOffset, Vector2.One * 3f, bloomColor, new(starSpawnOffset, () => Target.Center));

            NPC.netSpam = 0;
            NPC.netUpdate = true;
        }

        // Release stars.
        if (AITimer == redirectTime + hoverTime + starTelegraphTime + starBlastDelay)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                foreach (Vector2 starSpawnOffset in StarSpawnOffsets)
                    NPC.NewProjectileBetter(NPC.GetSource_FromAI(), Target.Center + starSpawnOffset, Vector2.Zero, ModContent.ProjectileType<ExplodingStar>(), ExplodingStarDamage, 0f, -1, 0f, 0.6f);

                StarSpawnOffsets.Clear();
                NPC.netSpam = 0;
                NPC.netUpdate = true;
            }
        }

        if (AITimer >= redirectTime + hoverTime + starTelegraphTime * 2f + starBlastDelay + attackTransitionDelay)
        {
            ConjureExplodingStars_ExplosionSetCounter++;
            if (ConjureExplodingStars_ExplosionSetCounter < ConjureExplodingStars_ExplosionSetCount)
            {
                AITimer = 0;
                NPC.netUpdate = true;
            }
        }
    }
}
