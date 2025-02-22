using Terraria;
using Terraria.ModLoader;
namespace NoxusBoss.Core.Graphics.UI.GraphicalUniverseImager;

public class GraphicalUniverseImagerMusicManager : ModSceneEffect
{
    public override SceneEffectPriority Priority => (SceneEffectPriority)15;

    public override int Music
    {
        get
        {
            if (GraphicalUniverseImagerSky.RenderSettings is not null)
            {
                string? musicPath = GraphicalUniverseImagerSky.RenderSettings.Music.MusicPath;
                if (musicPath is not null)
                    return MusicLoader.GetMusicSlot(musicPath);
            }

            // Default to regular music.
            return -1;
        }
    }

    public override bool IsSceneEffectActive(Player player) => ModContent.GetInstance<UniverseImagerUI>().ActiveTileEntity is not null;

    internal static readonly Dictionary<string, GraphicalUniverseImagerMusicOption> musicOptions = [];

    /// <summary>
    /// Registers a new graphical universe imager music option to select.
    /// </summary>
    public static GraphicalUniverseImagerMusicOption RegisterNew(GraphicalUniverseImagerMusicOption option) => musicOptions[option.LocalizationKey] = option;

    /// <summary>
    /// The default music option. Does not override existing music.
    /// </summary>
    public static readonly GraphicalUniverseImagerMusicOption Default =
        RegisterNew(new GraphicalUniverseImagerMusicOption("Mods.NoxusBoss.UI.GraphicalUniverseImager.NoMusic", null));

    /// <summary>
    /// The Avatar music option. Plays special HotoG auric soul type music in the style of the Avatar.
    /// </summary>
    public static readonly GraphicalUniverseImagerMusicOption Avatar =
        RegisterNew(new GraphicalUniverseImagerMusicOption("Mods.NoxusBoss.UI.GraphicalUniverseImager.AvatarMusic", "NoxusBoss/Assets/Sounds/Music/quiescence"));
}
