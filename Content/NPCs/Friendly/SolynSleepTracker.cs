using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public class SolynSleepTracker : ModSystem
{
    /// <summary>
    /// Whether Solyn is currently asleep or not.
    /// </summary>
    public static bool SolynIsAsleep
    {
        get;
        private set;
    }

    public override void PostUpdateNPCs()
    {
        SolynIsAsleep = false;

        int solynIndex = NPC.FindFirstNPC(ModContent.NPCType<Solyn>());
        if (solynIndex == -1)
            return;

        SolynIsAsleep = Main.npc[solynIndex].As<Solyn>().CurrentState == SolynAIType.Eepy;
    }
}
