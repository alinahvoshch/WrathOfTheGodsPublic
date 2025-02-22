using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    /// <summary>
    /// The chance for the Apple Thesis book to drop from a given tree.
    /// </summary>
    public static int AppleThesisDropChance => 400;

    private static void LoadAppleThesisObtainment()
    {
        GlobalTileEventHandlers.ShakeTreeEvent += (x, y, tileID) =>
        {
            if (Main.rand.NextBool(AppleThesisDropChance))
            {
                int treeTopY = y - 1;
                while (Main.tile[x, treeTopY].HasTile && TileID.Sets.IsShakeable[Main.tile[x, treeTopY].TileType])
                    treeTopY--;

                EntitySource_ShakeTree source = new EntitySource_ShakeTree(x, y);
                Item.NewItem(source, x * 16, treeTopY * 16, 16, 16, SolynBookAutoloader.Books["AppleThesis"].Type);

                int squirrel = NPC.NewNPC(source, x * 16, treeTopY * 16, NPCID.Squirrel);
                if (Main.npc.IndexInRange(squirrel))
                    Main.npc[squirrel].velocity.Y -= 6f;

                return false;
            }

            return true;
        };
    }
}
