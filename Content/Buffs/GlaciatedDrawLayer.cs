using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Buffs;

public class GlaciatedDrawLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.HasBuff<Glaciated>();

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Texture2D texture = GennedAssets.Textures.Buffs.GlaciatedIceBlock.Value;
        Vector2 drawPosition = drawInfo.Center - Main.screenPosition;
        drawInfo.DrawDataCache.Add(new DrawData(texture, drawPosition, null, Color.White * 0.65f, 0f, texture.Size() * 0.5f, 1.35f, SpriteEffects.None, 0));
    }
}
