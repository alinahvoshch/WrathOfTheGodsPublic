using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.Graphics.UI.SolynDialogue;
using NoxusBoss.Core.Pathfinding;
using NoxusBoss.Core.World.GameScenes.SolynEventHandlers;
using NoxusBoss.Core.World.WorldGeneration;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    [AutomatedMethodInvoke]
    public void LoadStateTransitions_WaitAtPermafrostKeep()
    {
        StateMachine.RegisterTransition(SolynAIType.WaitAtPermafrostKeep, SolynAIType.WalkAroundPermafrostKeep, false, () =>
        {
            return PermafrostKeepWorldGen.DoorHasBeenUnlocked && AITimer >= 70;
        });
        StateMachine.RegisterTransition(SolynAIType.TeleportFromPermafrostKeep, SolynAIType.StandStill, false, () =>
        {
            return AITimer >= 70;
        });
        StateMachine.RegisterStateBehavior(SolynAIType.WaitAtPermafrostKeep, DoBehavior_WaitAtPermafrostKeep);
        StateMachine.RegisterStateBehavior(SolynAIType.WalkAroundPermafrostKeep, DoBehavior_WalkAroundPermafrostKeep);
        StateMachine.RegisterStateBehavior(SolynAIType.TeleportFromPermafrostKeep, DoBehavior_TeleportFromPermafrostKeep);
    }

    /// <summary>
    /// Performs Solyn's waiting behavior at Permafrost's keep.
    /// </summary>
    public void DoBehavior_WaitAtPermafrostKeep()
    {
        CanBeSpokenTo = false;

        if (AITimer == 60)
        {
            Vector2 keepPosition = PermafrostKeepWorldGen.KeepArea.Center() * 16f + new Vector2(-510f, 48f);
            TeleportTo(keepPosition);
        }

        NPC.velocity.X = 0f;

        Player player = Main.player[Player.FindClosest(NPC.Center, 1, 1)];
        if (Collision.CanHit(NPC, player))
            NPC.spriteDirection = NPC.HorizontalDirectionTo(player.Center).NonZeroSign();
        else
            NPC.spriteDirection = 1;
    }

    /// <summary>
    /// Performs Solyn's walking behavior at Permafrost's keep.
    /// </summary>
    public void DoBehavior_WalkAroundPermafrostKeep()
    {
        CurrentConversation = SolynDialogRegistry.SolynQuest_DormantKey_AtSeed;

        Vector2 walkDestination = PermafrostKeepWorldGen.KeepArea.Center() * 16f + new Vector2(40f, 322f);
        List<Vector2> path = AStarPathfinding.PathfindThroughTiles(NPC.Center - Vector2.UnitX * NPC.spriteDirection * 10f, walkDestination, point =>
        {
            if (!WorldGen.InWorld(point.X, point.Y, 20))
                return 100f;

            return 0f;
        });
        Vector2 aheadPathfindingPoint = walkDestination;
        if (path.Count >= 6)
            aheadPathfindingPoint = path[5];

        bool reachedSeed = NPC.WithinRange(walkDestination, 80f);

        if (Abs(NPC.velocity.X) >= 0.1f)
            NPC.spriteDirection = NPC.velocity.X.NonZeroSign();

        DescendThroughSlopes = true;
        CanBeSpokenTo = reachedSeed;

        NPC.velocity.X = Lerp(NPC.velocity.X, NPC.SafeDirectionTo(aheadPathfindingPoint).X * 1.8f, 0.012f);
        if (reachedSeed)
            NPC.spriteDirection = NPC.HorizontalDirectionTo(walkDestination).NonZeroSign();

        DoBehavior_FollowPlayer_InteractWithDoors();
        PerformStandardFraming();
    }

    /// <summary>
    /// Performs Solyn's teleport-home behavior.
    /// </summary>
    public void DoBehavior_TeleportFromPermafrostKeep()
    {
        PermafrostKeepQuestSystem.Ongoing = false;
        PermafrostKeepQuestSystem.Completed = true;
        if (AITimer == 60)
        {
            TeleportTo(SolynCampsiteWorldGen.TentPosition);
            CurrentConversation = SolynDialogSystem.ChooseSolynConversation();
        }

        NPC.velocity.X = 0f;
    }
}
