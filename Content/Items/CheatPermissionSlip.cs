using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public class CheatPermissionSlip : ModItem
{
    public override string Texture => GetAssetPath("Content/Items", Name);

    public static bool PlayerHasLegitimateSlip(Player p) => BossDownedSaveSystem.HasDefeated<NamelessDeityBoss>() && p.HasItem(ModContent.ItemType<CheatPermissionSlip>());

    public static bool PlayerHasIllegitimateSlip(Player p) => !BossDownedSaveSystem.HasDefeated<NamelessDeityBoss>() && p.HasItem(ModContent.ItemType<CheatPermissionSlip>());

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 22;
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        Item.value = 0;
    }
}
