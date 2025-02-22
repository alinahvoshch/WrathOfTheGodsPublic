using NoxusBoss.Core.CrossCompatibility.Inbound;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon;

public partial class MarsBody : ModNPC
{
    public override void BossLoot(ref string name, ref int potionType)
    {
        potionType = ItemID.SuperHealingPotion;
        if (ModReferences.Calamity?.TryFind("OmegaHealingPotion", out ModItem potion) ?? false)
            potionType = potion.Type;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        if (ModLoader.TryGetMod("EverquartzAdventure", out Mod deimosMod) && deimosMod.TryFind("MarsBar", out ModItem marsBar))
        {
            DropOneByOne.Parameters marsBarSpam = new DropOneByOne.Parameters()
            {
                ChanceNumerator = 1,
                ChanceDenominator = 1,
                MinimumStackPerChunkBase = 20,
                MaximumStackPerChunkBase = 32,
                MinimumItemDropsCount = 50, // 20 * 50 = 1000
                MaximumItemDropsCount = 100, // 32 * 100 = 3200
            };
            npcLoot.Add(new DropOneByOne(marsBar.Type, marsBarSpam));
        }
    }
}
