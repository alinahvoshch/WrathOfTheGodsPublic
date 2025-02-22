using NoxusBoss.Core.Graphics.UI.Books;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class SolynBookStoragePacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        string bookName = (string)context[0];
        packet.Write(bookName);
    }

    public override void Read(BinaryReader reader)
    {
        SolynBookExchangeRegistry.RedeemedBooks.Add(reader.ReadString());
    }
}
