using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Core.World.GameScenes.SolynEventHandlers;

public class PermafrostKeepQuestSystem : ModSystem
{
    /// <summary>
    /// Whether Solyn has revealed Permafrost's keep on the map.
    /// </summary>
    public static bool KeepVisibleOnMap
    {
        get;
        set;
    }

    /// <summary>
    /// Whether the quest is ongoing.
    /// </summary>
    public static bool Ongoing
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
        KeepVisibleOnMap = false;
        Ongoing = false;
        Completed = false;
    }

    public override void OnWorldUnload()
    {
        KeepVisibleOnMap = false;
        Ongoing = false;
        Completed = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (KeepVisibleOnMap)
            tag[nameof(KeepVisibleOnMap)] = true;
        if (Ongoing)
            tag[nameof(Ongoing)] = true;
        if (Completed)
            tag[nameof(Completed)] = true;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        KeepVisibleOnMap = tag.ContainsKey(nameof(KeepVisibleOnMap));
        Ongoing = tag.ContainsKey(nameof(Ongoing));
        Completed = tag.ContainsKey(nameof(Completed));
    }
}
