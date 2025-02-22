using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    [AutomatedMethodInvoke]
    public void LoadState_ReturnPlayersFromMyUniverse()
    {
        StateMachine.RegisterTransition(AvatarAIType.ReturnPlayersFromMyUniverse, null, false, () =>
        {
            return AITimer >= 4;
        });

        // Load the AI state behavior.
        StateMachine.RegisterStateBehavior(AvatarAIType.ReturnPlayersFromMyUniverse, DoBehavior_ReturnPlayersFromMyUniverse);

        StatesToNotStartTeleportDuring.Add(AvatarAIType.ReturnPlayersFromMyUniverse);
    }

    public void DoBehavior_ReturnPlayersFromMyUniverse()
    {
        AvatarOfEmptinessSky.Dimension = null;

        foreach (Player player in Main.ActivePlayers)
        {
            player.Center = TargetPositionBeforeDimensionShift;
        }

        NPC.Center = TargetPositionBeforeDimensionShift - Vector2.UnitY * 390f;
        LeftArmPosition = NPC.Center;
        RightArmPosition = NPC.Center;
        HeadPosition = NPC.Center;
    }
}
