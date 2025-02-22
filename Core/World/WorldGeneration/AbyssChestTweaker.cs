using NoxusBoss.Content.Items.SummonItems;
using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.CrossCompatibility.Inbound;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace NoxusBoss.Core.World.WorldGeneration;

public class AbyssChestTweaker : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        tasks.Add(new PassLegacy("Add Boss Rush Item", AddItemToAbyssChest));
    }

    public static void AddItemToAbyssChest(GenerationProgress progress, GameConfiguration config)
    {
        ModItem? terminus = null;
        if (!ModReferences.Calamity?.TryFind("Terminus", out terminus) ?? false)
            return;
        if (terminus is null)
            return;

        // Check all chests to see if they contain Terminus. If they do, also add Terminal and Absence Notice.
        int terminusID = terminus.Type;
        for (int i = 0; i < Main.maxChests; i++)
        {
            Chest c = Main.chest[i];
            if (c?.item.Any(s => s.stack >= 1 && s.type == terminusID) ?? false)
            {
                c.AddItemToShop(new Item(ModContent.ItemType<Terminal>()));

                if (SolynBookAutoloader.Books.TryGetValue("AbsenceNotice", out AutoloadableSolynBook? book))
                    c.AddItemToShop(new Item(book.Type));
            }
        }
    }
}
