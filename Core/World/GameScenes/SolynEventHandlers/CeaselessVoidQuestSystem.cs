using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.CeaselessVoid;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Core.World.GameScenes.SolynEventHandlers;

public class CeaselessVoidQuestSystem : ModSystem
{
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
    /// The position in world space in which the Ceaseless Void's rift is.
    /// </summary>
    public static Vector2 RiftDefeatPosition
    {
        get;
        internal set;
    }

    public override void OnWorldLoad()
    {
        Ongoing = false;
        Completed = false;
    }

    public override void OnWorldUnload()
    {
        Ongoing = false;
        Completed = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (Ongoing)
            tag[nameof(Ongoing)] = true;
        if (Completed)
            tag[nameof(Completed)] = true;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        Ongoing = tag.ContainsKey(nameof(Ongoing));
        Completed = tag.ContainsKey(nameof(Completed));
    }

    public override void PostUpdateNPCs()
    {
        RiftDefeatPosition = Vector2.Zero;

        int riftIndex = NPC.FindFirstNPC(ModContent.NPCType<CeaselessVoidRift>());
        if (riftIndex != -1)
            RiftDefeatPosition = Main.npc[riftIndex].Center;
    }
}
