using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace NoxusBoss.Content.NPCs.Bosses.CeaselessVoid;

public class CeaselessVoidRiftMapLayer : ModMapLayer
{
    public override void Draw(ref MapOverlayDrawContext context, ref string text)
    {
        int riftIndex = NPC.FindFirstNPC(ModContent.NPCType<CeaselessVoidRift>());
        if (riftIndex == -1)
            return;

        Texture2D icon = CeaselessVoidRiftTargetManager.RiftIconTarget;
        if (context.Draw(icon, Main.npc[riftIndex].Center.ToTileCoordinates().ToVector2(), Alignment.Center).IsMouseOver)
            text = Language.GetTextValue("Mods.NoxusBoss.NPCs.CeaselessVoidRift.MapIconText");
    }
}
