using Microsoft.Xna.Framework;
using NoxusBoss.Content.Tiles.TileEntities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class SolynTentTileEntityPacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        int tileEntityID = (int)context[0];
        Vector2 ropeStart = Vector2.Zero;
        Vector2 ropeEnd = Vector2.Zero;
        if (TileEntity.ByID.TryGetValue(tileEntityID, out TileEntity? te) && te is TESolynTent tent)
        {
            ropeStart = tent.RopeStart;
            ropeEnd = tent.RopeEnd;
        }

        packet.Write(tileEntityID);
        packet.WriteVector2(ropeStart);
        packet.WriteVector2(ropeEnd);
    }

    public override void Read(BinaryReader reader)
    {
        int tileEntityID = reader.ReadInt32();
        Vector2 ropeStart = reader.ReadVector2();
        Vector2 ropeEnd = reader.ReadVector2();

        if (TileEntity.ByID.TryGetValue(tileEntityID, out TileEntity? te) && te is TESolynTent tent)
        {
            tent.RopeStart = ropeStart;
            tent.RopeEnd = ropeEnd;
        }
    }
}
