using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Dyes;

public class DichromaticGlossDye : BaseDye
{
    public override void RegisterShader()
    {
        ManagedShader dyeShader = ShaderManager.GetShader("NoxusBoss.DichromaticGlossDyeShader");
        dyeShader.TrySetParameter("uColor", new Vector3(0f, 0.56f, 1f));
        dyeShader.TrySetParameter("uSecondaryColor", new Vector3(1f, 0f, 0.74f));
        dyeShader.TrySetParameter("uSaturation", 0.6f);
        dyeShader.CreateDyeBindings(Type);
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.value = Item.sellPrice(0, 12, 0, 0);
    }
}
