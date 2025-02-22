using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public class MetallicChunk : ModItem
{
    public override string Texture => GetAssetPath("Content/Items", Name);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.width = 26;
        Item.height = 26;
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.value = 0;
    }

    public override void AddRecipes()
    {
        Recipe.Create(ItemID.IronBar).
            AddTile(TileID.Furnaces).
            AddIngredient(Type, 3).
            Register();
    }
}
