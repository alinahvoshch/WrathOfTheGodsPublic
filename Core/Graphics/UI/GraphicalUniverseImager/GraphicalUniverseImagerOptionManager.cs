using Terraria.ModLoader;
namespace NoxusBoss.Core.Graphics.UI.GraphicalUniverseImager;

public class GraphicalUniverseImagerOptionManager : ModSystem
{
    internal static readonly Dictionary<string, GraphicalUniverseImagerOption> options = [];

    /// <summary>
    /// Registers a new graphical universe imager option to select.
    /// </summary>
    public static GraphicalUniverseImagerOption RegisterNew(GraphicalUniverseImagerOption option) => options[option.LocalizationKey] = option;
}
