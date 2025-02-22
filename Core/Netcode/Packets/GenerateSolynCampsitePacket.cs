using NoxusBoss.Core.World.WorldGeneration;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class GenerateSolynCampsitePacket : Packet
{
    public override bool ResendFromServer => false;

    public override void Write(ModPacket packet, params object[] context) { }

    public override void Read(BinaryReader reader)
    {
        if (Main.netMode == NetmodeID.Server)
            SolynCampsiteWorldGen.GenerateOnNewThread();
    }
}
