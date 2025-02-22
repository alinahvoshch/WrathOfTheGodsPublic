using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Core.World.WorldSaving;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

namespace NoxusBoss.Core.DataStructures.DropRules;

public class BeforeNamelessDefeatedDropRule : IItemDropRuleCondition, IProvideItemConditionDescription
{
    public bool CanDrop(DropAttemptInfo info) => !BossDownedSaveSystem.HasDefeated<NamelessDeityBoss>();

    public bool CanShowItemDropInUI() => true;

    public string GetConditionDescription()
    {
        return Language.GetTextValue("Mods.NoxusBoss.Bestiary.ItemDropConditions.FirstTimeExclusive");
    }
}
