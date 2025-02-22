using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.UI;

namespace NoxusBoss.Core.Graphics.UI;

public class UICustomImage(Asset<Texture2D> texture, Color color) : UIElement
{
    /// <summary>
    /// The image texture.
    /// </summary>
    private readonly Asset<Texture2D> texture = texture;

    /// <summary>
    /// The color of the image.
    /// </summary>
    public Color Color = color;

    /// <summary>
    /// The color of the image when being hovered over by the mouse. By default, this is null and does not affect anything.
    /// </summary>
    public Color? HoverColor;

    /// <summary>
    /// An event that fires whenever the image is left clicked by the mouse cursor.
    /// </summary>
    public new event Action OnLeftClick;

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        // Calculate position and size based draw information.
        CalculatedStyle dimensions = GetDimensions();
        Point point = new Point((int)dimensions.X, (int)dimensions.Y);
        Rectangle drawArea = new Rectangle(point.X, point.Y, texture.Width(), texture.Height());

        // Handle hover and click interactions.
        Color color = Color;
        if (new Rectangle(Main.mouseX, Main.mouseY, 2, 2).Intersects(drawArea))
        {
            Main.LocalPlayer.mouseInterface = true;
            color = HoverColor ?? color;

            if (Main.mouseLeft && Main.mouseLeftRelease)
                OnLeftClick?.Invoke();
        }

        spriteBatch.Draw(texture.Value, new Vector2(point.X, point.Y), null, color, 0f, Vector2.Zero, new Vector2(Width.Pixels, Height.Pixels) / texture.Value.Size(), 0, 0f);
    }
}
