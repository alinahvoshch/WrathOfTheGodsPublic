using Luminance.Core.Graphics;
using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Dyes;

public class ParadiseDye : BaseDye
{
    public override void RegisterShader()
    {
        ManagedShader dyeShader = ShaderManager.GetShader("NoxusBoss.ParadiseDyeShader");
        dyeShader.CreateDyeBindings(Type);
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.value = Item.sellPrice(0, 12, 0, 0);
    }
}
