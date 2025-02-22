using NoxusBoss.Core.Graphics.UI.Books;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class SolynBookCreditPacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        string bookName = (string)context[0];
        string redeemerName = (string)context[1];
        packet.Write(bookName);
        packet.Write(redeemerName);
    }

    public override void Read(BinaryReader reader)
    {
        string bookName = reader.ReadString();
        string redeemerName = reader.ReadString();
        SolynBookExchangeRegistry.RedeemedBooksCreditRelationship[bookName] = redeemerName;
    }
}
