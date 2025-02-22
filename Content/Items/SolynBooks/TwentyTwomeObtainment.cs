using NoxusBoss.Core.GlobalInstances;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Core.Autoloaders.SolynBooks.SolynBookAutoloader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// The chance that a Twenty Twome will drop from a hornet.
    /// </summary>
    public static int TwentyTwomeHornetDropChance => 444;

    /// <summary>
    /// The chance that a Twenty Twome will drop from a moss hornet.
    /// </summary>
    public static int TwentyTwomeMossHornetDropChance => 222;

    private static void LoadTwentyTwomeObtainment()
    {
        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, loot) =>
        {
            if (npc.type == NPCID.Hornet)
                loot.Add(new CommonDrop(Books["TwentyTwome"].Type, TwentyTwomeHornetDropChance));
            if (npc.type == NPCID.MossHornet)
                loot.Add(new CommonDrop(Books["TwentyTwome"].Type, TwentyTwomeMossHornetDropChance));
        };
    }
}
