using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Graphics;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Content.Items;

public class GoodApple : ModItem
{
    public const string TotalApplesConsumedFieldName = "TotalGoodApplesConsumed";

    public static readonly SoundStyle AppleBiteSound = new SoundStyle("NoxusBoss/Assets/Sounds/Item/AppleBite", new ReadOnlySpan<(int variant, float weight)>(new[]
    {
        (1, 1f),
        (2, 1f),
        (3, 0.005f) // Was that the bite of 87?
    }));

    public override string Texture => GetAssetPath("Content/Items", Name);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        ItemID.Sets.SortingPriorityBossSpawns[Type] = 20;

        PlayerDataManager.MaxStatsEvent += ApplyHealthBoosts;
        PlayerDataManager.SaveDataEvent += SaveAppleCount;
        PlayerDataManager.LoadDataEvent += LoadAppleCount;
    }

    private void ApplyHealthBoosts(PlayerDataManager p, ref StatModifier health, ref StatModifier mana)
    {
        health.Base += p.GetValueRef<int>(TotalApplesConsumedFieldName);
    }

    private void LoadAppleCount(PlayerDataManager p, TagCompound tag)
    {
        p.GetValueRef<int>(TotalApplesConsumedFieldName).Value = tag.GetInt(TotalApplesConsumedFieldName);
    }

    private void SaveAppleCount(PlayerDataManager p, TagCompound tag)
    {
        tag[TotalApplesConsumedFieldName] = p.GetValueRef<int>(TotalApplesConsumedFieldName).Value;
    }

    public override void SetDefaults()
    {
        Item.maxStack = 99999;
        Item.consumable = true;
        Item.DefaultToFood(22, 22, 0, 0, false, 15);
        Item.width = 30;
        Item.height = 30;
        Item.UseSound = Main.zenithWorld ? (GennedAssets.Sounds.Item.NamelessGoesInsaneOverGoodApple with { SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }) : AppleBiteSound;
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        Item.value = 0;
    }

    public override bool? UseItem(Player player)
    {
        if (player.itemAnimation > 0 && player.itemTime == 0)
        {
            player.UseHealthMaxIncreasingItem(1);
            player.GetValueRef<int>(TotalApplesConsumedFieldName).Value++;
        }
        return true;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        // Rewrite tooltips post-Nameless Deity.
        if (BossDownedSaveSystem.HasDefeated<NamelessDeityBoss>())
        {
            // Remove the default tooltips.
            tooltips.RemoveAll(t => t.Name.Contains("Tooltip"));

            // Generate and use custom tooltips.
            string specialTooltip = this.GetLocalizedValue("TooltipPostNamelessDeity");
            TooltipLine[] tooltipLines = specialTooltip.Split('\n').Select((t, index) =>
            {
                return new TooltipLine(Mod, $"NamelessDeityTooltip{index + 1}", t);
            }).ToArray();

            // Color the last tooltip line.
            tooltipLines.Last().OverrideColor = DialogColorRegistry.NamelessDeityTextColor;
            tooltips.AddRange(tooltipLines);
            return;
        }

        // Make the final tooltip line about needing to pass the test use Nameless' dialog.
        TooltipLine? tooltip = tooltips.FirstOrDefault(t => t.Name == "Tooltip1");
        if (tooltip is not null)
            tooltip.OverrideColor = DialogColorRegistry.NamelessDeityTextColor;
    }

    public override bool CanUseItem(Player player)
    {
        // Prevent the consumption of the apples until the player has maxed out all vanilla max life boosters.
        if (player.ConsumedLifeCrystals < Player.LifeCrystalMax || player.ConsumedLifeFruit < Player.LifeFruitMax)
            return false;

        // Prevent the consumption of the apples until to the player has defeated Nameless.
        return BossDownedSaveSystem.HasDefeated<NamelessDeityBoss>();
    }
}
