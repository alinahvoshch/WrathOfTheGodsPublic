using Luminance.Common.StateMachines;
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
    /// <summary>
    /// The amount of feather barrages Nameless has performed so far during his psychedelic feather attack.
    /// </summary>
    public ref float PsychedelicFeatherStrikes_BarrageCounter => ref NPC.ai[2];

    /// <summary>
    /// The amount of feather barrages Nameless should perform during his psychedelic feather attack.
    /// </summary>
    public static int PsychedelicFeatherStrikes_BarrageCount => GetAIInt("PsychedelicFeatherStrikes_BarrageCount");

    /// <summary>
    /// The starting rate at which Nameless releases feathers during his psychedelic feather attack.
    /// </summary>
    public static int PsychedelicFeatherStrikes_StartingBarrageShootRate => GetAIInt("PsychedelicFeatherStrikes_StartingBarrageShootRate");

    /// <summary>
    /// The ending rate at which Nameless releases feathers during his psychedelic feather attack.
    /// </summary>
    public static int PsychedelicFeatherStrikes_EndingBarrageShootRate => GetAIInt("PsychedelicFeatherStrikes_EndingBarrageShootRate");

    /// <summary>
    /// How many psychedelic feather barrages Nameless needs to do in order to interpolate between <see cref="PsychedelicFeatherStrikes_StartingBarrageShootRate"/> and 
    /// <see cref="PsychedelicFeatherStrikes_EndingBarrageShootRate"/> fully. Once the barrage counter exceeds this value, burther barrages will not fire at a faster rate.
    /// </summary>
    public static int PsychedelicFeatherStrikes_BarrageBuildupCount => GetAIInt("PsychedelicFeatherStrikes_BarrageBuildupCount");

    /// <summary>
    /// The factor which dictates how far away feathers should spawn away from the player before they fire during Nameless' psychedelic feather attack.
    /// </summary>
    public static float PsychedelicFeatherStrikes_FeatherSpawnProximityFactor => GetAIFloat("PsychedelicFeatherStrikes_FeatherSpawnProximityFactor");

    /// <summary>
    /// The amount of damage Nameless' psychedelic feathers do.
    /// </summary>
    public static int PsychedelicFeatherDamage => GetAIInt("PsychedelicFeatherDamage");

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_PsychedelicFeatherStrikes()
    {
        StateMachine.RegisterTransition(NamelessAIType.PsychedelicFeatherStrikes, null, false, () =>
        {
            return PsychedelicFeatherStrikes_BarrageCounter >= PsychedelicFeatherStrikes_BarrageCount && !AnyProjectiles(ModContent.ProjectileType<PsychedelicFeather>()) && ZPosition <= 0.004f;
        });

        StateMachine.RegisterStateBehavior(NamelessAIType.PsychedelicFeatherStrikes, DoBehavior_PsychedelicFeatherStrikes);
    }

    public void DoBehavior_PsychedelicFeatherStrikes()
    {
        Vector2 flyOffset = (TwoPi * FightTimer / 150f).ToRotationVector2() * 120f - Vector2.UnitY * 300f;
        Vector2 idealVelocity = NPC.SafeDirectionTo(Target.Center + flyOffset) * 30f;
        NPC.SimpleFlyMovement(idealVelocity, 0.35f);

        // Enter the background when attacking, and return to the foreground afterwards.
        bool doneAttacking = PsychedelicFeatherStrikes_BarrageCounter >= PsychedelicFeatherStrikes_BarrageCount && !AnyProjectiles(ModContent.ProjectileType<PsychedelicFeather>());
        float idealZPosition = doneAttacking ? 0f : 2.1f;
        ZPosition = Lerp(ZPosition, idealZPosition, 0.11f);

        float barrageSetCompletion = InverseLerp(0f, PsychedelicFeatherStrikes_BarrageBuildupCount, PsychedelicFeatherStrikes_BarrageCounter);
        int barrageDuration = (int)Lerp(PsychedelicFeatherStrikes_StartingBarrageShootRate, PsychedelicFeatherStrikes_EndingBarrageShootRate, barrageSetCompletion);
        barrageDuration = (int)Clamp(barrageDuration / DifficultyFactor, 1f, barrageDuration);

        if (AITimer >= barrageDuration && PsychedelicFeatherStrikes_BarrageCounter < PsychedelicFeatherStrikes_BarrageCount)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.FeatherAppear with { MaxInstances = 0, Volume = 0.7f });
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 spawnPosition = Target.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(350f, 475f) * PsychedelicFeatherStrikes_FeatherSpawnProximityFactor / DifficultyFactor;
                NPC.NewProjectileBetter(NPC.GetSource_FromAI(), spawnPosition, Vector2.Zero, ModContent.ProjectileType<PsychedelicFeather>(), PsychedelicFeatherDamage, 0f, -1);

                AITimer = 0;
                PsychedelicFeatherStrikes_BarrageCounter++;

                NPC.netUpdate = true;
            }
        }

        UpdateWings(FightTimer / 50f);
        DoBehavior_PsychedelicFeatherStrikes_UpdateHands();
    }

    /// <summary>
    /// Updates Nameless hands during his psychedelic feather strikes attack.
    /// </summary>
    public void DoBehavior_PsychedelicFeatherStrikes_UpdateHands()
    {
        float horizontalHandOffset = Lerp(420f, 920f, Cos01(TwoPi * FightTimer / 30f).Cubed());
        float verticalHandOffset = Sin(TwoPi * FightTimer / 120f) * 75f - 160f;

        if (Hands.Count(h => h.HasArms) < TotalUniversalHands)
        {
            Hands.Insert(0, new(NPC.Center, true));
            NPC.netUpdate = true;
        }
        if (Hands.Count < 2)
            return;

        DefaultHandDrift(Hands[0], NPC.Center + new Vector2(-horizontalHandOffset, verticalHandOffset), 300f);
        DefaultHandDrift(Hands[1], NPC.Center + new Vector2(horizontalHandOffset, verticalHandOffset), 300f);

        Hands[0].DirectionOverride = 0;
        Hands[1].DirectionOverride = 0;
        Hands[0].RotationOffset = 0f;
        Hands[1].RotationOffset = 0f;
        Hands[0].HasArms = true;
        Hands[1].HasArms = true;
        Hands[0].VisuallyFlipHand = false;
        Hands[1].VisuallyFlipHand = false;
        Hands[0].ForearmIKLengthFactor = 0.5f;
        Hands[1].ForearmIKLengthFactor = 0.5f;
    }
}
