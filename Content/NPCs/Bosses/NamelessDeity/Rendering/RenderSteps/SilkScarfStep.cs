using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Configuration;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class SilkScarfStep : INamelessDeityRenderStep
{
    public int LayerIndex => 70;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    /// <summary>
    /// The texture of the scarf.
    /// </summary>
    public NamelessDeitySwappableTexture ScarfTexture
    {
        get;
        private set;
    }

    public void Initialize()
    {
        ScarfTexture = Composite.RegisterSwappableTexture("Scarf", 4).WithAutomaticSwapRule(() =>
        {
            int swapRate = WoTGConfig.Instance.PhotosensitivityMode ? 8 : 4;
            return Composite.Time % swapRate == 0;
        });
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        Vector2 scarfDrawPosition = drawCenter - Vector2.UnitY * 272f;
        Texture2D scarf = ScarfTexture.UsedTexture;
        Main.EntitySpriteDraw(scarf, scarfDrawPosition, null, Color.White, 0f, scarf.Size() * 0.5f, 1f, 0);
    }
}
