using NoxusBoss.Core.World.WorldGeneration;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class HandwrittenNotePacket : Packet
{
    public override void Write(ModPacket packet, params object[] context) => packet.Write(SolynCampsiteNoteManager.HasReceivedNote);

    public override void Read(BinaryReader reader) => SolynCampsiteNoteManager.HasReceivedNote = reader.ReadBoolean();
}
