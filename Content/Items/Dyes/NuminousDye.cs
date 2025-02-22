using Luminance.Core.Graphics;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Rarities;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Dyes;

public class NuminousDye : BaseDye
{
    internal static Asset<Texture2D> ShaderTexture;

    public override void RegisterShader()
    {
        ShaderTexture = ModContent.Request<Texture2D>(GetAssetPath("Content/Items/Dyes", "NuminousDyeTexture"), AssetRequestMode.ImmediateLoad);
        ManagedShader dyeShader = ShaderManager.GetShader("NoxusBoss.NuminousDyeShader");
        dyeShader.SetTexture(ShaderTexture, 1, SamplerState.LinearWrap);
        dyeShader.CreateDyeBindings(Type);
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        Item.value = Item.sellPrice(0, 15, 0, 0);
    }
}
