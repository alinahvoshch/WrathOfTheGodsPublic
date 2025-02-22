using NoxusBoss.Core.Autoloaders.SolynBooks;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    private static void LoadInvisibleInkDissertationObtainment()
    {
        ItemID.Sets.ShimmerTransformToItem[ItemID.Book] = SolynBookAutoloader.Books["InvisibleInkDissertation"].Type;
    }
}
