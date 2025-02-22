using Microsoft.Xna.Framework;
using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Fishing;

public class Dreamcatcher : ModItem
{
    public override string Texture => "NoxusBoss/Assets/Textures/Content/Items/Fishing/Dreamcatcher";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        ItemID.Sets.CanFishInLava[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.WoodFishingPole);
        Item.fishingPole = 65;
        Item.shootSpeed = 16.5f;
        Item.shoot = ModContent.ProjectileType<DreamcatcherBobber>();
        Item.rare = ModContent.RarityType<SolynRewardRarity>();
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        for (int i = (int)DreamcatcherBobber.CatchType.RegularFish; i < (int)DreamcatcherBobber.CatchType.Count; i++)
        {
            Vector2 bobberVelocity = velocity + Vector2.UnitX * i * 3.4f;
            Projectile.NewProjectile(source, position, bobberVelocity, type, 0, 0f, player.whoAmI, 0f, 0f, i);
        }

        return false;
    }

    public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
    {
        lineOriginOffset = new Vector2(48f, -30f);
        lineColor = Color.Lerp(Color.HotPink, Color.Yellow, Cos01(TwoPi * bobber.ai[2] / 3f + Main.GlobalTimeWrappedHourly * 0.6f));
    }
}
