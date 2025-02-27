using NoxusBoss.Core.GlobalInstances;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using static NoxusBoss.Core.Autoloaders.SolynBooks.SolynBookAutoloader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// The chance of an Ominous Storybook dropping from any non-statue enemy.
    /// </summary>
    public static int OminousStorybookDropChance => 23000;

    private static void LoadOminousStorybookObtainment()
    {
        GlobalNPCEventHandlers.ModifyGlobalLootEvent += loot =>
        {
            LeadingConditionRule notFromStatue = new LeadingConditionRule(new Conditions.NotFromStatue());
            {
                notFromStatue.OnSuccess(new CommonDrop(Books["OminousStorybook"].Type, OminousStorybookDropChance));
            }

            loot.Add(notFromStatue);
        };
    }
}
