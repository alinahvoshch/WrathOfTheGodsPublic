using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Accessories.VanityEffects;

public class DeificTouch : ModItem
{
    public static bool UsingEffect => !Main.gameMenu && Main.LocalPlayer.GetValueRef<bool>("DeificTouch") && NamelessDeityBoss.Myself is null;

    public override string Texture => "NoxusBoss/Assets/Textures/Content/Items/Accessories/VanityEffects/DeificTouch";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        PlayerDataManager.ResetEffectsEvent += ResetValue;
    }

    private void ResetValue(PlayerDataManager p)
    {
        p.GetValueRef<bool>("DeificTouch").Value = false;
    }

    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 36;
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (!hideVisual)
            player.GetValueRef<bool>("DeificTouch").Value = true;
    }

    public override void UpdateVanity(Player player) => player.GetValueRef<bool>("DeificTouch").Value = true;
}
