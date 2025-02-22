using NoxusBoss.Content.Tiles.TileEntities;
using NoxusBoss.Core.World.GameScenes.SolynEventHandlers;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class SolynTelescopeTileEntityPacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        int tileEntityID = (int)context[0];
        bool isRepaired = false;
        if (TileEntity.ByID.TryGetValue(tileEntityID, out TileEntity? te) && te is TESolynTelescope telescope)
            isRepaired = telescope.IsRepaired;

        packet.Write(tileEntityID);
        packet.Write((byte)(isRepaired ? 1 : 0));
    }

    public override void Read(BinaryReader reader)
    {
        int tileEntityID = reader.ReadInt32();
        bool isRepaired = reader.ReadByte() != 0;
        if (TileEntity.ByID.TryGetValue(tileEntityID, out TileEntity? te) && te is TESolynTelescope telescope)
            telescope.IsRepaired = isRepaired;

        if (isRepaired)
            StargazingQuestSystem.TelescopeRepaired = true;
    }
}
