using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Core.World.GameScenes.SolynEventHandlers;

public class StargazingQuestSystem : ModSystem
{
    /// <summary>
    /// Whether Solyn's telescope has been repaired.
    /// </summary>
    public static bool TelescopeRepaired
    {
        get;
        set;
    }

    /// <summary>
    /// Whether the quest has been completed.
    /// </summary>
    public static bool Completed
    {
        get;
        set;
    }

    public override void OnWorldLoad()
    {
        TelescopeRepaired = false;
        Completed = false;
    }

    public override void OnWorldUnload()
    {
        TelescopeRepaired = false;
        Completed = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (TelescopeRepaired)
            tag[nameof(TelescopeRepaired)] = true;
        if (Completed)
            tag[nameof(Completed)] = true;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        TelescopeRepaired = tag.ContainsKey(nameof(TelescopeRepaired));
        Completed = tag.ContainsKey(nameof(Completed));
    }
}
