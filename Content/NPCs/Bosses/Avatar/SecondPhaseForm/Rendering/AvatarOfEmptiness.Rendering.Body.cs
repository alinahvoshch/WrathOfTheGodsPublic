using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria;
using Terraria.ID;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    internal static Vector2 BodyRenderTargetSize => Vector2.One * TargetDownscaleFactor * 4200f;

    internal static InstancedRequestableTarget BodyRenderTarget;

    internal void LoadTargets_Body()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        BodyRenderTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(BodyRenderTarget);
    }

    private void ProcessTargets_Body()
    {
        BodyRenderTarget.Request((int)BodyRenderTargetSize.X, (int)BodyRenderTargetSize.Y, RenderTargetIdentifier, () =>
        {
            if (!TargetsShouldBeProcessed)
                return;

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            Vector2 center = BodyRenderTargetSize * 0.5f;

            float leftArmScale = LeftFrontArmScale;
            float rightArmScale = RightFrontArmScale;
            RenderLeftArm(center - Vector2.UnitX * TargetDownscaleFactor * leftArmScale * 120f);
            RenderRightArm(center + Vector2.UnitX * TargetDownscaleFactor * rightArmScale * 120f);
            RenderHead(center);

            Texture2D lily = GennedAssets.Textures.SecondPhaseForm.SpiderLily.Value;
            Vector2 lilyDrawPosition = center + (SpiderLilyPosition - NPC.Center) * TargetDownscaleFactor;
            Rectangle frame = lily.Frame(1, 3, 0, (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3);
            Main.spriteBatch.Draw(lily, lilyDrawPosition, frame, Color.White, 0f, frame.Size() * 0.5f, LilyScale, 0, 0f);

            Main.spriteBatch.End();
        });
    }
}
