using CalamityMod.Events;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.SummonItems;

[JITWhenModsEnabled(CalamityCompatibility.ModName)]
[ExtendsFromMod(CalamityCompatibility.ModName)]
public class Terminal : ModItem
{
    public override string Texture => GetAssetPath("Content/Items/SummonItems", Name);

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults()
    {
        Item.width = 58;
        Item.height = 70;
        Item.useAnimation = 40;
        Item.useTime = 40;
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = null;
        Item.value = 0;
        Item.rare = ItemRarityID.Blue;
    }

    public override bool? UseItem(Player player)
    {
        if (BossRushEvent.BossRushActive)
            BossRushEvent.End();
        else
        {
            BossRushEvent.SyncStartTimer(120);

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active)
                    continue;

                // Will also correctly despawn EoW because none of his segments are boss flagged.
                bool shouldDespawn = n.boss || n.type == NPCID.EaterofWorldsHead || n.type == NPCID.EaterofWorldsBody || n.type == NPCID.EaterofWorldsTail;
                if (shouldDespawn)
                {
                    n.active = false;
                    n.netUpdate = true;
                }
            }

            BossRushEvent.BossRushStage = 0;
            BossRushEvent.BossRushActive = true;
        }

        return true;
    }
}
