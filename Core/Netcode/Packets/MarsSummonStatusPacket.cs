using NoxusBoss.Core.SolynEvents;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class MarsSummonStatusPacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        packet.Write((byte)(MarsCombatEvent.MarsBeingSummoned ? 1 : 0));
    }

    public override void Read(BinaryReader reader)
    {
        MarsCombatEvent.MarsBeingSummoned = reader.ReadByte() != 0;
    }
}
