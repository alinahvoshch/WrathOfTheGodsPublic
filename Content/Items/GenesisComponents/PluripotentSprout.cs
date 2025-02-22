using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.GenesisComponents;

public class PluripotentSprout : ModItem
{
    public override string Texture => $"NoxusBoss/Assets/Textures/Content/Items/GenesisComponents/{Name}";

    public override bool IsLoadingEnabled(Mod mod) => ModLoader.TryGetMod("CalamityHunt", out _) && false;

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        GlobalNPCEventHandlers.ModifyNPCLootEvent += MakeGoozmaDropSprout;
    }

    private void MakeGoozmaDropSprout(NPC npc, NPCLoot npcLoot)
    {
        if (ModLoader.TryGetMod("CalamityHunt", out Mod hunt) && hunt.TryFind("Goozma", out ModNPC goozma) && npc.type == goozma.Type)
            npcLoot.Add(new CommonDrop(Type, 1));
    }

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.value = 0;
        Item.rare = ModContent.RarityType<GenesisComponentRarity>();
        Item.Wrath().GenesisComponent = true;
    }
}
