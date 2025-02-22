using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class AtlasMothStep : INamelessDeityRenderStep
{
    public int LayerIndex => 110;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        // Draw the moth with flapping wings via a scaling illusion.
        Vector2 mothDrawPosition = drawCenter - Vector2.UnitY * 226f;
        PiecewiseCurve wingFlapCurve = new PiecewiseCurve().
            Add(EasingCurves.Quartic, EasingType.In, 0.03f, 0.3f, 1f).
            Add(EasingCurves.Sextic, EasingType.Out, 1.04f, 0.7f).
            Add(EasingCurves.Quadratic, EasingType.Out, 1f, 1f);
        float flapScale = wingFlapCurve.Evaluate(Main.GlobalTimeWrappedHourly * 0.9f % 1f);

        Texture2D body = GennedAssets.Textures.NamelessDeity.AtlasMothBody.Value;
        Texture2D wings = GennedAssets.Textures.NamelessDeity.AtlasMothWing.Value;
        if (Composite.UsedPreset?.Data.MothReplacementTexture is not null)
        {
            body = ModContent.Request<Texture2D>(Composite.UsedPreset?.Data.MothReplacementTexture).Value;
            wings = InvisiblePixel;
        }

        // Draw the wings.
        Vector2 wingSpacing = Vector2.UnitX * 6f;
        Vector2 leftWingDrawPosition = mothDrawPosition - wingSpacing;
        Vector2 rightWingDrawPosition = mothDrawPosition + wingSpacing;
        Vector2 wingScale = 1f * new Vector2(flapScale, 1f - Abs(flapScale - 1f) * 0.12f);
        Main.EntitySpriteDraw(wings, leftWingDrawPosition, null, Color.White, 0f, wings.Size() * new Vector2(0f, 0.5f), wingScale, SpriteEffects.None);
        Main.EntitySpriteDraw(wings, rightWingDrawPosition, null, Color.White, 0f, wings.Size() * new Vector2(1f, 0.5f), wingScale, SpriteEffects.FlipHorizontally);

        // Draw the body.
        Main.EntitySpriteDraw(body, mothDrawPosition, null, Color.White, 0f, body.Size() * 0.5f, 1f, 0);
    }
}
