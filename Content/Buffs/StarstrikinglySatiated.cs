using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Buffs;

public class StarstrikinglySatiated : ModBuff
{
    public override string Texture => GetAssetPath("Content/Buffs", Name);

    public override void SetStaticDefaults()
    {
        On_Player.QuickBuff_FindFoodPriority += AddSpecialPriority;
        BuffID.Sets.IsWellFed[Type] = true;
    }

    private int AddSpecialPriority(On_Player.orig_QuickBuff_FindFoodPriority orig, Player self, int buffType)
    {
        if (buffType == Type)
            return 4;

        return orig(self, buffType);
    }

    public override void Update(Player player, ref int buffIndex)
    {
        /* Exquisitely stuffed stats, for reference.
        player.wellFed = true;
        player.statDefense += 4;
        player.meleeCrit += 4;
        player.meleeDamage += 0.1f;
        player.meleeSpeed += 0.1f;
        player.magicCrit += 4;
        player.magicDamage += 0.1f;
        player.rangedCrit += 4;
        player.rangedDamage += 0.1f;
        player.minionDamage += 0.1f;
        player.minionKB += 1f;
        player.moveSpeed += 0.4f;
        player.pickSpeed -= 0.15f;
        */

        player.wellFed = true;
        player.statDefense += 5;
        player.GetAttackSpeed<MeleeDamageClass>() += 0.125f;
        player.GetDamage<GenericDamageClass>() += 0.125f;
        player.GetCritChance<GenericDamageClass>() += 5f;
        player.GetKnockback<SummonDamageClass>() += 1f;

        player.moveSpeed += 0.45f;
        player.pickSpeed -= 0.2f;

        player.ClearBuff(BuffID.WellFed);
        player.ClearBuff(BuffID.WellFed2);
        player.ClearBuff(BuffID.WellFed3);
    }
}
