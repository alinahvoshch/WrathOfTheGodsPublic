using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.DialogueSystem;
using NoxusBoss.Core.World.TileDisabling;
using NoxusBoss.Core.World.WorldGeneration;
using Terraria;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace NoxusBoss.Core.World;

public class PermafrostKeepMapIconLayer : ModMapLayer
{
    public override void Draw(ref MapOverlayDrawContext context, ref string text)
    {
        if (!DialogueManager.FindByRelativePrefix("DormantKeyDiscussion").SeenBefore("Conversation7"))
            return;

        if (TileDisablingSystem.TilesAreUninteractable || PermafrostKeepWorldGen.KeepArea == Rectangle.Empty)
            return;

        Texture2D icon = GennedAssets.Textures.Map.PermafrostKeepMapIcon;
        if (context.Draw(icon, PermafrostKeepWorldGen.KeepArea.Center(), Alignment.Center).IsMouseOver)
            text = Language.GetTextValue("Mods.NoxusBoss.Dialog.PermafrostKeepMapIconText");
    }
}
