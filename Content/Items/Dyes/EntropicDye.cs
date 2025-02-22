using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Rarities;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Dyes;

public class EntropicDye : BaseDye
{
    public static readonly Color BaseShaderColor = new Color(72, 48, 122);

    public override void RegisterShader()
    {
        Asset<Texture2D> dyeTexture = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Content/Items/Dyes/EntropicDyeTexture", AssetRequestMode.ImmediateLoad);
        ManagedShader dyeShader = ShaderManager.GetShader("NoxusBoss.EntropicDyeShader");
        dyeShader.TrySetParameter("uColor", BaseShaderColor.ToVector3());
        dyeShader.SetTexture(dyeTexture, 1, SamplerState.LinearWrap);
        dyeShader.CreateDyeBindings(Type);
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.value = Item.sellPrice(0, 12, 0, 0);
    }
}
