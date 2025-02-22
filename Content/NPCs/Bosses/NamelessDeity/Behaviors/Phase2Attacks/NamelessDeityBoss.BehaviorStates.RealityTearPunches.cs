using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Content.Particles.Metaballs;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.Configuration;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    /// <summary>
    /// The hover offset angle of Nameless' fists during his Reality Tear Punches state.
    /// </summary>
    public float PunchOffsetAngle
    {
        get;
        set;
    }

    /// <summary>
    /// The desired punch destination during Nameless' Reality Tear Punches state. Typically locked onto Nameless' chosen target.
    /// </summary>
    public Vector2 PunchDestination
    {
        get;
        set;
    }

    /// <summary>
    /// The previous punch impact position during Nameless' Reality Tear Punches state.
    /// </summary>
    public Vector2 PreviousPunchImpactPosition
    {
        get;
        set;
    }

    /// <summary>
    /// The intensity of screen overlay effects during Nameless' Reality Tear Punches state.
    /// </summary>
    public ref float RealityTearPunches_ScreenOverlayIntensity => ref NPC.ai[2];

    /// <summary>
    /// The amount of punches Nameless has performed so far during his Reality Tear Punches state.
    /// </summary>
    public ref float RealityTearPunches_PunchCounter => ref NPC.ai[3];

    /// <summary>
    /// The amount of punches Nameless should perform during his Reality Tear Punches state.
    /// </summary>
    public static int RealityTearPunches_PunchCount => GetAIInt("RealityTearPunches_PunchCount");

    /// <summary>
    /// The amount of time Nameless spends repositioning his hands during his Reality Tear Punches state.
    /// </summary>
    public static int RealityTearPunches_InitialRepositionTime => GetAIInt("RealityTearPunches_InitialRepositionTime");

    /// <summary>
    /// The amount of time Nameless spends reeling back his hands during his Reality Tear Punches state.
    /// </summary>
    public static int RealityTearPunches_ReelBackTime => GetAIInt("RealityTearPunches_ReelBackTime");

    /// <summary>
    /// The amount of time Nameless spends making his hands punch during his Reality Tear Punches state.
    /// </summary>
    public static int RealityTearPunches_PunchTime => GetAIInt("RealityTearPunches_PunchTime");

    /// <summary>
    /// The amount of time Nameless spends letting his hands rest during his Reality Tear Punches state.
    /// </summary>
    public static int RealityTearPunches_PostAttackHandRestTime => GetAIInt("RealityTearPunches_PostAttackHandRestTime");

    /// <summary>
    /// The default hover offset distance for Nameless' hands during his Reality Tear Punches state.
    /// </summary>
    public static float RealityTearPunches_DefaultHoverOffsetDistance => GetAIFloat("RealityTearPunches_DefaultHoverOffsetDistance");

    /// <summary>
    /// The distance Nameless' fists should be from each other after impact during his Reality Tear Punches state.
    /// </summary>
    public static float RealityTearPunches_ImpactCollisionDistance => GetAIFloat("RealityTearPunches_ImpactCollisionDistance");

    /// <summary>
    /// The amount of angular motion Nameless' hands cover before punching during his Reality Tear Punches state..
    /// </summary>
    public static float RealityTearPunches_PunchArc => GetAIFloat("RealityTearPunches_PunchArc");

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_RealityTearPunches()
    {
        // Load the transition from RealityTearPunches to the next in the cycle.
        StateMachine.RegisterTransition(NamelessAIType.RealityTearPunches, null, false, () =>
        {
            return RealityTearPunches_PunchCounter >= RealityTearPunches_PunchCount && !AnyProjectiles(ModContent.ProjectileType<BigNamelessPunchImpact>());
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(NamelessAIType.RealityTearPunches, DoBehavior_RealityTearPunches);
    }

    public void DoBehavior_RealityTearPunches()
    {
        // Enter the foreground.
        ZPosition *= 0.8f;

        // This AI method involves the usage of hands that are detached from Nameless, and as such are not guaranteed to be within the range of his render target.
        // As such, it is necessary to draw hands separately from it.
        DrawHandsSeparateFromRT = true;

        RealityTearPunches_ScreenOverlayIntensity = Saturate(RealityTearPunches_ScreenOverlayIntensity - 0.019f);

        // Flap wings.
        UpdateWings(FightTimer / 48f);

        bool handsShouldLookAtTarget = true;
        bool handsShouldLookForward = false;
        bool handsDoDamage = false;

        float difficulty = Pow(DifficultyFactor, 0.24f);
        int initialRepositionTime = (int)Clamp(RealityTearPunches_InitialRepositionTime / difficulty, 1f, 1000f);
        int reelBackTime = (int)Clamp(RealityTearPunches_ReelBackTime / difficulty, 3f, 1000f);
        int punchTime = (int)Clamp(RealityTearPunches_PunchTime / difficulty, 4f, 1000f);
        int handRestTime = RealityTearPunches_PostAttackHandRestTime;
        bool finalPunch = RealityTearPunches_PunchCounter >= RealityTearPunches_PunchCount - 1f;
        float reelBackDistance = RealityTearPunches_DefaultHoverOffsetDistance;
        float minReelBackDistance = RealityTearPunches_ImpactCollisionDistance;
        float punchArc = RealityTearPunches_PunchArc;
        float maxReelBackDistance = 500f / difficulty;
        float freeHandReachInterpolant = 0f;

        if (finalPunch)
        {
            reelBackTime += (int)(20f / difficulty);
            maxReelBackDistance += 240f / difficulty;
        }

        if (RealityTearPunches_PunchCounter >= RealityTearPunches_PunchCount)
        {
            DoBehavior_RealityTearPunches_PostAttackBehavior();
            return;
        }

        // Hover to the side of the target.
        Vector2 hoverDestination = Target.Center + new Vector2((Target.Center.X < NPC.Center.X).ToDirectionInt() * 560f, -380f);
        NPC.SmoothFlyNear(hoverDestination, 0.06f, 0.94f);

        if (AITimer == 1)
        {
            StartTeleportAnimation(() =>
            {
                ZPosition = 0f;
                return Target.Center + new Vector2((Target.Center.X < NPC.Center.X).ToDirectionInt() * 450f, -360f);
            }, 11, 11);
        }

        if (AITimer == 2)
        {
            PunchOffsetAngle = Target.Velocity.SafeNormalize((PunchOffsetAngle + 0.2f).ToRotationVector2()).ToRotation() - punchArc;
            NPC.netUpdate = true;
        }

        if (RealityTearPunches_PunchCounter >= 1f)
            freeHandReachInterpolant = InverseLerp(initialRepositionTime + reelBackTime, 0f, AITimer);

        // Redirect.
        if (AITimer <= initialRepositionTime)
        {
            PunchDestination = Target.Center;
            PunchOffsetAngle += punchArc / initialRepositionTime;
        }

        // Reel back before punching.
        else if (AITimer <= initialRepositionTime + reelBackTime)
        {
            float reelBackInterpolant = InverseLerp(0f, reelBackTime, AITimer - initialRepositionTime);
            reelBackDistance += Pow(reelBackInterpolant, 3.2f) * maxReelBackDistance;

            PunchDestination = Target.Center;
        }

        // Punch.
        else if (AITimer <= initialRepositionTime + reelBackTime + punchTime)
        {
            int punchTimer = AITimer - initialRepositionTime - reelBackTime;
            float punchDistance = InverseLerp(0f, punchTime, punchTimer) * 900f;
            reelBackDistance = Clamp(reelBackDistance - punchDistance, minReelBackDistance, 900f);
            handsDoDamage = true;

            // Handle punch impact effects.
            if (AITimer == initialRepositionTime + reelBackTime + punchTime)
                DoBehavior_RealityTearPunches_ImpactEffects();
        }

        // Handle post-punch effects.
        else if (AITimer <= initialRepositionTime + reelBackTime + punchTime + handRestTime)
        {
            reelBackDistance = minReelBackDistance;
            freeHandReachInterpolant = InverseLerp(0f, handRestTime, AITimer - (initialRepositionTime + reelBackTime + punchTime));
        }

        // Reset for the next punch.
        else
        {
            AITimer = initialRepositionTime / 2 - 9;
            if (AITimer < 3)
                AITimer = 3;

            PunchOffsetAngle = Target.Velocity.SafeNormalize((PunchOffsetAngle + 0.2f).ToRotationVector2()).ToRotation() - punchArc + Main.rand.NextFloatDirection() * 0.4f;
            RealityTearPunches_PunchCounter++;
            NPC.netUpdate = true;
        }

        Vector2 handHoverOffest = PunchOffsetAngle.ToRotationVector2() * reelBackDistance;

        // Reset the heavenly background intensity over time after it's been darkened.
        if (AITimer >= initialRepositionTime && AITimer <= initialRepositionTime + reelBackTime)
            NamelessDeitySky.HeavenlyBackgroundIntensity = Lerp(NamelessDeitySky.HeavenlyBackgroundIntensity, 1f, 0.12f);
        NamelessDeitySky.KaleidoscopeInterpolant = InverseLerp(0.9f, 0.25f, NamelessDeitySky.HeavenlyBackgroundIntensity) * 0.8f;

        if (RealityTearPunches_PunchCounter <= 0)
        {
            while (Hands.Count < RealityTearPunches_PunchCount * 2)
                ConjureHandsAtPosition(NPC.Center, Vector2.Zero);
        }

        // Update hands.
        int usedHandIndexOffset = (int)RealityTearPunches_PunchCounter * 2;
        float handFlySpeedInterpolant = InverseLerp(0f, initialRepositionTime, AITimer).Cubed();
        for (int i = 0; i < 2; i++)
        {
            if (usedHandIndexOffset + i >= Hands.Count)
                continue;

            NamelessDeityHand hand = Hands[usedHandIndexOffset + i];
            hand.HasArms = false;
            hand.ScaleFactor = 1f;
            hand.HandType = NamelessDeityHandType.ClosedFist;
            hand.CanDoDamage = handsDoDamage;
            hand.DirectionOverride = 1;

            Vector2 handDestination = PunchDestination + handHoverOffest * (i == 0).ToDirectionInt();

            DefaultHandDrift(hand, handDestination, handFlySpeedInterpolant * Pow(DifficultyFactor, 1.3f) * 3f);
            hand.FreeCenter = Vector2.Lerp(hand.FreeCenter, handDestination, handFlySpeedInterpolant * 0.3f);

            if (handsShouldLookAtTarget)
                hand.RotationOffset = hand.FreeCenter.AngleTo(PunchDestination) + Pi;
            if (handsShouldLookForward && hand.Velocity.Length() >= 1f)
                hand.RotationOffset = hand.Velocity.ToRotation() + Pi;
        }
        for (int i = 0; i < Hands.Count; i++)
        {
            NamelessDeityHand hand = Hands[i];

            // Don't interfere with the punching hands.
            if (i >= usedHandIndexOffset && i <= usedHandIndexOffset + 1)
                continue;

            hand.DirectionOverride = 0;
            hand.CanDoDamage = false;
            hand.HandType = NamelessDeityHandType.ClosedFist;
            hand.RotationOffset = hand.RotationOffset.AngleLerp(0f, 0.1f).AngleTowards(0f, 0.04f);

            // Functionally remove old hands.
            if (i < usedHandIndexOffset)
            {
                hand.HasArms = false;
                hand.FreeCenter = Vector2.One * -99999f;
                continue;
            }

            hand.HasArms = true;
            hand.ForearmIKLengthFactor = 0.5f;

            int side = (i % 2 == 0).ToDirectionInt();
            int index = i / 2;
            float verticalOffset = Sin(TwoPi * FightTimer / 105f + index * 2f) * 80f - 100f;
            Vector2 handOffset = new Vector2(side * (500f + index * 120f), index * 100f + verticalOffset);
            handOffset.X *= 1f - freeHandReachInterpolant * 0.7f;
            handOffset.Y = Lerp(handOffset.Y, -700f, freeHandReachInterpolant * 0.7f);

            Vector2 handDestination = NPC.Center + handOffset;
            hand.Velocity *= 0.2f;

            hand.FreeCenter = Vector2.Lerp(hand.FreeCenter, handDestination, freeHandReachInterpolant * 0.9f);
        }
    }

    /// <summary>
    /// Handles Nameless' post-attack behaviors after his reality tear punches attack has naturally concluded.
    /// </summary>
    public void DoBehavior_RealityTearPunches_PostAttackBehavior()
    {
        NPC.SmoothFlyNear(Target.Center - Vector2.UnitY * 1656f, 0.12f, 0.87f);
    }

    /// <summary>
    /// Updates Nameless' code background screen shader.
    /// </summary>
    public void DoBehavior_RealityTearPunches_UpdateScreenShader()
    {
        if (RealityTearPunches_ScreenOverlayIntensity > 0f && CurrentState == NamelessAIType.RealityTearPunches)
        {
            ManagedScreenFilter overlayShader = ShaderManager.GetFilter("NoxusBoss.RealityPunchOverlayShader");
            overlayShader.TrySetParameter("barOffsetMax", WoTGConfig.Instance.PhotosensitivityMode ? 0f : 0.085f);
            overlayShader.TrySetParameter("intensity", RealityTearPunches_ScreenOverlayIntensity);
            overlayShader.TrySetParameter("impactPosition", Vector2.Transform(PreviousPunchImpactPosition - Main.screenPosition, Main.GameViewMatrix.TransformationMatrix));
            overlayShader.SetTexture(WavyBlotchNoise, 1, SamplerState.PointWrap);
            overlayShader.Activate();
        }
    }

    /// <summary>
    /// Handles the creation of impact effects during the Nameless Deity's reality tear punches attack.
    /// </summary>
    public void DoBehavior_RealityTearPunches_ImpactEffects()
    {
        bool finalPunch = RealityTearPunches_PunchCounter >= RealityTearPunches_PunchCount - 1f;

        ScreenShakeSystem.StartShakeAtPoint(PunchDestination, 25f, shakeStrengthDissipationIncrement: 1.2f);
        NamelessDeitySky.HeavenlyBackgroundIntensity = 0.45f;

        SoundEngine.PlaySound((finalPunch ? GennedAssets.Sounds.NamelessDeity.RealityTearPunchBig : GennedAssets.Sounds.NamelessDeity.RealityTearPunch) with { Volume = 1.5f }, PunchDestination);
        SoundEngine.PlaySound(GennedAssets.Sounds.Common.Glitch with { PitchVariance = 0.2f, Volume = 1.3f, MaxInstances = 8 }, PunchDestination);

        RealityTearPunches_ScreenOverlayIntensity = 1f;

        Vector2 punchCenter = PunchDestination;

        // Create a bunch of special, hexagon-shaped code metaballs that fly outward.
        for (int i = 0; i < 50; i++)
            ModContent.GetInstance<CodeMetaball>().CreateParticle(punchCenter, Main.rand.NextVector2Circular(20f, 20f), Main.rand.NextFloat(40f, 338f), Main.rand.NextFloat(TwoPi));
        for (int i = 0; i < 50; i++)
        {
            Vector2 metaballVelocity = Main.rand.NextVector2Circular(finalPunch ? 95f : 40f, 8f).RotatedBy(PunchOffsetAngle + PiOver2);
            ModContent.GetInstance<CodeMetaball>().CreateParticle(punchCenter, metaballVelocity, Main.rand.NextFloat(30f, 67f), Main.rand.NextFloat(TwoPi));
        }

        // Handle server-side syncing and projectile creation.
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            if (finalPunch)
                NPC.NewProjectileBetter(NPC.GetSource_FromAI(), PunchDestination, Vector2.Zero, ModContent.ProjectileType<BigNamelessPunchImpact>(), 0, 0f);

            PreviousPunchImpactPosition = PunchDestination;
            NPC.netUpdate = true;

            for (int i = 0; i < 19; i++)
            {
                int arcLifetime = SecondsToFrames(Main.rand.NextFloat(0.1833f, 0.4f));
                Vector2 arcSpawnPosition = punchCenter + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 arcOffset = Main.rand.NextVector2Unit() * Main.rand.NextFloat(70f, 650f);
                Vector2 arcDestination = arcSpawnPosition + arcOffset;
                Vector2 arcLength = (arcDestination - arcSpawnPosition) * Main.rand.NextFloat(0.9f, 1f);
                NPC.NewProjectileBetter(NPC.GetSource_FromAI(), arcSpawnPosition, arcLength, ModContent.ProjectileType<CodeLightningArc>(), 0, 0f, -1, arcLifetime, 1f);
            }
        }
    }
}
