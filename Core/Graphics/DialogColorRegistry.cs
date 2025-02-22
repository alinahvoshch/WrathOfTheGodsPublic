using Microsoft.Xna.Framework;
using NoxusBoss.Core.Data;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics;

public class DialogColorRegistry : ModSystem
{
    /// <summary>
    /// The text color used by the cattail animation.
    /// </summary>
    public static Color CattailAnimationTextColor => GetColor("CattailAnimation");

    /// <summary>
    /// The text color used by the purifier warning.
    /// </summary>
    public static Color PurifierWarningTextColor => GetColor("PurifierWarning");

    /// <summary>
    /// The text color used by the Nameless Deity.
    /// </summary>
    public static Color NamelessDeityTextColor => GetColor("NamelessDeity");

    /// <summary>
    /// The text color used by Solyn.
    /// </summary>
    public static Color SolynTextColor => GetColor("Solyn");

    /// <summary>
    /// The file path for the colors.
    /// </summary>
    public const string PaletteFilePath = "Core/Graphics/DialogColors.json";

    /// <summary>
    /// Returns a given color of a given key from the dialogue colors set.
    /// </summary>
    /// <param name="key"></param>
    public static Color GetColor(string key) => new(LocalDataManager.Read<Vector3>(PaletteFilePath)[key]);
}
