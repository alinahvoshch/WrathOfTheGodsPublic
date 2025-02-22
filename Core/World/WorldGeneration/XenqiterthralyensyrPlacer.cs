using NoxusBoss.Content.Items.Placeable;
using NoxusBoss.Content.Items.Placeable.Paintings;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace NoxusBoss.Core.World.WorldGeneration;

public class XenqiterthralyensyrPlacer : ModSystem
{
    /// <summary>
    /// The amount of Xenqiterthralyensyr paintings to add to the world in chests.
    /// </summary>
    public const int TotalPaintingsPerWorld = 1;

    /// <summary>
    /// The amount of Solyn statues to add to the world in chests.
    /// </summary>
    public const int TotalStatuesPerWorld = 8;

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        tasks.Add(new PassLegacy("Hiding forgotten artworks", AddXenqiterthralyensyrAndStatueToChests));
    }

    public static void AddXenqiterthralyensyrAndStatueToChests(GenerationProgress progress, GameConfiguration config)
    {
        AddRandomItemToChests(ModContent.ItemType<Xenqiterthralyensyr>(), TotalPaintingsPerWorld);
        AddRandomItemToChests(ModContent.ItemType<SolynStatue>(), TotalStatuesPerWorld);
    }

    public static void AddRandomItemToChests(int itemID, int totalPlacements)
    {
        // Attempt to place Xenqiterthralyensyr randomly into chests throughout the world.
        // This process ensures that the same chest doesn't get provided with the item twice.
        List<int> accessedChests = [];
        for (int tries = 0; tries < 1000; tries++)
        {
            int chestIndex = WorldGen.genRand.Next(Main.maxChests);
            Chest? c = Main.chest[chestIndex];
            if (c is null)
                continue;

            // Ignore chests that have no items, are invalid, or were already accessed.
            bool invalidChest = c.x == 0 && c.y == 0;
            if (invalidChest || !c.item.Any(i => !i.IsAir && i.stack >= 1) || accessedChests.Contains(chestIndex))
                continue;

            // Place the item in the chest.
            c.AddItemToShop(new Item(itemID));
            accessedChests.Add(chestIndex);

            // Stop if the amount of chest placements has exceeded the intended limit.
            if (accessedChests.Count >= totalPlacements)
                break;
        }
    }
}
