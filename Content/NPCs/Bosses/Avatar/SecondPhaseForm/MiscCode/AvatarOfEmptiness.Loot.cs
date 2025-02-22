using NoxusBoss.Content.Items;
using NoxusBoss.Content.Items.Accessories.VanityEffects;
using NoxusBoss.Content.Items.Dyes;
using NoxusBoss.Content.Items.HuntAuricSouls;
using NoxusBoss.Content.Items.LoreItems;
using NoxusBoss.Content.Items.MiscOPTools;
using NoxusBoss.Content.Items.Pets;
using NoxusBoss.Content.Items.Placeable.Trophies;
using NoxusBoss.Core.CrossCompatibility.Inbound;
using NoxusBoss.Core.CrossCompatibility.Inbound.BossChecklist;
using NoxusBoss.Core.DataStructures.DropRules;
using NoxusBoss.Core.Graphics.UI;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness : IBossChecklistSupport, IBossDowned
{
    public bool AutomaticallyRegisterDeathGlobally => true;

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        // Add the boss bag.
        npcLoot.Add(ItemDropRule.BossBag(TreasureBagID));

        // Define non-expert specific loot.
        LeadingConditionRule normalOnly = new LeadingConditionRule(new Conditions.NotExpert());
        {
            // General drops.
            normalOnly.OnSuccess(ItemDropRule.Common(ModContent.ItemType<PortalSkirt>()));
            normalOnly.OnSuccess(ItemDropRule.Common(ModContent.ItemType<EmptinessSprayer>()));
            normalOnly.OnSuccess(ItemDropRule.Common(ModContent.ItemType<DichromaticGlossDye>(), 1, 3, 5));
            normalOnly.OnSuccess(ItemDropRule.Common(ModContent.ItemType<EntropicDye>(), 1, 3, 5));
            normalOnly.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ParadiseDye>(), 1, 3, 5));
            normalOnly.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ScarletShadeDye>(), 1, 3, 5));
        }
        npcLoot.Add(normalOnly);

        // Define Rev/Master exclusive loot.
        LeadingConditionRule revOrMaster = new LeadingConditionRule(new RevengeanceOrMasterDropRule());
        {
            revOrMaster.OnSuccess(ItemDropRule.Common(RelicID));
            revOrMaster.OnSuccess(ItemDropRule.Common(ModContent.ItemType<OblivionChime>()));
        }
        npcLoot.Add(revOrMaster);

        // Lore items.
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LoreAvatar>()));

        // Hunt exclusive auric souls.
        LeadingConditionRule huntEnabled = new LeadingConditionRule(new HuntEnabledDropRule());
        {
            huntEnabled.OnSuccess(ItemDropRule.Common(ModContent.ItemType<AvatarAuricSoul>()));
        }
        npcLoot.Add(huntEnabled);

        // Vanity and decorations.
        npcLoot.Add(ItemDropRule.Common(MaskID, 7));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AvatarTrophy>(), 10));
    }

    public static void ModifyNPCBagLoot(ItemLoot bagLoot)
    {
        // General drops.
        bagLoot.Add(ItemDropRule.Common(ModContent.ItemType<PortalSkirt>()));
        bagLoot.Add(ItemDropRule.Common(ModContent.ItemType<EmptinessSprayer>()));
        bagLoot.Add(ItemDropRule.Common(ModContent.ItemType<DichromaticGlossDye>(), 1, 3, 5));
        bagLoot.Add(ItemDropRule.Common(ModContent.ItemType<EntropicDye>(), 1, 3, 5));
        bagLoot.Add(ItemDropRule.Common(ModContent.ItemType<ParadiseDye>(), 1, 3, 5));
        bagLoot.Add(ItemDropRule.Common(ModContent.ItemType<ScarletShadeDye>(), 1, 3, 5));
        bagLoot.Add(ItemDropRule.Common(ModContent.ItemType<MetallicChunk>()));
        bagLoot.Add(ItemDropRule.Common(ModContent.ItemType<NilkCarton>(), 1, 8, 12));
    }

    public void OnDefeat()
    {
        if (!Main.gameMenu && !Main.zenithWorld)
            TerminusHintAnimationSystem.Start();
    }

    public override void BossLoot(ref string name, ref int potionType)
    {
        potionType = ItemID.SuperHealingPotion;
        if (ModReferences.Calamity?.TryFind("OmegaHealingPotion", out ModItem potion) ?? false)
            potionType = potion.Type;
    }
}
