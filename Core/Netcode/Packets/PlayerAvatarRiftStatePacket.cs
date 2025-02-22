using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Netcode.Packets;

public class PlayerAvatarRiftStatePacket : Packet
{
    public override void Write(ModPacket packet, params object[] context)
    {
        int playerIndex = (int)context[0];
        Player player = Main.player[playerIndex];
        packet.Write(playerIndex);
        packet.Write(player.GetValueRef<bool>(AvatarRiftSuckVisualsManager.WasSuckedInVariableName).Value);
        packet.Write(player.GetValueRef<float>(AvatarRiftSuckVisualsManager.ZoomInInterpolantName).Value);
    }

    public override void Read(BinaryReader reader)
    {
        int playerIndex = reader.ReadInt32();
        Player player = Main.player[playerIndex];
        player.GetValueRef<bool>(AvatarRiftSuckVisualsManager.WasSuckedInVariableName).Value = reader.ReadBoolean();
        player.GetValueRef<float>(AvatarRiftSuckVisualsManager.ZoomInInterpolantName).Value = reader.ReadSingle();
    }
}
