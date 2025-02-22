using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Core.World.GameScenes.SolynEventHandlers;

public class DraedonCombatQuestSystem : ModSystem
{
    /// <summary>
    /// Whether Mars is in the process of being summoned. This is used to determine whether Draedon should be transformed into his secret alt form so that he can summon Mars instead of the standard Exo Mechs.
    /// </summary>
    public static bool MarsBeingSummoned
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

    /// <summary>
    /// Whether Solyn and the player have spoken to Draedon already before, and should therefore not have to listen to his spiel again.
    /// </summary>
    public static bool HasSpokenToDraedonBefore
    {
        get;
        set;
    }

    public override void OnWorldLoad()
    {
        HasSpokenToDraedonBefore = false;
        Ongoing = false;
        Completed = false;
    }

    public override void OnWorldUnload()
    {
        HasSpokenToDraedonBefore = false;
        Ongoing = false;
        Completed = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (Ongoing)
            tag[nameof(Ongoing)] = true;
        if (Completed)
            tag[nameof(Completed)] = true;
        if (HasSpokenToDraedonBefore)
            tag[nameof(HasSpokenToDraedonBefore)] = true;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        Ongoing = tag.ContainsKey(nameof(Ongoing));
        Completed = tag.ContainsKey(nameof(Completed));
        HasSpokenToDraedonBefore = tag.ContainsKey(nameof(HasSpokenToDraedonBefore));
    }
}
