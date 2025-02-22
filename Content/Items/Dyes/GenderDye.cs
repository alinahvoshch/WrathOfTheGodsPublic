using Luminance.Core.Graphics;
using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Dyes;

public class GenderDye : BaseDye
{
    public override void RegisterShader()
    {
        ManagedShader dyeShader = ShaderManager.GetShader("NoxusBoss.GenderDyeShader");
        dyeShader.CreateDyeBindings(Type);
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = Item.buyPrice(gold: 5);
        Item.rare = ModContent.RarityType<SolynRewardRarity>();
    }
}
