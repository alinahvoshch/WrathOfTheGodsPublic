using NoxusBoss.Content.Projectiles.Kites;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public class StarKite : ModItem
{
    public override string Texture => GetAssetPath("Content/Items", Name);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        ItemID.Sets.IsAKite[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultTokite(ModContent.ProjectileType<StarKiteProjectile>());
    }

    public static int TotalKitesOwnedByPlayer(Player player)
    {
        int kiteCount = 0;
        int kiteProjID = ModContent.ProjectileType<StarKiteProjectile>();
        foreach (Projectile p in Main.ActiveProjectiles)
        {
            if (p.type != kiteProjID || p.owner != player.whoAmI)
                continue;

            if (p.As<StarKiteProjectile>().Owner is not Player)
                continue;

            kiteCount++;
        }

        return kiteCount;
    }

    public override bool CanUseItem(Player player) => TotalKitesOwnedByPlayer(player) <= 0;
}
