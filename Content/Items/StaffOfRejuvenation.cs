using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public class StaffOfRejuvenation : ModItem
{
    // The layout of this class must be consistent with that of the analogous private class used by SmartCursorHelper, hence the disabling of warnings.
#pragma warning disable 
    private class SmartCursorUsageInfo
    {
        public Player player;

        public Item item;

        public Vector2 mouse;

        public Vector2 position;

        public Vector2 Center;

        public int screenTargetX;

        public int screenTargetY;

        public int reachableStartX;

        public int reachableEndX;

        public int reachableStartY;

        public int reachableEndY;

        public int paintLookup;

        public int paintCoatingLookup;
    }
#pragma warning enable

    public override string Texture => GetAssetPath("Content/Items", Name);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        On_SmartCursorHelper.Step_StaffOfRegrowth += AddSmartCursorCompatibility;
    }

    private static void AddSmartCursorCompatibility(On_SmartCursorHelper.orig_Step_StaffOfRegrowth orig, object providedInfoWrapped, ref int focusedX, ref int focusedY)
    {
        SmartCursorUsageInfo providedInfo = Unsafe.As<SmartCursorUsageInfo>(providedInfoWrapped);
        orig(providedInfoWrapped, ref focusedX, ref focusedY);

        if ((providedInfo.item.type != ModContent.ItemType<StaffOfRejuvenation>()) || focusedX != -1 || focusedY != -1)
            return;

        // Search for valid dirt or grass in the reachable area that can be converted to eternal garden grass.
        List<Tuple<int, int>> candidates = new List<Tuple<int, int>>();
        for (int i = providedInfo.reachableStartX; i <= providedInfo.reachableEndX; i++)
        {
            for (int j = providedInfo.reachableStartY; j <= providedInfo.reachableEndY; j++)
            {
                Tile tile = Main.tile[i, j];
                bool flag = !Main.tile[i - 1, j].HasTile || !Main.tile[i, j + 1].HasTile || !Main.tile[i + 1, j].HasTile || !Main.tile[i, j - 1].HasTile;
                bool flag2 = !Main.tile[i - 1, j - 1].HasTile || !Main.tile[i - 1, j + 1].HasTile || !Main.tile[i + 1, j + 1].HasTile || !Main.tile[i + 1, j - 1].HasTile;
                bool isValidTileID = tile.TileType == TileID.Dirt || tile.TileType == TileID.Grass;

                if (tile.HasTile && !tile.IsActuated && isValidTileID && (flag || (isValidTileID && flag2)))
                    candidates.Add(new Tuple<int, int>(i, j));
            }
        }
        if (candidates.Count > 0)
        {
            float bestDistance = 9999999f;
            Tuple<int, int> chosenCandidate = candidates[0];
            for (int k = 0; k < candidates.Count; k++)
            {
                float distanceToMouse = Vector2.Distance(new Vector2(candidates[k].Item1, candidates[k].Item2) * 16f + Vector2.One * 8f, providedInfo.mouse);
                if (distanceToMouse < bestDistance)
                {
                    bestDistance = distanceToMouse;
                    chosenCandidate = candidates[k];
                }
            }
            if (Collision.InTileBounds(chosenCandidate.Item1, chosenCandidate.Item2, providedInfo.reachableStartX, providedInfo.reachableStartY, providedInfo.reachableEndX, providedInfo.reachableEndY))
            {
                focusedX = chosenCandidate.Item1;
                focusedY = chosenCandidate.Item2;
            }
        }
    }

    public override void SetDefaults()
    {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 10;
        Item.useTime = 6;
        Item.autoReuse = true;
        Item.width = 24;
        Item.height = 28;
        Item.damage = 20;
        Item.DamageType = DamageClass.Melee;
        Item.UseSound = SoundID.Item1;
        Item.knockBack = 3f;
        Item.rare = ModContent.RarityType<SolynRewardRarity>();
        Item.useStyle = ItemUseStyleID.Swing;
    }

    public override bool? UseItem(Player player)
    {
        if (player.itemAnimation != player.itemAnimationMax - 1)
            return null;

        Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
        bool canBeTransformed = tile.TileType == TileID.Dirt || tile.TileType == TileID.Grass;
        if (player.inventory[player.selectedItem].type == Type && canBeTransformed && tile.HasTile)
        {
            SoundEngine.PlaySound(SoundID.Dig, player.Center);

            tile.TileType = (ushort)ModContent.TileType<EternalGardenGrass>();
            WorldGen.SquareTileFrame(Player.tileTargetX, Player.tileTargetY);
            NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY);
        }
        return null;
    }
}
