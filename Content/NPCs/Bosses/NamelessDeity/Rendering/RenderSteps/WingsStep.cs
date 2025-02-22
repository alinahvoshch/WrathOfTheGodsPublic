using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class WingsStep : INamelessDeityRenderStep
{
    public int LayerIndex => 40;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    /// <summary>
    /// The handler for the wing set.
    /// </summary>
    public NamelessDeityWingSet Wings
    {
        get;
        set;
    }

    /// <summary>
    /// The texture of the wings.
    /// </summary>
    public NamelessDeitySwappableTexture WingsTexture
    {
        get;
        private set;
    }

    public void Initialize()
    {
        Wings = new();
        WingsTexture = Composite.RegisterSwappableTexture("Wings", 5, Composite.UsedPreset?.Data.PreferredWingTextures).WithAutomaticSwapRule(() =>
        {
            return Composite.Time % 180 == 0;
        });
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        // Calculate wing textures.
        Texture2D wingsTexture = WingsTexture.UsedTexture;
        Vector2 leftWingOrigin = wingsTexture.Size() * new Vector2(1f, 0.84f);
        Vector2 rightWingOrigin = leftWingOrigin;
        rightWingOrigin.X = wingsTexture.Width - rightWingOrigin.X;
        Color wingsDrawColor = Color.White;

        // Calculate rotation values.
        float wingRotation = Wings.Rotation;

        // Draw the wings.
        Vector2 drawPosition = drawCenter + Vector2.UnitY * 70f;
        Vector2 scale = new Vector2(1f, 1f - Wings.Squish) * new Vector2(1.35f, 1.1f);
        Main.spriteBatch.Draw(wingsTexture, drawPosition - Vector2.UnitX * 86f, null, wingsDrawColor, wingRotation, leftWingOrigin, scale, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(wingsTexture, drawPosition + Vector2.UnitX * 86f, null, wingsDrawColor, -wingRotation, rightWingOrigin, scale, SpriteEffects.FlipHorizontally, 0f);
    }
}
