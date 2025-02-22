using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.World.GameScenes.SolynEventHandlers;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    [AutomatedMethodInvoke]
    public void LoadStateTransitions_FollowPlayerToCodebreaker()
    {
        StateMachine.RegisterTransition(SolynAIType.FollowPlayerToCodebreaker, SolynAIType.WalkHome, false, () =>
        {
            return DraedonCombatQuestSystem.Completed;
        });
        StateMachine.RegisterStateBehavior(SolynAIType.FollowPlayerToCodebreaker, DoBehavior_FollowPlayerToCodebreaker);
    }

    /// <summary>
    /// Performs Solyn's waiting behavior.
    /// </summary>
    public void DoBehavior_FollowPlayerToCodebreaker()
    {
        Player player = Main.player[Player.FindClosest(NPC.Center, 1, 1)];
        Vector2 flyDestination = player.Center + new Vector2(player.direction * -74f, -60f);
        Vector2 idealVelocity = (flyDestination - NPC.Center).ClampLength(0f, player.velocity.Length() + 5f);

        NPC.Center = Vector2.Lerp(NPC.Center, flyDestination, 0.0125f);
        NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, 0.05f).MoveTowards(idealVelocity, 0.2f);
        if (Vector2.Dot(NPC.velocity, idealVelocity) < 0f)
            NPC.velocity *= 0.74f;

        NPC.noGravity = true;
        NPC.spriteDirection = (player.Center.X - NPC.Center.X).NonZeroSign();

        CanBeSpokenTo = false;

        UseStarFlyEffects();
    }
}
