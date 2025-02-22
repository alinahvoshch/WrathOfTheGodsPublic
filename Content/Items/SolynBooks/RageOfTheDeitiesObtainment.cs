using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Core.Autoloaders.SolynBooks.SolynBookAutoloader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    private static void LoadRageOfTheDeitiesObtainment()
    {
        CalamityCompatibility.Calamity?.Call("MakeItemExhumable", ItemID.SpellTome, Books["RageOfTheDeities"].Type);
    }
}
