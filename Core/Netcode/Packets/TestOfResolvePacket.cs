using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class TestOfResolvePacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        packet.Write((byte)TestOfResolveSystem.IsActive.ToInt());
    }

    public override void Read(BinaryReader reader)
    {
        TestOfResolveSystem.IsActive = reader.ReadByte() != 0;
    }
}
