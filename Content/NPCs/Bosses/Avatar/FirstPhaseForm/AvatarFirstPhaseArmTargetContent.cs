using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;

public class AvatarFirstPhaseArmTargetContent : ARenderTargetContentByRequest
{
    public AvatarRift Host
    {
        get;
        internal set;
    }

    public static readonly Vector2 Size = new Vector2(3200, 3200);

    protected override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
    {
        // Initialize the underlying render target if necessary.
        PrepareARenderTarget_AndListenToEvents(ref _target, device, (int)Size.X, (int)Size.Y, RenderTargetUsage.PreserveContents);

        device.SetRenderTarget(_target);
        device.Clear(Color.Transparent);

        // Draw the host's contents to the render target.
        Host.DrawArms(Host.NPC.Center - Size * 0.5f);

        device.SetRenderTarget(null);

        // Mark preparations as completed.
        _wasPrepared = true;
    }
}
