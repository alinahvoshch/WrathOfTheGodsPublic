using NoxusBoss.Core.World.GameScenes.RiftEclipse;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class RiftEclipseFogTimePacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        packet.Write(RiftEclipseFogEventManager.FogRestartCooldown);
        packet.Write(RiftEclipseFogEventManager.FogTime);
        packet.Write(RiftEclipseFogEventManager.FogDuration);
    }

    public override void Read(BinaryReader reader)
    {
        RiftEclipseFogEventManager.FogRestartCooldown = reader.ReadInt32();
        RiftEclipseFogEventManager.FogTime = reader.ReadInt32();
        RiftEclipseFogEventManager.FogDuration = reader.ReadInt32();
    }
}
