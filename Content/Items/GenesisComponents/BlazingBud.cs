using CalamityMod.Items.Materials;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles.GenesisComponents;
using NoxusBoss.Core.CrossCompatibility.Inbound;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.GenesisComponents;

public class BlazingBud : ModItem
{
    public override string Texture => $"NoxusBoss/Assets/Textures/Content/Items/GenesisComponents/{Name}";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        ItemID.Sets.ItemNoGravity[Type] = true;

        GlobalNPCEventHandlers.ModifyNPCLootEvent += MakeCalamitasDropBud;
    }

    private void MakeCalamitasDropBud(NPC npc, NPCLoot npcLoot)
    {
        if (!ModLoader.TryGetMod(CalamityCompatibility.ModName, out Mod calamityMod))
            return;

        if (!calamityMod.TryFind("SupremeCalamitas", out ModNPC calamitas))
            return;

        if (npc.type != calamitas.Type)
            return;

        npcLoot.Add(new CommonDrop(Type, 1));
    }

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.value = 0;
        Item.rare = ModContent.RarityType<GenesisComponentRarity>();
        Item.DefaultToPlaceableTile(ModContent.TileType<BlazingBudTile>());
        Item.Wrath().GenesisComponent = true;
    }

    [JITWhenModsEnabled(CalamityCompatibility.ModName)]
    public override void AddRecipes()
    {
        CreateRecipe(1).
            AddTile(TileID.WorkBenches).
            AddIngredient<AshesofAnnihilation>(5).
            AddIngredient(ItemID.Seed).
            AddCondition(Language.GetText("Mods.NoxusBoss.Conditions.ObtainedBefore"), () => CommonCalamityVariables.CalamitasDefeated).
            Register();
    }
}
