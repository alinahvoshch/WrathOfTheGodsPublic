using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

namespace NoxusBoss.Core.DataStructures.DropRules;

public class GFBDropRule : IItemDropRuleCondition, IProvideItemConditionDescription
{
    public bool CanDrop(DropAttemptInfo info) => Main.zenithWorld;

    public bool CanShowItemDropInUI() => Main.zenithWorld;

    public string GetConditionDescription() => Language.GetTextValue("Mods.NoxusBoss.Conditions.IsGFB");
}
