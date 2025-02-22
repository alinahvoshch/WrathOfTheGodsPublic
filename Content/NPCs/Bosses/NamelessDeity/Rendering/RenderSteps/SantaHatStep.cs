using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class SantaHatStep : INamelessDeityRenderStep
{
    public int LayerIndex => 150;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        DateTime date = DateTime.Now;
        if (date.Month != 12 || date.Day != 25)
            return;

        float hatScale = 0.5f;
        Vector2 hatDrawPosition = drawCenter - Vector2.UnitY * 326f;
        Texture2D santaHat = GennedAssets.Textures.NamelessDeity.SantaHat;

        Main.EntitySpriteDraw(santaHat, hatDrawPosition, null, Color.White, 0f, santaHat.Size() * 0.5f, hatScale, 0);
    }
}
