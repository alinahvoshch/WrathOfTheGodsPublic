using Luminance.Core.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    public bool CloseToPlayer => Main.player[Player.FindClosest(NPC.Center, 1, 1)].WithinRange(NPC.Center, 150f);

    public void DoBehavior_StandStill()
    {
        if (ShouldGoEepy)
        {
            SwitchState(SolynAIType.EnterTentToSleep);
            return;
        }

        if (AITimer % 120 == 119 && !CloseToPlayer)
            SwitchState(SolynAIType.WanderAbout);

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

        // TODO -- Let out a kite on a windy day.
        /*
        bool windyDayConversation = CurrentConversation == SolynDialogRegistry.SolynWindyDay;
        if (windyDayConversation && !LetOutKite && Main.myPlayer == closest.whoAmI)
        {
            ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(kite => kite.ai[0] = NPC.whoAmI + 500);
            NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY * -3f, ModContent.ProjectileType<StarKiteProjectile>(), 0, 0f, -1, NPC.whoAmI + 500);
            LetOutKite = true;
        }
        */

        PerformStandardFraming();
    }
}
