using NoxusBoss.Core.World.WorldGeneration;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class PermafrostKeepKeyReceivePacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        packet.Write(PermafrostKeepWorldGen.PlayerGivenKey);
    }

    public override void Read(BinaryReader reader)
    {
        PermafrostKeepWorldGen.PlayerGivenKey = reader.ReadBoolean();
    }
}
