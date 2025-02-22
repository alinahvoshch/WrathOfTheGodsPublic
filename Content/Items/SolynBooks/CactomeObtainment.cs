using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// The chance for the Apple Thesis book to drop from a broken rolling cactus.
    /// </summary>
    public static int CactomeObtainmentDropChance => 50;

    private static void LoadCactomeObtainment()
    {
        GlobalProjectileEventHandlers.PreKillEvent += projectile =>
        {
            if (Main.netMode != NetmodeID.MultiplayerClient && projectile.type == ProjectileID.RollingCactus && Main.rand.NextBool(CactomeObtainmentDropChance))
                Item.NewItem(projectile.GetSource_Death(), projectile.Center, SolynBookAutoloader.Books["Cactome"].Type);

            return true;
        };
    }
}
