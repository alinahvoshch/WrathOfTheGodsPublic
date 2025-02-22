using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class AntlersStep : INamelessDeityRenderStep
{
    public int LayerIndex => 80;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    /// <summary>
    /// The texture of the antlers.
    /// </summary>
    public NamelessDeitySwappableTexture AntlersTexture
    {
        get;
        private set;
    }

    public void Initialize()
    {
        AntlersTexture = Composite.RegisterSwappableTexture("Antlers", 7, Composite.UsedPreset?.Data.PreferredAntlerTextures).WithAutomaticSwapRule(() =>
        {
            return Composite.Time % 90 == 0;
        });
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        // Draw the antlers statically.
        float antlerScale = 1f;
        Vector2 antlerDrawPosition = drawCenter - Vector2.UnitY * 254f;
        Texture2D antlers = AntlersTexture.UsedTexture;

        Main.EntitySpriteDraw(antlers, antlerDrawPosition, null, Color.White, 0f, antlers.Size() * 0.5f, antlerScale, 0);
    }
}
