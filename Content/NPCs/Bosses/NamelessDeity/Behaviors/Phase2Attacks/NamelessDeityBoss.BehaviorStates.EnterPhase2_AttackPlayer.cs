using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    /// <summary>
    /// How long Nameless waits before firing purifying matter during his Celestial Dreamcatcher state.
    /// </summary>
    public static int EnterPhase2_AttackPlayer_ShootDelay => GetAIInt("EnterPhase2_AttackPlayer_ShootDelay");

    /// <summary>
    /// How much time Nameless spends firing purifying matter during his Celestial Dreamcatcher state.
    /// </summary>
    public static int EnterPhase2_AttackPlayer_ShootTime => GetAIInt("EnterPhase2_AttackPlayer_ShootTime");

    /// <summary>
    /// How long it takes for Nameless' celestial dreamcatcher to start fading away after purifying matter has stopped being summoned.
    /// </summary>
    public static int EnterPhase2_AttackPlayer_FadeOutDelay => GetAIInt("EnterPhase2_AttackPlayer_FadeOutDelay");

    /// <summary>
    /// How long it takes for Nameless' celestial dreamcatcher to fully fade away.
    /// </summary>
    public static int EnterPhase2_AttackPlayer_FadeOutTime => GetAIInt("EnterPhase2_AttackPlayer_FadeOutTime");

    /// <summary>
    /// The rate at which purifying matter is expelled during Nameless' Celestial Dreamcatcher state.
    /// </summary>
    public static int EnterPhase2_AttackPlayer_PurifyingMatterExpelRate => GetAIInt("EnterPhase2_AttackPlayer_PurifyingMatterExpelRate");

    /// <summary>
    /// The rate at which purifying matter is summoned to be sucked in during Nameless' Celestial Dreamcatcher state.
    /// </summary>
    public static int EnterPhase2_AttackPlayer_PurifyingMatterSuckRate => GetAIInt("EnterPhase2_AttackPlayer_PurifyingMatterSuckRate");

    /// <summary>
    /// The amount of damage Nameless' purfying matter does.
    /// </summary>
    public static int PurifyingMatterDamage => GetAIInt("PurifyingMatterDamage");

    /// <summary>
    /// The max speed factor of Nameless' purifying matter.
    /// </summary>
    public static float EnterPhase2_AttackPlayer_PurifyingMatterMaxSpeedFactor => GetAIFloat("EnterPhase2_AttackPlayer_PurifyingMatterMaxSpeedFactor");

    /// <summary>
    /// The acceleration of Nameless' purifying matter.
    /// </summary>
    public static float EnterPhase2_AttackPlayer_PurifyingMatterAcceleration => GetAIFloat("EnterPhase2_AttackPlayer_PurifyingMatterAcceleration");

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_EnterPhase2_AttackPlayer()
    {
        StateMachine.RegisterTransition(NamelessAIType.EnterPhase2_AttackPlayer, NamelessAIType.EnterPhase2_Return, false, () =>
        {
            return !AnyProjectiles(ModContent.ProjectileType<CelestialDreamcatcher>()) && AITimer >= 6;
        });
        StateMachine.RegisterStateBehavior(NamelessAIType.EnterPhase2_AttackPlayer, DoBehavior_EnterPhase2_AttackPlayer);
    }

    public void DoBehavior_EnterPhase2_AttackPlayer()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient && AITimer == 1)
        {
            Vector2 dreamcatcherSpawnPosition = Target.Center - Vector2.UnitY * 1100f;
            if (dreamcatcherSpawnPosition.Y < 500f)
                dreamcatcherSpawnPosition.Y = 500f;

            NPC.NewProjectileBetter(NPC.GetSource_FromAI(), dreamcatcherSpawnPosition, Vector2.Zero, ModContent.ProjectileType<CelestialDreamcatcher>(), 0, 0f);
        }

        NPC.SmoothFlyNear(Target.Center - Vector2.UnitY * 1640f, 0.06f, 0.93f);
        NPC.dontTakeDamage = true;

        UpdateWings(AITimer / 45f);
        DefaultUniversalHandMotion();
    }
}
