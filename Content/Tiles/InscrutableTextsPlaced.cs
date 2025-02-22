using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Autoloaders.SolynBooks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NoxusBoss.Content.Tiles;

public class InscrutableTextsPlaced : ModTile
{
    public override string Texture => GetAssetPath("Content/Tiles", Name);

    public override void SetStaticDefaults()
    {
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);

        RegisterItemDrop(SolynBookAutoloader.Books["InscrutableTexts"].Type);
        AddMapEntry(new Color(45, 105, 204));
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        // Draw the main tile texture.
        Texture2D mainTexture = TextureAssets.Tile[Type].Value;
        Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        Vector2 drawPosition = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y + 2f) + drawOffset;
        Color lightColor = Lighting.GetColor(i, j);
        spriteBatch.Draw(mainTexture, drawPosition, null, lightColor, 0f, Vector2.Zero, 1f, 0, 0f);

        // Draw the outline glow.
        Texture2D glow = GennedAssets.Textures.Tiles.InscrutableTextsPlacedOutline.Value;
        spriteBatch.Draw(glow, drawPosition - Vector2.UnitX * 2f, null, Color.Yellow, 0f, Vector2.Zero, 1f, 0, 0f);

        return false;
    }
}
