using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class GlowRingStep : INamelessDeityRenderStep
{
    public int LayerIndex => 130;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

        float ringOpacity = 0.6f;
        Texture2D ring = GennedAssets.Textures.NamelessDeity.GlowRing.Value;
        Main.EntitySpriteDraw(ring, drawCenter, null, new Color(1f, 1f, 1f, 0f) * ringOpacity, 0f, ring.Size() * 0.5f, 0.67f, 0);

        Composite.ResetSpriteBatch(true);
    }
}
