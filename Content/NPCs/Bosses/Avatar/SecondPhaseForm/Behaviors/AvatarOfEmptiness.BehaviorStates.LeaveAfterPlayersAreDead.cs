using Luminance.Common.DataStructures;
using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Core.World.GameScenes.RiftEclipse;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    [AutomatedMethodInvoke]
    public void LoadState_LeaveAfterPlayersAreDead()
    {
        // Load the leave state behavior.
        StateMachine.RegisterStateBehavior(AvatarAIType.LeaveAfterPlayersAreDead, DoBehavior_LeaveAfterPlayersAreDead);
        StateMachine.ApplyToAllStatesExcept(previousState =>
        {
            StateMachine.RegisterTransition(previousState, AvatarAIType.LeaveAfterPlayersAreDead, false, CheckLeaveCondition);
        });
    }

    /// <summary>
    /// Checks if the Avatar should despawn and perform his leave animation or not.
    /// </summary>
    public bool CheckLeaveCondition()
    {
        if (!Target.Invalid)
            return false;

        // Don't restart an attack if the Avatar is already leaving.
        bool alreadyLeaving = CurrentState == AvatarAIType.LeaveAfterBeingHit || CurrentState == AvatarAIType.LeaveAfterPlayersAreDead;
        if (alreadyLeaving)
            return false;

        // Special case: If the Avatar is currently teleporting, hijack the teleport condition so that he leaves instead of going to
        // wherever he was planning to originally, instead of going to the leave state and doing an awkward second teleport.
        if (CurrentState == AvatarAIType.Teleport)
        {
            TeleportAction = DoBehavior_LeaveAfterPlayersAreDead_TeleportAction;
            return false;
        }

        // All the above checks were passed successfully. Make the Avatar perform his leave animation.
        return true;
    }

    /// <summary>
    /// The teleport action the Avatar performs when leaving. This makes him disappear after the teleport is over.
    /// </summary>
    public Vector2 DoBehavior_LeaveAfterPlayersAreDead_TeleportAction()
    {
        NPC.active = false;
        IProjOwnedByBoss<AvatarOfEmptiness>.KillAll();
        return NPC.Center;
    }

    public void DoBehavior_LeaveAfterPlayersAreDead()
    {
        RiftEclipseFogShaderData.FogDrawIntensityOverride *= 0.75f;
        AvatarOfEmptinessSky.Dimension = null;
        NPC.velocity *= 0.7f;
        LilyGlowIntensityBoost *= 0.7f;
        DistortionIntensity = 0f;
        IdealDistortionIntensity = 0f;

        if (AITimer >= 20)
            StartTeleportAnimation(DoBehavior_LeaveAfterPlayersAreDead_TeleportAction);
    }
}
