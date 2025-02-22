using NoxusBoss.Content.Buffs;
using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public class NilkCarton : ModItem
{
    /// <summary>
    /// How long the frightening effects of the nilk last.
    /// </summary>
    public static int DebuffDuration => MinutesToFrames(60f);

    public override string Texture => GetAssetPath("Content/Items", Name);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        ItemID.Sets.SortingPriorityBossSpawns[Type] = 20;

        On_Player.QuickBuff_ShouldBotherUsingThisBuff += PreventAutobuffGrantingNilk;
    }

    private static bool PreventAutobuffGrantingNilk(On_Player.orig_QuickBuff_ShouldBotherUsingThisBuff orig, Player self, int buffID)
    {
        if (buffID == ModContent.BuffType<NilkDebuff>())
            return false;

        return orig(self, buffID);
    }

    public override void SetDefaults()
    {
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.DefaultToFood(40, 44, ModContent.BuffType<NilkDebuff>(), DebuffDuration, true);
        Item.width = 24;
        Item.height = 36;
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.value = 0;
    }

    public override bool CanUseItem(Player player) => !player.HasBuff<NilkDebuff>();
}
