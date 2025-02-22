using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class TeleportPlayerPacket : Packet
{
    public override bool ResendFromServer => false;

    public override void Write(ModPacket packet, params object[] context)
    {
        int playerIndex = (int)context[0];
        Player player = Main.player[playerIndex];
        packet.Write(playerIndex);
        packet.WriteVector2(player.position);
        packet.WriteVector2(player.velocity);
    }

    public override void Read(BinaryReader reader)
    {
        int playerIndex = reader.ReadInt32();
        Vector2 position = reader.ReadVector2();
        Vector2 velocity = reader.ReadVector2();

        Player player = Main.player[playerIndex];
        player.position = position;
        player.velocity = velocity;
    }
}
