using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class EyeOfEternityStep : INamelessDeityRenderStep
{
    public int LayerIndex => 120;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        // Draw the eye flower with a pulsation motion.
        float eyePulse = Lerp(0.9f, 1.1f, Cos01(Main.GlobalTimeWrappedHourly * 1.95f));
        Vector2 eyeDrawPosition = drawCenter - Vector2.UnitY * 226f;
        Texture2D eye = GennedAssets.Textures.NamelessDeity.EyeOfEternity.Value;
        Main.EntitySpriteDraw(eye, eyeDrawPosition, null, Color.White, 0f, eye.Size() * 0.5f, eyePulse, 0);
    }
}
