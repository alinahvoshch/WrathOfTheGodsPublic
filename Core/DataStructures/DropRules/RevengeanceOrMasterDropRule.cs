using NoxusBoss.Core.CrossCompatibility.Inbound;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

namespace NoxusBoss.Core.DataStructures.DropRules;

public class RevengeanceOrMasterDropRule : IItemDropRuleCondition, IProvideItemConditionDescription
{
    public bool CanDrop(DropAttemptInfo info)
    {
        return Main.masterMode || CommonCalamityVariables.RevengeanceModeActive;
    }

    public bool CanShowItemDropInUI()
    {
        return Main.masterMode || CommonCalamityVariables.RevengeanceModeActive;
    }

    public string GetConditionDescription()
    {
        if (ModReferences.Calamity is null)
            return Language.GetTextValue("Bestiary_ItemDropConditions.IsMasterMode");

        if (!Main.masterMode)
            return Language.GetTextValue("Mods.CalamityMod.Condition.InRev");

        return Language.GetTextValue("Bestiary_ItemDropConditions.IsMasterMode");
    }
}
