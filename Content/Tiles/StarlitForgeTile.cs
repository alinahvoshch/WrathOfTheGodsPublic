using System.Reflection;
using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NoxusBoss.Assets;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NoxusBoss.Content.Tiles;

public class StarlitForgeTile : ModTile
{
    /// <summary>
    /// The tiled width of this forge.
    /// </summary>
    public const int Width = 3;

    /// <summary>
    /// The tiled height of this forge.
    /// </summary>
    public const int Height = 2;

    /// <summary>
    /// The glowmask of this forge.
    /// </summary>
    public static LazyAsset<Texture2D> Glowmask
    {
        get;
        private set;
    }

    public override string Texture => GetAssetPath("Content/Tiles", Name);

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLighted[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.Width = Width;
        TileObjectData.newTile.Height = Height;
        TileObjectData.newTile.Origin = new Point16(Width / 2, Height - 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height).ToArray();
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);
        AddMapEntry(new Color(98, 98, 98));

        HitSound = SoundID.Tink;

        // NO ONE CAN STOP ME!!!!
        AdjTiles = Enumerable.Range(0, TileLoader.TileCount).ToArray();

        On_Recipe.PlayerMeetsEnvironmentConditions += EnableAllEnvironmentConditionsForForge;
        On_Recipe.PlayerMeetsTileRequirements += EnableAllTileConditionsForForge;
        new ManagedILEdit("Remove Recipe Restrictions for Starlit Forge", Mod, e => IL_Recipe.FindRecipes += e.SubscriptionWrapper, e => IL_Recipe.FindRecipes -= e.SubscriptionWrapper, RemoveRecipeConditions).Apply();

        if (Main.netMode != NetmodeID.Server)
            Glowmask = LazyAsset<Texture2D>.FromPath($"{Texture}Glowmask");
    }

    private void RemoveRecipeConditions(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);
        MethodInfo? recipeAvailabilityMethod = typeof(RecipeLoader).GetMethod("RecipeAvailable");

        if (recipeAvailabilityMethod is null || !cursor.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt(recipeAvailabilityMethod)))
        {
            edit.LogFailure("Could not locate the RecipeAvailable load.");
            return;
        }
        cursor.EmitDelegate(() => Main.LocalPlayer.adjTile[Type]);
        cursor.Emit(OpCodes.Or);
    }

    private bool EnableAllEnvironmentConditionsForForge(On_Recipe.orig_PlayerMeetsEnvironmentConditions orig, Player player, Recipe tempRec)
    {
        if (player.adjTile[Type])
            return true;

        return orig(player, tempRec);
    }

    private bool EnableAllTileConditionsForForge(On_Recipe.orig_PlayerMeetsTileRequirements orig, Player player, Recipe tempRec)
    {
        if (player.adjTile[Type])
            return true;

        return orig(player, tempRec);
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r += 0.3f;
        g += 0.04f;
        b += 0.09f;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        Vector2 drawPosition = new Vector2(i * 16, j * 16 + 2) - Main.screenPosition + (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange));
        Rectangle frame = new Rectangle(tile.TileFrameX, tile.TileFrameY + AnimationFrameHeight * Main.tileFrame[Type], 16, 16);
        spriteBatch.Draw(Glowmask.Value, drawPosition, frame, Color.White);
    }
}
