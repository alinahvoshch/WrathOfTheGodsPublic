using NoxusBoss.Core.World.WorldGeneration;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public class DormantKey : ModItem
{
    public override string Texture => GetAssetPath("Content/Items", Name);

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 0;

    public override void SetDefaults()
    {
        Item.width = 26;
        Item.height = 48;
        Item.value = 0;
        Item.rare = ItemRarityID.Pink;
    }

    public override void UpdateInventory(Player player)
    {
        player.GetValueRef<bool>(PermafrostKeepWorldGen.PlayerWasGivenKeyVariableName).Value = true;
    }
}
