using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Core.Graphics.UI.SolynDialogue;

public class ConversationDataSaveSystem : ModSystem
{
    internal static List<string> seenConversations = [];

    internal static List<string> seenDialogue = [];

    public override void OnWorldLoad()
    {
        seenDialogue.Clear();
        seenConversations.Clear();
    }

    public override void OnWorldUnload()
    {
        seenDialogue.Clear();
        seenConversations.Clear();
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["seenDialogue"] = seenDialogue;
        tag["seenConversations"] = seenConversations;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        seenDialogue = tag.GetList<string>("seenDialogue").ToList();
        seenConversations = tag.GetList<string>("seenConversations").ToList();
    }
}
