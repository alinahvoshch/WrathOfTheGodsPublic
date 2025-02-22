using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.GlobalInstances;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    private static void LoadExoBlueprintsObtainment()
    {
        if (!CalamityCompatibility.Enabled || !CalamityCompatibility.Calamity.TryFind("Draedon", out ModNPC draedon))
            return;

        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, loot) =>
        {
            if (npc.type == draedon.Type)
                loot.Add(new CommonDrop(SolynBookAutoloader.Books["ExoBlueprints"].Type, 1));
        };
        GlobalNPCEventHandlers.CheckDeadEvent += npc =>
        {
            if (npc.type == draedon.Type)
                npc.NPCLoot();

            return true;
        };
    }
}
