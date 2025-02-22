using Microsoft.Xna.Framework;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.World.GameScenes.RiftEclipse;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Fishing;

public class Veilray : ModItem
{
    public override string Texture => "NoxusBoss/Assets/Textures/Content/Items/Fishing/Veilray";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 2;
        ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true; // All vanilla fish can be placed in a weapon rack.
        PlayerDataManager.CatchFishEvent += AddCatchCondition;
    }

    private void AddCatchCondition(PlayerDataManager p, FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
    {
        if (attempt.questFish == Type && attempt.uncommon && p.Player.ZoneBeach)
            itemDrop = Type;
    }

    public override void SetDefaults()
    {
        // DefaultToQuestFish sets quest fish properties.
        // Of note, it sets rare to ItemRarityID.Quest, which is the special rarity for quest items.
        // It also sets uniqueStack to true, which prevents players from picking up a 2nd copy of the item into their inventory.
        Item.DefaultToQuestFish();
    }

    public override bool IsQuestFish() => true;

    public override bool IsAnglerQuestAvailable() => RiftEclipseManagementSystem.RiftEclipseOngoing;

    public override void AnglerQuestChat(ref string description, ref string catchLocation)
    {
        description = this.GetLocalizedValue("QuestDescription");
        catchLocation = this.GetLocalizedValue("QuestionCatchLocation");
    }
}
