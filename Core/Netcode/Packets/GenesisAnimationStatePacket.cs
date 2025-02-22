using NoxusBoss.Core.Graphics.GenesisEffects;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class GenesisAnimationStatePacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        packet.Write(GenesisVisualsSystem.Time);
        packet.Write((int)GenesisVisualsSystem.ActivationPhase);
        packet.WriteVector2(GenesisVisualsSystem.Position);
    }

    public override void Read(BinaryReader reader)
    {
        GenesisVisualsSystem.Time = reader.ReadInt32();
        GenesisVisualsSystem.ActivationPhase = (GenesisActivationPhase)reader.ReadInt32();
        GenesisVisualsSystem.Position = reader.ReadVector2();
    }
}
