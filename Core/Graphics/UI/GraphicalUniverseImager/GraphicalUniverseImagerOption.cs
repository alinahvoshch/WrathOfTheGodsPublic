using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;

namespace NoxusBoss.Core.Graphics.UI.GraphicalUniverseImager;

public delegate void TileColorFunction(ref Color tileColor, ref Color backgroundColor);

public record GraphicalUniverseImagerOption(string LocalizationKey, bool DrawOnlyToForeground, LazyAsset<Texture2D> IconTexture, Action<GraphicalUniverseImagerSettings> PortraitRenderFunction,
    Action<float, float, GraphicalUniverseImagerSettings> BackgroundRenderFunction, TileColorFunction? TileColorFunction = null);
