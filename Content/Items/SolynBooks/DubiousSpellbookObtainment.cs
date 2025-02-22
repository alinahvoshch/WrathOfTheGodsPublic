using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.GlobalInstances;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// The chance for Tim and Rune Wizards to drop dubious spellbooks.
    /// </summary>
    public static int DubiousSpellbookDropChance => 2;

    private static void LoadDubiousSpellbookObtainment()
    {
        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, loot) =>
        {
            if (npc.type == NPCID.Tim || npc.type == NPCID.RuneWizard)
                loot.Add(new CommonDrop(SolynBookAutoloader.Books["DubiousSpellbook"].Type, DubiousSpellbookDropChance));
        };
    }
}
