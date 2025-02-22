using NoxusBoss.Content.Tiles.TileEntities;
using NoxusBoss.Core.Graphics.UI.GraphicalUniverseImager;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class GraphicalUniverseImagerSettingsPacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        int id = (int)context[0];
        packet.Write(id);

        GraphicalUniverseImagerSettings settings = new GraphicalUniverseImagerSettings();
        if (TileEntity.ByID.TryGetValue(id, out TileEntity? te) && te is TEGraphicalUniverseImager imager)
            settings = imager.Settings;

        settings.Send(packet);
    }

    public override void Read(BinaryReader reader)
    {
        int id = reader.ReadInt32();
        GraphicalUniverseImagerSettings settings = new GraphicalUniverseImagerSettings();
        settings.Receive(reader);

        if (TileEntity.ByID.TryGetValue(id, out TileEntity? te) && te is TEGraphicalUniverseImager imager)
            imager.Settings = settings;
    }
}
