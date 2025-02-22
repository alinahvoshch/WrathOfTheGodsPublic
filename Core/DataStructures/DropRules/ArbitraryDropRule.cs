using Terraria.GameContent.ItemDropRules;

namespace NoxusBoss.Core.DataStructures.DropRules;

public class ArbitraryDropRule : IItemDropRuleCondition, IProvideItemConditionDescription
{
    private readonly Func<DropAttemptInfo, bool> condition;

    private readonly bool visibleInUI;

    private readonly string? description;

    internal ArbitraryDropRule(Func<DropAttemptInfo, bool> lambda, bool ui = true, string? desc = null)
    {
        condition = lambda;
        visibleInUI = ui;
        description = desc;
    }

    public bool CanDrop(DropAttemptInfo info) => condition(info);

    public bool CanShowItemDropInUI() => visibleInUI;

    public string? GetConditionDescription() => description;
}
