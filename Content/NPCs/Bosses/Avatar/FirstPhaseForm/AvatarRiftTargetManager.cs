using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;

[Autoload(Side = ModSide.Client)]
public class AvatarRiftTargetManager : ModSystem
{
    /// <summary>
    /// The render target that holds the Avatar's rift.
    /// </summary>
    public static ManagedRenderTarget AvatarRiftTarget
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        On_Main.DrawNPCs += DrawRiftTarget;
        AvatarRiftTarget = new(true, ManagedRenderTarget.CreateScreenSizedTarget);

        RenderTargetManager.RenderTargetUpdateLoopEvent += DrawToTarget;
    }

    private void DrawToTarget()
    {
        if (AvatarRift.Myself is null)
            return;

        GraphicsDevice gd = Main.instance.GraphicsDevice;
        gd.SetRenderTarget(AvatarRiftTarget);
        gd.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        AvatarRift.Myself.As<AvatarRift>().DrawSelf(Main.screenPosition);
        Main.spriteBatch.End();

        gd.SetRenderTarget(null);
    }

    private void DrawRiftTarget(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        bool drawBeforeOtherNPCs = AvatarRift.Myself is not null && AvatarRift.Myself.As<AvatarRift>().CurrentAttack == AvatarRift.RiftAttackType.KillOldDuke;

        if (!behindTiles && AvatarRift.Myself is not null && drawBeforeOtherNPCs)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Main.spriteBatch.Draw(AvatarRiftTarget, Vector2.Zero, Color.White);
            Main.spriteBatch.ResetToDefault();
        }

        orig(self, behindTiles);

        if (!behindTiles && AvatarRift.Myself is not null && !drawBeforeOtherNPCs)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Main.spriteBatch.Draw(AvatarRiftTarget, Vector2.Zero, Color.White);
            Main.spriteBatch.ResetToDefault();
        }
    }
}
