using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Configuration;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class BackWheelStep : INamelessDeityRenderStep
{
    public int LayerIndex => 20;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    /// <summary>
    /// The texture of the wheel.
    /// </summary>
    public NamelessDeitySwappableTexture WheelTexture
    {
        get;
        private set;
    }

    public void Initialize()
    {
        WheelTexture = Composite.RegisterSwappableTexture("Wheel", 6, Composite.UsedPreset?.Data.PreferredWheelTextures).WithAutomaticSwapRule(() =>
        {
            int swapRate = WoTGConfig.Instance.PhotosensitivityMode ? 19 : 7;
            return Composite.Time % swapRate == 0;
        });
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        // Draw the wheel as a forever spinning object.
        float wheelRotation = Main.GlobalTimeWrappedHourly * 3f;
        Vector2 wheelDrawPosition = drawCenter - Vector2.UnitY * 310f;
        Texture2D wheel = WheelTexture.UsedTexture;
        Main.EntitySpriteDraw(wheel, wheelDrawPosition, null, Color.White, wheelRotation, wheel.Size() * 0.5f, 1f, 0);
    }
}
