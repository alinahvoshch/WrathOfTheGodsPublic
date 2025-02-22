using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Accessories.VanityEffects;

public class PortalSkirt : ModItem
{
    /// <summary>
    /// The render target that holds the portal render contents.
    /// </summary>
    public static ManagedRenderTarget PortalTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The shader index for the portal skirt.
    /// </summary>
    public static int SkirtShaderIndex
    {
        get;
        private set;
    }

    public const string WearingPortalSkirtVariableName = "WearingPortalSkirt";

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        ItemID.Sets.ItemNoGravity[Type] = true;

        PlayerDataManager.ResetEffectsEvent += ResetSkirt;
        PlayerDataManager.PlayerModifyDrawInfoEvent += HideLegs;
        On_Player.UpdateItemDye += FindSkirtItemDyeShader;

        if (Main.netMode == NetmodeID.Server)
            return;

        PortalTarget = new ManagedRenderTarget(false, (_, _2) => new RenderTarget2D(Main.instance.GraphicsDevice, 208, 100));
        RenderTargetManager.RenderTargetUpdateLoopEvent += RenderPortalTarget;
    }

    private void HideLegs(PlayerDataManager p, ref PlayerDrawSet drawInfo)
    {
        if (!p.GetValueRef<bool>(WearingPortalSkirtVariableName))
            return;

        drawInfo.colorLegs = Color.Transparent;
        drawInfo.colorArmorLegs = Color.Transparent;
    }

    private void ResetSkirt(PlayerDataManager p) => p.GetValueRef<bool>(WearingPortalSkirtVariableName).Value = false;

    private void RenderPortalTarget()
    {
        if (!Main.LocalPlayer.TryGetModPlayer(out PlayerDataManager p))
            return;

        if (!p.GetValueRef<bool>(WearingPortalSkirtVariableName))
            return;

        var gd = Main.instance.GraphicsDevice;
        gd.SetRenderTarget(PortalTarget);
        gd.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null);

        Texture2D innerRiftTexture = GennedAssets.Textures.FirstPhaseForm.RiftInnerTexture.Value;
        var riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
        riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.1f);
        riftShader.TrySetParameter("baseCutoffRadius", 0.1f);
        riftShader.TrySetParameter("swirlOutwardnessExponent", 0.151f);
        riftShader.TrySetParameter("swirlOutwardnessFactor", 3.67f);
        riftShader.TrySetParameter("vanishInterpolant", 0f);
        riftShader.TrySetParameter("edgeColor", new Vector4(1f, 0.08f, 0.08f, 1f));
        riftShader.TrySetParameter("edgeColorBias", 0.15f);
        riftShader.SetTexture(WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
        riftShader.SetTexture(WavyBlotchNoise, 2, SamplerState.AnisotropicWrap);
        riftShader.Apply();

        Main.spriteBatch.Draw(innerRiftTexture, PortalTarget.Size() * 0.5f, null, new Color(77, 0, 2), 0f, innerRiftTexture.Size() * 0.5f, PortalTarget.Size() / innerRiftTexture.Size() * 0.99f, 0, 0f);
        Main.spriteBatch.End();

        gd.SetRenderTarget(null);
    }

    private void FindSkirtItemDyeShader(On_Player.orig_UpdateItemDye orig, Player self, bool isNotInVanitySlot, bool isSetToHidden, Item armorItem, Item dyeItem)
    {
        orig(self, isNotInVanitySlot, isSetToHidden, armorItem, dyeItem);
        if (armorItem.type == Type)
            SkirtShaderIndex = GameShaders.Armor.GetShaderIdFromItemId(dyeItem.type);
    }

    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 36;
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.value = Item.buyPrice(2, 0, 0, 0);

        Item.accessory = true;
        Item.vanity = true;
    }

    public override void UpdateVanity(Player player) => player.GetValueRef<bool>(WearingPortalSkirtVariableName).Value = true;

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (!hideVisual)
            player.GetValueRef<bool>(WearingPortalSkirtVariableName).Value = true;
    }

    private static void DrawRift(Vector2 drawPosition)
    {
        Texture2D innerRiftTexture = GennedAssets.Textures.FirstPhaseForm.RiftInnerTexture.Value;
        Vector2 textureArea = Vector2.One * 100f / innerRiftTexture.Size();

        var riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
        riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.1f);
        riftShader.TrySetParameter("baseCutoffRadius", 0.1f);
        riftShader.TrySetParameter("swirlOutwardnessExponent", 0.151f);
        riftShader.TrySetParameter("swirlOutwardnessFactor", 3.67f);
        riftShader.TrySetParameter("vanishInterpolant", 0f);
        riftShader.TrySetParameter("edgeColor", new Vector4(1f, 0.08f, 0.08f, 1f));
        riftShader.TrySetParameter("edgeColorBias", 0.15f);
        riftShader.SetTexture(WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
        riftShader.SetTexture(WavyBlotchNoise, 2, SamplerState.AnisotropicWrap);
        riftShader.Apply();

        Main.spriteBatch.Draw(innerRiftTexture, drawPosition, null, new Color(77, 0, 2), 0f, innerRiftTexture.Size() * 0.5f, textureArea, 0, 0f);
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, Main.UIScaleMatrix);
        DrawRift(position);
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);

        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Main.spriteBatch.PrepareForShaders();
        DrawRift(Item.position - Main.screenPosition);
        Main.spriteBatch.ResetToDefault();

        return false;
    }
}
