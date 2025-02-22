using NoxusBoss.Core.DataStructures.Conditions;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.HairStyles;

public class SolynHairStyle : ModHair
{
    public override string Texture => GetAssetPath("HairStyles", Name);

    public override string AltTexture => GetAssetPath("HairStyles", $"{Name}_Alt");

    public override bool AvailableDuringCharacterCreation => false;

    public override IEnumerable<Condition> GetUnlockConditions()
    {
        yield return CustomConditions.SolynHasAppeared;
    }
}
