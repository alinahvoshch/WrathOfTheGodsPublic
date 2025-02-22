using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace NoxusBoss.Core.Graphics.ModIcon;

public class UIArbitraryDrawImage(UIArbitraryDrawImage.ImageDrawDelegate drawFunction, LazyAsset<Texture2D> texture) : UIImage(texture)
{
    protected LazyAsset<Texture2D> textureAsset = texture;

    public readonly ImageDrawDelegate DrawFunction = drawFunction;

    public delegate void ImageDrawDelegate(Texture2D texture, Vector2 drawPosition, Rectangle? rectangle, Color color, float rotation, Vector2 origin, Vector2 scale);

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D texture = textureAsset.Value;
        CalculatedStyle dimensions = GetDimensions();
        if (ScaleToFit)
        {
            spriteBatch.Draw(texture, dimensions.ToRectangle(), Color);
            Vector2 scale = new Vector2(dimensions.Width, dimensions.Height) / texture.Size();
            DrawFunction(texture, dimensions.Position(), null, Color, 0f, Vector2.Zero, scale);
            return;
        }
        Vector2 size = texture.Size();
        Vector2 drawPosition = dimensions.Position() + size * (1f - ImageScale) * 0.5f + size * NormalizedOrigin;
        if (RemoveFloatingPointsFromDrawPosition)
            drawPosition = drawPosition.Floor();

        DrawFunction(texture, drawPosition, null, Color, Rotation, size * NormalizedOrigin, Vector2.One * ImageScale);
    }
}
