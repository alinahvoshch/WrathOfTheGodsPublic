using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Projectiles.Kites;
using NoxusBoss.Core.Fixes;
using NoxusBoss.Core.Graphics.UI.SolynDialogue;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    [AutomatedMethodInvoke]
    public void LoadStateTransitions_StandStill()
    {
        StateMachine.RegisterTransition(SolynAIType.StandStill, SolynAIType.WanderAbout, false, () =>
        {
            // Frequently attempt to do something else if possible, to minimize the amount of time just standing in place like a weirdo.
            return AITimer % SecondsToFrames(2f) == 0 && !LetOutKite && !CloseToPlayer && AITimer >= 10;
        });

        StateMachine.RegisterStateBehavior(SolynAIType.StandStill, DoBehavior_StandStill);
    }

    /// <summary>
    /// Performs Solyn's stand-still state.
    /// </summary>
    public void DoBehavior_StandStill()
    {
        // Horizontally decelerate.
        NPC.velocity.X *= 0.85f;

        // Keep the kite out.
        ReelInKite = false;

        // Look around occasionally.
        Player closest = Main.player[Player.FindClosest(NPC.Center, 1, 1)];
        int lookInOtherDirectionRate = SecondsToFrames(4f);
        float lookInOtherDirectionChance = 0.5f;
        if (AITimer % lookInOtherDirectionRate == 0 && Main.rand.NextBool(lookInOtherDirectionChance))
        {
            if (CloseToPlayer)
                NPC.spriteDirection = (closest.Center.X - NPC.Center.X).NonZeroSign();
            else
                NPC.spriteDirection *= -1;

            NPC.netUpdate = true;
        }

        // Let out a kite on a windy day.
        bool windyDayConversation = CurrentConversation == SolynDialogRegistry.SolynWindyDay;
        if (windyDayConversation && !LetOutKite && Main.myPlayer == closest.whoAmI)
        {
            ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(kite => kite.ai[0] = NPC.whoAmI + 500);
            NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY * -3f, ModContent.ProjectileType<StarKiteProjectile>(), 0, 0f, -1, NPC.whoAmI + 500);
            LetOutKite = true;
        }

        PerformStandardFraming();
    }
}
