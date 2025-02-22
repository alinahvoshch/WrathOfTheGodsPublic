using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.FormPresets;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Walls;

public class EternalGardenGrassWall : ModWall
{
    internal static LazyAsset<Texture2D> MyTexture;

    internal static LazyAsset<Texture2D> AutumnalTexture;

    public override string Texture => GetAssetPath("Content/Walls", Name);

    public override void SetStaticDefaults()
    {
        MyTexture = LazyAsset<Texture2D>.FromPath(Texture);
        AutumnalTexture = LazyAsset<Texture2D>.FromPath($"{Texture}Autumnal");

        DustType = DustID.Grass;
        AddMapEntry(new Color(30, 80, 48));
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        Color color = WorldGen.paintColor(tile.WallColor).MultiplyRGBA(Lighting.GetColor(i, j));
        color.A = 255;
        Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new(Main.offScreenRange, Main.offScreenRange);
        Vector2 drawPosition = new Vector2(i * 16f, j * 16f) + drawOffset - Main.screenPosition;
        Rectangle frame = new Rectangle(tile.WallFrameX, tile.WallFrameY, 32, 32);

        Texture2D texture = (NamelessDeityFormPresetRegistry.UsingLucillePreset ? AutumnalTexture : MyTexture).Value;
        spriteBatch.Draw(texture, drawPosition, frame, Lighting.GetColor(i, j, Color.White), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        return false;
    }
}
