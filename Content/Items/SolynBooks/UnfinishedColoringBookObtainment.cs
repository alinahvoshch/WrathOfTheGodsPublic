using Microsoft.Xna.Framework;
using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.World.WorldGeneration;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// The chance that the unfinished coloring book will spawn at Solyn's campsite at dawn.
    /// </summary>
    public static int UnfinishedColoringBookChance => 6;

    private static void TryToSpawnUnfinishedColoringBook()
    {
        if (SolynCampsiteWorldGen.CampSitePosition == Vector2.Zero)
            return;

        if (Distance((float)Main.time, 5f) <= 0.3f && Main.dayTime && Main.rand.NextBool(UnfinishedColoringBookChance))
        {
            int bookID = SolynBookAutoloader.Books["UnfinishedColoringBook"].Type;
            bool bookAlreadyPlaced = Main.item.Take(Main.maxItems).Any(i => i.active && i.type == bookID);
            Vector2 bookSpawnPosition = SolynCampsiteWorldGen.CampSitePosition + new Vector2(-92f, -56f);

            if (!bookAlreadyPlaced)
                Item.NewItem(new EntitySource_WorldEvent(), bookSpawnPosition, bookID);
        }
    }
}
