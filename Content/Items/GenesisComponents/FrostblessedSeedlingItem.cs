using CalamityMod.Items.Materials;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles.GenesisComponents;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.SolynEvents;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.GenesisComponents;

public class FrostblessedSeedlingItem : ModItem
{
    public override string Texture => $"NoxusBoss/Assets/Textures/Content/Items/GenesisComponents/{Name}";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 40;
        Item.value = 0;
        Item.rare = ModContent.RarityType<GenesisComponentRarity>();
        Item.DefaultToPlaceableTile(ModContent.TileType<GenesisTile>());
        Item.Wrath().GenesisComponent = true;
    }

    [JITWhenModsEnabled(CalamityCompatibility.ModName)]
    public override void AddRecipes()
    {
        CreateRecipe(1).
            AddTile(TileID.WorkBenches).
            AddIngredient<CryonicBar>(5).
            AddIngredient(ItemID.Seed).
            AddCondition(Language.GetText("Mods.NoxusBoss.Conditions.ObtainedBefore"), () => ModContent.GetInstance<PermafrostKeepEvent>().Finished).
            Register();
    }
}
