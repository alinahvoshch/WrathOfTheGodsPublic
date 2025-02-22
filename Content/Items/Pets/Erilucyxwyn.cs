using Microsoft.Xna.Framework;
using NoxusBoss.Content.Projectiles.Pets;
using NoxusBoss.Core.Autoloaders;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Pets;

public class Erilucyxwyn : ModItem
{
    /// <summary>
    /// The buff ID associated with this lotus.
    /// </summary>
    public static int BuffID
    {
        get;
        private set;
    }

    public override string Texture => GetAssetPath("Content/Items/Pets", Name);

    public override void Load() => BuffID = PetBuffAutoloader.Create(Mod, "NoxusBoss/BabyNameless", "BabyNamelessBuff");

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.ZephyrFish);
        Item.shoot = ModContent.ProjectileType<BabyNameless>();
        Item.buffType = BuffID;
        Item.master = true;
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            player.AddBuff(Item.buffType, 3600);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // The item applies the buff, the buff spawns the projectile.
        player.AddBuff(Item.buffType, 2);
        return false;
    }
}
