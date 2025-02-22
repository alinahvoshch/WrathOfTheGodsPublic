using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.CeaselessVoid;

[Autoload(Side = ModSide.Client)]
[JITWhenModsEnabled(CalamityCompatibility.ModName)]
[ExtendsFromMod(CalamityCompatibility.ModName)]
public class CeaselessVoidRiftTargetManager : ModSystem
{
    /// <summary>
    /// The render target that holds the drawn contents of the Ceaseless Void's rift.
    /// </summary>
    public static ManagedRenderTarget RiftTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The render target that holds the drawn contents of the Ceaseless Void's rift icon.
    /// </summary>
    public static ManagedRenderTarget RiftIconTarget
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        RiftTarget = new(true, ManagedRenderTarget.CreateScreenSizedTarget);
        RiftIconTarget = new(false, (_, _2) => new(Main.instance.GraphicsDevice, 48, 48));
        RenderTargetManager.RenderTargetUpdateLoopEvent += RenderRiftToTarget;
        RenderTargetManager.RenderTargetUpdateLoopEvent += RenderMiniIcon;

        On_Main.DrawNPCs += DrawTarget;
    }

    private static void DrawTarget(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        orig(self, behindTiles);

        int riftID = ModContent.NPCType<CeaselessVoidRift>();
        if (!NPC.AnyNPCs(riftID))
            return;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

        bool anyoneInteractingWithRift = false;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is CeaselessVoidRift rift && rift.InteractingWithRift && CeaselessVoidRift.CanEnterRift)
            {
                anyoneInteractingWithRift = true;
                break;
            }
        }

        if (anyoneInteractingWithRift)
        {
            Main.pixelShader.CurrentTechnique.Passes["ColorOnly"].Apply();
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (TwoPi * i / 8f).ToRotationVector2() * 2f;
                Main.spriteBatch.Draw(RiftTarget, drawOffset, Color.Yellow);
            }
        }

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        Main.spriteBatch.Draw(RiftTarget, Vector2.Zero, Color.White);

        Main.spriteBatch.ResetToDefault();
    }

    private static void RenderRiftToTarget()
    {
        int riftID = ModContent.NPCType<CeaselessVoidRift>();
        if (!NPC.AnyNPCs(riftID))
            return;

        GraphicsDevice gd = Main.instance.GraphicsDevice;
        gd.SetRenderTarget(RiftTarget);
        gd.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is CeaselessVoidRift rift)
                rift.PreDraw(Main.spriteBatch, Main.screenPosition, Color.White);
        }

        Main.spriteBatch.End();

        gd.SetRenderTarget(null);
    }

    private static void RenderMiniIcon()
    {
        int riftID = ModContent.NPCType<CeaselessVoidRift>();
        if (!NPC.AnyNPCs(riftID))
            return;

        GraphicsDevice gd = Main.instance.GraphicsDevice;
        gd.SetRenderTarget(RiftIconTarget);
        gd.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

        Texture2D innerRiftTexture = GennedAssets.Textures.FirstPhaseForm.RiftInnerTexture.Value;
        Vector2 textureArea = RiftIconTarget.Size();

        ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.CeaselessVoidRiftShader");
        riftShader.TrySetParameter("textureSize", RiftIconTarget.Size());
        riftShader.TrySetParameter("center", Vector2.One * 0.5f);
        riftShader.TrySetParameter("darkeningRadius", 0.2f);
        riftShader.TrySetParameter("pitchBlackRadius", 0.13f);
        riftShader.TrySetParameter("brightColorReplacement", new Vector3(1f, 0f, 0.2f));
        riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.25f);
        riftShader.TrySetParameter("erasureInterpolant", 0f);
        riftShader.TrySetParameter("redEdgeBuffer", 0.15f);
        riftShader.SetTexture(innerRiftTexture, 1, SamplerState.LinearWrap);
        riftShader.SetTexture(PerlinNoise, 2, SamplerState.LinearWrap);
        riftShader.Apply();

        Main.spriteBatch.Draw(innerRiftTexture, RiftIconTarget.Size() * 0.5f, null, Color.White, 0f, innerRiftTexture.Size() * 0.5f, textureArea / innerRiftTexture.Size(), 0, 0f);

        Main.spriteBatch.End();

        gd.SetRenderTarget(null);
    }
}
