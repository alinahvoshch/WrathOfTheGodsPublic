using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Accessories.VanityEffects;

public class PortalSkirtDrawLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.GetValueRef<bool>(PortalSkirt.WearingPortalSkirtVariableName) && !drawInfo.drawPlayer.invis;

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Carpet);

    public override void Load() => On_PlayerDrawLayers.DrawPlayer_13_Leggings += What;

    private void What(On_PlayerDrawLayers.orig_DrawPlayer_13_Leggings orig, ref PlayerDrawSet drawinfo)
    {
        if (drawinfo.drawPlayer.GetValueRef<bool>(PortalSkirt.WearingPortalSkirtVariableName))
        {
            Draw(ref drawinfo);
            return;
        }

        orig(ref drawinfo);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (drawInfo.shadow != 0f || drawInfo.drawPlayer.dead)
            return;

        Texture2D portalTexture = PortalSkirt.PortalTarget;
        Vector2 position = drawInfo.drawPlayer.legPosition + drawInfo.legVect + new Vector2(
            (int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.legFrame.Width * 0.5f + drawInfo.drawPlayer.width * 0.5f),
            (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.legFrame.Height + 4f) + 5
        );

        DrawData rift = new DrawData(portalTexture, position, null, Color.White, drawInfo.drawPlayer.legRotation, portalTexture.Size() * 0.5f, 1f, 0, 0f)
        {
            shader = PortalSkirt.SkirtShaderIndex
        };
        drawInfo.DrawDataCache.Add(rift);
    }
}
