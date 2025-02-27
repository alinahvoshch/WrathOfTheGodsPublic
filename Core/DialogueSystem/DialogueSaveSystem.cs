using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Core.DialogueSystem;

public class DialogueSaveSystem : ModSystem
{
    internal static List<string> seenDialogue = [];

    internal static List<string> clickedDialogue = [];

    public override void OnWorldLoad()
    {
        seenDialogue.Clear();
        clickedDialogue.Clear();
    }

    public override void OnWorldUnload()
    {
        seenDialogue.Clear();
        clickedDialogue.Clear();
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["seenDialogue"] = seenDialogue;
        tag["clickedDialogue"] = clickedDialogue;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        seenDialogue = tag.GetList<string>("seenDialogue").ToList();
        clickedDialogue = tag.GetList<string>("clickedDialogue").ToList();
    }
}
