using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class HeadHandsStep : INamelessDeityRenderStep
{
    public int LayerIndex => 90;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        // Draw the head hands with a cyclic contracting motion.
        float handRotationOffset = Lerp(-0.05f, 0.23f, Cos01(Main.GlobalTimeWrappedHourly * 0.4f));
        Vector2 handDrawCenter = drawCenter - Vector2.UnitY * 266f;
        Vector2 handSpacing = Vector2.UnitX * 70f;
        Vector2 leftHandDrawPosition = handDrawCenter - handSpacing;
        Vector2 rightHandDrawPosition = handDrawCenter + handSpacing;
        Texture2D hand = GennedAssets.Textures.NamelessDeity.HeadHands.Value;

        // Draw each hand separately.
        Main.EntitySpriteDraw(hand, leftHandDrawPosition, null, Color.White, -handRotationOffset, hand.Size() * new Vector2(1f, 0.5f), 1f, SpriteEffects.None);
        Main.EntitySpriteDraw(hand, rightHandDrawPosition, null, Color.White, handRotationOffset, hand.Size() * new Vector2(0f, 0.5f), 1f, SpriteEffects.FlipHorizontally);
    }
}
