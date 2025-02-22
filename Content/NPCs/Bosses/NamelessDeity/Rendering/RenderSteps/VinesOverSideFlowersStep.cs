using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class VinesOverSideFlowersStep : INamelessDeityRenderStep
{
    public int LayerIndex => 60;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        // Draw the side plants almost statically, with a tiny bit of vertical offset over time.
        float verticalOffset = Cos(Main.GlobalTimeWrappedHourly * 0.1f) * 4f + 60f;
        Vector2 plantDrawPosition = drawCenter - Vector2.UnitY * verticalOffset;
        Texture2D plant = GennedAssets.Textures.NamelessDeity.FlowerTop.Value;

        Main.EntitySpriteDraw(plant, plantDrawPosition, null, Color.White, 0f, plant.Size() * 0.5f, 1f, 0);
    }
}
