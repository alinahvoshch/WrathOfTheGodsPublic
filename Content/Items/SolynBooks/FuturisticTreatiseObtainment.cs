using System.Reflection;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using NoxusBoss.Assets;
using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    internal static float FuturisticTreatiseCodeScale => 0.85f;

    /// <summary>
    /// The chance of the Futuristic Treatise dropping from a given Exo Mech.
    /// </summary>
    public static int FuturisticTreatiseDropChance => 33;

    private static void LoadFuturisticTreatise()
    {
        LoadFuturisticTreatiseObtainment();

        HookHelper.ModifyMethodWithIL(typeof(Main).GetMethod("MouseText_DrawItemTooltip", UniversalBindingFlags), DrawBlackBoxForFuturisticTreatise);
        SolynBookAutoloader.Books["FuturisticTreatise"].ModifyLinesAction = (item, tooltips) =>
        {
            tooltips.RemoveAll(t => t.Name.Contains("Tooltip") || t.Name == "Favorite" || t.Name == "FavoriteDesc");
        };
        SolynBookAutoloader.Books["FuturisticTreatise"].PreDrawTooltipAction += (item, x, y) =>
        {
            if (IsUnobtainedItemInUI(Main.HoverItem))
                return;

            Vector2 drawPosition = new Vector2(x, y);
            Texture2D code = GennedAssets.Textures.SolynBooks.FuturisticTreatiseCode.Value;
            Main.spriteBatch.Draw(code, drawPosition + Vector2.UnitY * 25f, null, Color.White, 0f, Vector2.Zero, FuturisticTreatiseCodeScale, 0, 0f);
        };
    }

    [JITWhenModsEnabled(CalamityCompatibility.ModName)]
    private static void LoadFuturisticTreatiseObtainment()
    {
        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, loot) =>
        {
            if (npc.type == ModContent.NPCType<AresBody>() || npc.type == ModContent.NPCType<ThanatosHead>() || npc.type == ModContent.NPCType<Apollo>())
                loot.Add(new CommonDrop(SolynBookAutoloader.Books["FuturisticTreatise"].Type, FuturisticTreatiseDropChance));
        };
    }

    internal static bool IsUnobtainedItemInUI(Item item) => item.TryGetGlobalItem(out InstancedGlobalItem globalItem) && globalItem.UnobtainedSolynBook;

    private static void DrawBlackBoxForFuturisticTreatise(ILContext c)
    {
        ILCursor cursor = new ILCursor(c);

        // Search for the number 81, since the box is drawn with an RGBA value of (23, 25, 81, 255).
        if (!cursor.TryGotoNext(i => i.MatchLdcI4(81)))
        {
            ModContent.GetInstance<NoxusBoss>().Logger.Error("Could not apply the IL edit for the tooltip box for the futuristic treatise. The blue value of 81 could not be located.");
            return;
        }

        // Search for the created color, after the opacity multiplication of 0.925.
        MethodInfo? colorFloatMultiply = typeof(Color).GetMethod("op_Multiply", [typeof(Color), typeof(float)]);
        if (colorFloatMultiply is null || !cursor.TryGotoNext(MoveType.After, i => i.MatchCall(colorFloatMultiply)))
        {
            ModContent.GetInstance<NoxusBoss>().Logger.Error("Could not apply the IL edit for the tooltip box for the futuristic treatise. The Color object creation could not be located.");
            return;
        }

        // Take in the newly created color and modify it safely.
        cursor.EmitDelegate((Color originalColor) =>
        {
            if (Main.HoverItem.type == SolynBookAutoloader.Books["FuturisticTreatise"].Type && !IsUnobtainedItemInUI(Main.HoverItem))
                return new Color(30, 30, 30);

            return originalColor;
        });

        // Go back to the first Vector2.Zero load. This is used to express the dimensions of the box, and must be expanded a bit in order to
        // ensure that the exo mechs lore item image properly renders, given that there's no actual text to properly define the boundaries.
        cursor.Goto(0);
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCall<Vector2>("get_Zero")))
        {
            ModContent.GetInstance<NoxusBoss>().Logger.Error("Could not apply the IL edit for the tooltip box for the futuristic treatise. The Vector2.Zero load could not be located.");
            return;
        }
        cursor.EmitDelegate((Vector2 originalBaseDimensions) =>
        {
            if (Main.HoverItem.type == SolynBookAutoloader.Books["FuturisticTreatise"].Type && !IsUnobtainedItemInUI(Main.HoverItem))
            {
                Texture2D code = GennedAssets.Textures.SolynBooks.FuturisticTreatiseCode.Value;
                originalBaseDimensions.X += code.Width * FuturisticTreatiseCodeScale;
                originalBaseDimensions.Y += code.Height * FuturisticTreatiseCodeScale;
            }

            return originalBaseDimensions;
        });
    }
}
