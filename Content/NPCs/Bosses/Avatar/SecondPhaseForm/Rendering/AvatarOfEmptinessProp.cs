using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.Rendering;

public class AvatarOfEmptinessProp
{
    /// <summary>
    /// The starting point of this prop.
    /// </summary>
    public Vector2 Start;

    /// <summary>
    /// The ending point of this prop.
    /// </summary>
    public Vector2 End;

    /// <summary>
    /// The texture of this prop.
    /// </summary>
    public readonly LazyAsset<Texture2D> PropTexture;

    /// <summary>
    /// The origin of this prop relative to the texture.
    /// </summary>
    public readonly Vector2 OriginFactor;

    /// <summary>
    /// The undirectioned draw offset of this prop.
    /// </summary>
    public readonly Vector2 UndirectionedDrawOffset;

    /// <summary>
    /// The general rotation offset of this prop.
    /// </summary>
    /// 
    /// <remarks>
    /// As examples, if the prop faces upward this should be pi/2. If it faces downward it should be -pi/2. If it faces to the right it should be 0.
    /// </remarks>
    public readonly float GeneralRotationOffset;

    public AvatarOfEmptinessProp(LazyAsset<Texture2D> propTexture, Vector2 originFactor, float generalRotationOffset, Vector2 undirectionedDrawOffset = default)
    {
        PropTexture = propTexture;
        OriginFactor = originFactor;
        GeneralRotationOffset = generalRotationOffset;
        UndirectionedDrawOffset = undirectionedDrawOffset;
    }

    /// <summary>
    /// Moves this prop's end point to a given destination.
    /// </summary>
    /// <param name="destination">The destination to move towards.</param>
    public void MoveTowards(Vector2 destination)
    {
        End.X = Lerp(End.X, destination.X, 0.06f);
        End.Y = Lerp(End.Y, destination.Y, 0.093f);
        End = End.MoveTowards(destination, 0.3f);
    }

    /// <summary>
    /// Renders this prop.
    /// </summary>
    /// <param name="stringColor">The string color.</param>
    public void Render(Color stringColor)
    {
        Texture2D propTexture = PropTexture.Value;
        float propRotation = Start.AngleTo(End) + GeneralRotationOffset;
        Vector2 origin = propTexture.Size() * OriginFactor;
        Vector2 propDrawPosition = End + UndirectionedDrawOffset.RotatedBy(propRotation);

        float opacity = stringColor.A / 255f;
        Main.spriteBatch.DrawLineBetter(Start + Main.screenPosition, End + Main.screenPosition, stringColor, 3f);
        Main.spriteBatch.Draw(propTexture, propDrawPosition, null, Color.White * opacity, propRotation, origin, 0.4f, 0, 0f);
    }
}
