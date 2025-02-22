using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NoxusBoss.Content.Tiles.GenesisComponents;

public class GenesisGrassBlades : ModTile
{
    /// <summary>
    /// The amount of variants this grass has.
    /// </summary>
    public const int VariantCount = 10;

    /// <summary>
    /// The glowmask of this grass.
    /// </summary>
    public static LazyAsset<Texture2D> Glowmask
    {
        get;
        private set;
    }

    public override string Texture => GetAssetPath("Content/Tiles/GenesisComponents", Name);

    public override void SetStaticDefaults()
    {
        Main.tileCut[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileWaterDeath[Type] = true;
        Main.tileFrameImportant[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        // Use plant destruction visuals and sounds.
        HitSound = GennedAssets.Sounds.Mining.GenesisGrassBreak;
        DustType = DustID.CrimsonPlants;

        AddMapEntry(new Color(141, 40, 40));

        if (Main.netMode != NetmodeID.Server)
            Glowmask = LazyAsset<Texture2D>.FromPath($"{Texture}Glowmask");
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        Vector2 worldPosition = new Vector2(i, j).ToWorldCoordinates();
        Player nearestPlayer = Main.player[Player.FindClosest(worldPosition, 16, 16)];
        if (nearestPlayer.active)
        {
            if (nearestPlayer.HeldMouseItem().type == ItemID.Sickle)
                yield return new Item(ItemID.Hay, Main.rand.Next(1, 2 + 1));
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameY != 0)
            return false;

        float windOffset = 0f;
        Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
        SpriteEffects direction = (i * 8 + j * 17) % 3 == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        for (int k = 1; k >= 0; k--)
        {
            Vector2 drawPosition = new Vector2(i * 16, (j + k * 0.9f) * 16 + 4f) - Main.screenPosition + drawOffset;
            drawPosition += new Vector2(8f, 16f);

            Rectangle frame = new Rectangle(tile.TileFrameX, tile.TileFrameY + k * 18, 16, 16);

            int windGridTime = 15;
            Main.instance.TilesRenderer.Wind.GetWindTime(i, j + k, windGridTime, out int windTimeLeft, out int windDirection, out _);
            float windGridInterpolant = windTimeLeft / (float)windGridTime;
            float windGridRotation = Convert01To010(windGridInterpolant) * windDirection * 0.53f;

            Color light = Lighting.GetColor(i, j + k);

            spriteBatch.Draw(TextureAssets.Tile[Type].Value, drawPosition, frame, light, windGridRotation, frame.Size() * new Vector2(0.5f, 1f), 1f, direction, 0f);
            spriteBatch.Draw(Glowmask.Value, drawPosition, frame, Color.White, windOffset + windGridRotation, frame.Size() * new Vector2(0.5f, 1f), 1f, direction, 0f);

            drawOffset.X += Sin(windGridRotation) * 16f;
        }

        return false;
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        Tile t = Framing.GetTileSafely(i, j);
        if (t.TileFrameY != 18)
            return;

        Tile ground = Framing.GetTileSafely(i, j + 1);
        bool groundIsGrass = ground.HasUnactuatedTile && ground.TileType == ModContent.TileType<GenesisGrass>();
        if (!groundIsGrass)
            WorldGen.KillTile(i, j);
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.06f;
        g = 0.02f;
        b = 0.02f;
    }
}
