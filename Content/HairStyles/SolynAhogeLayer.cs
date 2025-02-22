using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Content.HairStyles;

public class SolynAhogeLayer : PlayerDrawLayer
{
    public override bool IsHeadLayer => true;

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        if (drawInfo.drawPlayer.hair != ModContent.GetInstance<SolynHairStyle>().Type)
            return false;

        if (drawInfo.drawPlayer.head != -1 && drawInfo.drawPlayer.head != 0)
            return false;

        return true;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Player drawPlayer = drawInfo.drawPlayer;

        Vector2 headDrawPosition = drawInfo.Position - Main.screenPosition;
        headDrawPosition += new Vector2((drawPlayer.width - drawPlayer.bodyFrame.Width) * 0.5f + drawPlayer.direction * -1f, drawPlayer.height - drawPlayer.bodyFrame.Height);
        headDrawPosition += drawPlayer.headPosition + drawInfo.headVect;
        headDrawPosition -= Vector2.UnitY.RotatedBy(drawPlayer.headRotation) * 10f;
        headDrawPosition = headDrawPosition.Floor();
        if (headDrawPosition.Y % 2f == 1f)
            headDrawPosition.Y++;
        if (drawInfo.hairFrontFrame.Y == 56 || drawInfo.hairFrontFrame.Y == 112 || drawInfo.hairFrontFrame.Y == 168 || drawInfo.hairFrontFrame.Y == 448 || drawInfo.hairFrontFrame.Y == 504 || drawInfo.hairFrontFrame.Y == 560)
            headDrawPosition.Y -= 2f;
        headDrawPosition.Y += Main.gameMenu ? 6f : 5f;

        Texture2D ahoge = GennedAssets.Textures.HairStyles.SolynAhoge.Value;
        DrawData ahogeData = new DrawData(ahoge, headDrawPosition, new Rectangle(0, 0, ahoge.Width, drawInfo.hairBackFrame.Height), drawInfo.colorHair, drawPlayer.headRotation, ahoge.Size() * new Vector2(0.5f, 1f), 1f, drawInfo.playerEffect, 0f)
        {
            shader = drawInfo.hairDyePacked
        };
        drawInfo.DrawDataCache.Insert(0, ahogeData);
    }
}
