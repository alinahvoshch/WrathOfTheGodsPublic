using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity;

public partial class NamelessDeityBoss : ModNPC
{
    /// <summary>
    /// How long Nameless spends waiting during his phase 2 return state before returning to his typical attack cycle.
    /// </summary>
    public static int EnterPhase2_Return_StateDuration => SecondsToFrames(0.75f);

    [AutomatedMethodInvoke]
    public void LoadStateTransitions_EnterPhase2_Return()
    {
        StateMachine.RegisterTransition(NamelessAIType.EnterPhase2_Return, NamelessAIType.ResetCycle, false, () =>
        {
            return AITimer >= EnterPhase2_Return_StateDuration;
        });

        StateMachine.RegisterStateBehavior(NamelessAIType.EnterPhase2_Return, DoBehavior_EnterPhase2_Return);
    }

    public void DoBehavior_EnterPhase2_Return()
    {
        NPC.SmoothFlyNear(Target.Center - Vector2.UnitY * 400f, 0.04f, 0.93f);
        ZPosition = Lerp(ZPosition, 0f, 0.127f);

        NamelessDeitySky.KaleidoscopeInterpolant = Lerp(NamelessDeitySky.KaleidoscopeInterpolant, 0.23f, 0.12f);

        UpdateWings(FightTimer / 50f);
        DefaultUniversalHandMotion();
    }
}
