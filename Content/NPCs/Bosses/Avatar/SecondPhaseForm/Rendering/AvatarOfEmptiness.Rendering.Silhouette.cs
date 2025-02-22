using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria;
using Terraria.ID;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    internal static InstancedRequestableTarget SilhouetteRenderTarget;

    internal void LoadTargets_Silhouette()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        SilhouetteRenderTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(SilhouetteRenderTarget);
    }

    private void ProcessTargets_Silhouette()
    {
        Vector2 targetSize = new Vector2(750f);
        SilhouetteRenderTarget.Request((int)targetSize.X, (int)targetSize.Y, RenderTargetIdentifier, () =>
        {
            if (!TargetsShouldBeProcessed || !DrawnAsSilhouette)
                return;

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

            float oldScale = NPC!.scale;
            NPC.scale = 0.2f;
            PreDraw(Main.spriteBatch, NPC.Center + targetSize * -0.5f, Color.White);
            NPC.scale = oldScale;

            Main.spriteBatch.End();
        });
    }
}
