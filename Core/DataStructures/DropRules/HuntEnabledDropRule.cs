using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace NoxusBoss.Core.DataStructures.DropRules;

public class HuntEnabledDropRule : IItemDropRuleCondition, IProvideItemConditionDescription
{
    public bool CanDrop(DropAttemptInfo info) => ModLoader.HasMod("CalamityHunt");

    public bool CanShowItemDropInUI() => false;

    public string GetConditionDescription() => "";
}
