using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class XerocEyeStep : INamelessDeityRenderStep
{
    public int LayerIndex => 0;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        if (owner is NPC && AvatarOfEmptiness.Myself is not null)
            return;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

        // Draw the eye statically and with a decent amount of glow.
        Vector2 eyeDrawPosition = drawCenter + new Vector2(-12f, -32f);
        Texture2D eye = GennedAssets.Textures.NamelessDeity.NamelessDeityEyeFull.Value;
        Main.spriteBatch.Draw(eye, eyeDrawPosition, null, new Color(1f, 1f, 1f), 0f, eye.Size() * 0.5f, 0.8f, 0, 0f);

        Composite.ResetSpriteBatch(true);
    }
}
