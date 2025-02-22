using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class BodyStep : INamelessDeityRenderStep
{
    public int LayerIndex => 100;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        // Draw the body statically.
        Texture2D body = GennedAssets.Textures.NamelessDeity.DivineBody.Value;
        if (Composite.UsedPreset?.Data.BodyReplacementTexture is not null)
            body = ModContent.Request<Texture2D>(Composite.UsedPreset?.Data.BodyReplacementTexture).Value;

        Main.EntitySpriteDraw(body, drawCenter, null, Color.White, 0f, body.Size() * 0.5f, 1f, 0);
    }
}
