using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Core.DialogueSystem;

public class DialogueSaveSystem : ModSystem
{
    internal static List<string> seenDialogue = [];

    public override void OnWorldLoad()
    {
        seenDialogue.Clear();
    }

    public override void OnWorldUnload()
    {
        seenDialogue.Clear();
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["seenDialogue"] = seenDialogue;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        seenDialogue = tag.GetList<string>("seenDialogue").ToList();
    }
}
