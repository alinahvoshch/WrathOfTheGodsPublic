using NoxusBoss.Content.Items;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Core.CrossCompatibility.Inbound.ModReferences;

namespace NoxusBoss.Core.CrossCompatibility.Inbound.CalamityHunt;

public class CalamityHuntCompatibilitySystem : ModSystem
{
    public override void PostSetupContent()
    {
        // Don't load anything if the calamity hunt mod is not enabled.
        if (CalamityHuntMod is null)
            return;

        AddAppleShimmer();
    }

    internal static void AddAppleShimmer()
    {
        // Make the good and bad apple interchangeable in shimmer.
        int badAppleID = CalamityHuntMod.Find<ModItem>("BadApple").Type;
        int goodAppleID = ModContent.ItemType<GoodApple>();
        ItemID.Sets.ShimmerTransformToItem[badAppleID] = goodAppleID;
        ItemID.Sets.ShimmerTransformToItem[goodAppleID] = badAppleID;
    }
}
