using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.World.Subworlds;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public class StarCharm : ModItem
{
    public override string Texture => GetAssetPath("Content/Items", Name);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        PlayerDataManager.PostUpdateEvent += ProvideLuck;
    }

    private void ProvideLuck(PlayerDataManager p)
    {
        if (p.Player.HasItem(Type))
            p.Player.luck += 0.25f;
    }

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 44;
        Item.rare = ModContent.RarityType<SolynRewardRarity>();
    }

    public override void UpdateInventory(Player player)
    {
        if (Main.instance.IsActive && Main.hasFocus && !Main.dayTime && Main.rand.NextBool(400) && !EternalGardenUpdateSystem.WasInSubworldLastUpdateFrame)
            Terraria.Star.StarFall(player.Center.X);
    }
}
