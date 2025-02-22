using NoxusBoss.Content.Tiles.GenesisComponents.Seedling;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class AddGenesisPointPacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        int pointX = (int)context[0];
        int pointY = (int)context[1];
        packet.Write(pointX);
        packet.Write(pointY);
    }

    public override void Read(BinaryReader reader)
    {
        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        GrowingGenesisRenderSystem.AddGenesisPointInternal(new(x, y));
    }
}
