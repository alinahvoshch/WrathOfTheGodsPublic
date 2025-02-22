using NoxusBoss.Core.Graphics.UI.SolynDialogue;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class SeenDialoguePacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        string key = (string)context[0];
        packet.Write(key);
    }

    public override void Read(BinaryReader reader)
    {
        string key = reader.ReadString();
        if (!ConversationDataSaveSystem.seenDialogue.Contains(key))
            ConversationDataSaveSystem.seenDialogue.Add(key);
    }
}
