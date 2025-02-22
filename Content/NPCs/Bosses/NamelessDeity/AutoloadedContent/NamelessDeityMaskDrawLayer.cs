using Microsoft.Xna.Framework;
using NoxusBoss.Core.DataStructures;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.AutoloadedContent;

public class NamelessDeityMaskDrawLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        int headItemID = drawInfo.drawPlayer.armor[0].type;
        if (drawInfo.drawPlayer.armor[10].type != ItemID.None)
            headItemID = drawInfo.drawPlayer.armor[10].type;

        return headItemID == NamelessDeityBoss.MaskID;
    }

    public override Position GetDefaultPosition() => PlayerDrawLayers.AfterLastVanillaLayer;

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (drawInfo.shadow != 0f)
            return;

        Vector2 idealCensorPosition = drawInfo.Center - Vector2.UnitY.RotatedBy(drawInfo.drawPlayer.fullRotation) * 8f;
        Referenced<Vector2> censorPosition = drawInfo.drawPlayer.GetValueRef<Vector2>("NamelessDeityMaskCensorPosition");
        if (!censorPosition.Value.WithinRange(idealCensorPosition, 6f) || Main.gameMenu)
            censorPosition.Value = idealCensorPosition;

        DrawData censor = new DrawData(WhitePixel, censorPosition.Value.Floor() - Main.screenPosition + Vector2.UnitX * drawInfo.drawPlayer.gfxOffY, null, Color.Black, 0f, WhitePixel.Size() * 0.5f, new Vector2(22f, 30f), 0, 0f);
        drawInfo.DrawDataCache.Add(censor);
    }
}
