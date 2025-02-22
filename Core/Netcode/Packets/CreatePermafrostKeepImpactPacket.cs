using Microsoft.Xna.Framework;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class CreatePermafrostKeepImpactPacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        packet.Write((float)context[0]);
        packet.Write((float)context[1]);
        packet.Write((float)context[2]);
    }

    public override void Read(BinaryReader reader)
    {
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float maxRadius = reader.ReadSingle();
        PermafrostTileProtectionVisualSystem.CreateImpactInner(new Vector2(x, y), maxRadius);
    }
}
