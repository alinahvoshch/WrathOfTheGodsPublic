using NoxusBoss.Content.Tiles.GenesisComponents;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class AddGenesisPlantPointPacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        string systemName = (string)context[0];
        int pointX = (int)context[1];
        int pointY = (int)context[2];
        packet.Write(systemName);
        packet.Write(pointX);
        packet.Write(pointY);
    }

    public override void Read(BinaryReader reader)
    {
        string systemName = reader.ReadString();
        int x = reader.ReadInt32();
        int y = reader.ReadInt32();

        if (GenesisPlantTileRenderingSystem.renderSystems.TryGetValue(systemName, out GenesisPlantTileRenderingSystem? system))
            system.AddPointInternal(new(x, y));
    }
}
