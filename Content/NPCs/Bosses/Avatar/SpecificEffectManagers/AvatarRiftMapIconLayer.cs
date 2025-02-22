using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;

public class AvatarRiftMapIconLayer : ModMapLayer
{
    /// <summary>
    /// The render target that holds the drawn contents of the Avatar's rift icon.
    /// </summary>
    public static ManagedRenderTarget RiftIconTarget
    {
        get;
        private set;
    }

    public override void Load()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        RiftIconTarget = new(false, (_, _2) => new(Main.instance.GraphicsDevice, 48, 48));
        RenderTargetManager.RenderTargetUpdateLoopEvent += RenderMiniIcon;
    }

    private static void RenderMiniIcon()
    {
        if (AvatarRift.Myself is null)
            return;

        GraphicsDevice gd = Main.instance.GraphicsDevice;
        gd.SetRenderTarget(RiftIconTarget);
        gd.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

        Texture2D innerRiftTexture = GennedAssets.Textures.FirstPhaseForm.RiftInnerTexture.Value;
        Vector2 textureArea = RiftIconTarget.Size();

        ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.CeaselessVoidRiftShader");
        riftShader.TrySetParameter("textureSize", RiftIconTarget.Size() * 2f);
        riftShader.TrySetParameter("center", Vector2.One * 0.5f);
        riftShader.TrySetParameter("darkeningRadius", 0.2f);
        riftShader.TrySetParameter("pitchBlackRadius", 0.13f);
        riftShader.TrySetParameter("brightColorReplacement", new Vector3(1f, 0f, 0.2f));
        riftShader.TrySetParameter("time", AvatarRift.Myself.As<AvatarRift>().RiftRotationTimer * -10f);
        riftShader.TrySetParameter("erasureInterpolant", 1f - AvatarRift.Myself.scale);
        riftShader.TrySetParameter("redEdgeBuffer", 0.15f);
        riftShader.SetTexture(innerRiftTexture, 1, SamplerState.LinearWrap);
        riftShader.SetTexture(PerlinNoise, 2, SamplerState.LinearWrap);
        riftShader.Apply();

        Main.spriteBatch.Draw(innerRiftTexture, RiftIconTarget.Size() * 0.5f, null, Color.White, 0f, innerRiftTexture.Size() * 0.5f, textureArea / innerRiftTexture.Size(), 0, 0f);

        Main.spriteBatch.End();

        gd.SetRenderTarget(null);
    }

    public override void Draw(ref MapOverlayDrawContext context, ref string text)
    {
        if (AvatarRift.Myself is null)
            return;

        Texture2D icon = RiftIconTarget;
        if (context.Draw(icon, AvatarRift.Myself.Center.ToTileCoordinates().ToVector2(), Alignment.Center).IsMouseOver)
            text = Language.GetTextValue("Mods.NoxusBoss.NPCs.AvatarRift.DisplayName");
    }
}
