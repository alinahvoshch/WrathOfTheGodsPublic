using NoxusBoss.Core.World.GameScenes.SolynEventHandlers;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class MarsSummonStatusPacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        packet.Write((byte)(DraedonCombatQuestSystem.MarsBeingSummoned ? 1 : 0));
    }

    public override void Read(BinaryReader reader)
    {
        DraedonCombatQuestSystem.MarsBeingSummoned = reader.ReadByte() != 0;
    }
}
