using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Dyes;

public class ScarletShadeDye : BaseDye
{
    public override void RegisterShader()
    {
        ManagedShader dyeShader = ShaderManager.GetShader("NoxusBoss.ScarletShadeDyeShader");
        dyeShader.TrySetParameter("uColor", new Vector3(0.64f, 0f, 0.16f));
        dyeShader.TrySetParameter("uSaturation", 0.06f);
        dyeShader.CreateDyeBindings(Type);
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.value = Item.sellPrice(0, 12, 0, 0);
    }
}
