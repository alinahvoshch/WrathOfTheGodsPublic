using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class SideFinsStep : INamelessDeityRenderStep
{
    public int LayerIndex => 30;

    /// <summary>
    /// The fan animation timer.
    /// </summary>
    public float FanAnimationTimer
    {
        get;
        set;
    }

    /// <summary>
    /// The animation speed of fins that have a fan shape. Defaults to 1.
    /// </summary>
    public float FanAnimationSpeed
    {
        get;
        set;
    }

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    /// <summary>
    /// The texture of the fins.
    /// </summary>
    public NamelessDeitySwappableTexture FinsTexture
    {
        get;
        private set;
    }

    public void Initialize()
    {
        FinsTexture = Composite.RegisterSwappableTexture("Fins", 5, Composite.UsedPreset?.Data.PreferredFinTextures).WithAutomaticSwapRule(() =>
        {
            return Composite.Time % 164 == 0;
        });
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        // Make the fan animation speed approach normalcy.
        if (!Main.gamePaused)
            FanAnimationSpeed = Lerp(FanAnimationSpeed, 1f, 0.04f);

        // Update the fan animation.
        FanAnimationTimer += TwoPi * FanAnimationSpeed / 120f;
        if (FanAnimationTimer >= TwoPi * 10000f)
            FanAnimationTimer = 0f;

        // Draw the fins with weak swaying motions.
        float scaleFactor = 1.3f;
        bool isFan = false;
        Texture2D fins = FinsTexture.UsedTexture;
        Vector2 finPivotLeft = fins.Size() * 0.5f;
        switch (FinsTexture.TextureName)
        {
            case "Fins1":
                finPivotLeft = fins.Size() * new Vector2(0.7878f, 0.3625f);
                scaleFactor = 1f;
                break;
            case "Fins2":
                finPivotLeft = fins.Size() * new Vector2(0.5637f, 0.4963f);
                isFan = true;
                break;
            case "Fins3":
                finPivotLeft = fins.Size() * new Vector2(0.701f, 0.5263f);
                scaleFactor = 1.15f;
                break;
            case "Fins4":
                finPivotLeft = fins.Size() * new Vector2(0.6434f, 0.238f);
                scaleFactor = 0.8f;
                isFan = true;
                break;
            case "Fins5":
                finPivotLeft = fins.Size() * new Vector2(0.6532f, 0.4237f);
                scaleFactor = 1.31f;
                isFan = true;
                break;
        }
        Vector2 finPivotRight = new Vector2(fins.Width - finPivotLeft.X, finPivotLeft.Y);

        // Draw both fins.
        float finRotation = 0f;
        float squishOffset = Clamp(owner.velocity.Length() * 0.033f, 0f, 0.1f);
        if (isFan)
        {
            squishOffset = Sin01(FanAnimationTimer) * 0.3f;
            finRotation += Sin(FanAnimationTimer / FanAnimationSpeed + 0.54f) * 0.07f;
        }

        Vector2 scale = new Vector2(1f, 1f - squishOffset) * scaleFactor * 0.875f;
        Main.EntitySpriteDraw(fins, drawCenter - Vector2.UnitX * 280f, null, Color.White, -finRotation, finPivotLeft, scale, SpriteEffects.None);
        Main.EntitySpriteDraw(fins, drawCenter + Vector2.UnitX * 280f, null, Color.White, finRotation, finPivotRight, scale, SpriteEffects.FlipHorizontally);
    }
}
