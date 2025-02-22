using NoxusBoss.Core.CrossCompatibility.Inbound;
using NoxusBoss.Core.CrossCompatibility.Inbound.Infernum;
using Terraria.GameContent.ItemDropRules;

namespace NoxusBoss.Core.DataStructures.DropRules;

public class InfernumActiveDropRule : IItemDropRuleCondition
{
    public bool CanDrop(DropAttemptInfo info) => InfernumCompatibilitySystem.InfernumModeIsActive;

    public bool CanShowItemDropInUI() => ModReferences.InfernumMod is not null;

    public string GetConditionDescription() => string.Empty;
}
