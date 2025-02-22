using System.Reflection;
using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NoxusBoss.Core.Graphics.UI;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace NoxusBoss.Core.Graphics.SpecificEffectManagers;

public class NPCNameFontSystem : ModSystem
{
    internal static int npcIDForMouseTextHackZoom;

    private static readonly Dictionary<int, DynamicSpriteFont> specialFontRegistry = [];

    private static readonly Dictionary<string, int> specialFontNameRegistry = [];

    private static readonly FieldInfo npcIDForMouseTextHackZoomField = typeof(NPCNameFontSystem).GetField("npcIDForMouseTextHackZoom", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly FieldInfo bestiaryIconField = typeof(UIBestiaryEntryButton).GetField("_icon", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly MethodInfo assetValueGetter = typeof(Asset<DynamicSpriteFont>).GetProperty("Value")!.GetMethod!;

    public override void OnModLoad()
    {
        Main.QueueMainThreadAction(() =>
        {
            new ManagedILEdit("Draw Health Bars with Custom Fonts", Mod, edit =>
            {
                IL_Main.DrawInterface_14_EntityHealthBars += edit.SubscriptionWrapper;
            }, edit =>
            {
                IL_Main.DrawInterface_14_EntityHealthBars -= edit.SubscriptionWrapper;
            }, DrawHealthBarsWithFont).Apply();

            new ManagedILEdit("Draw Mouse Text with Custom Fonts", Mod, edit =>
            {
                IL_Main.MouseTextInner += edit.SubscriptionWrapper;
            }, edit =>
            {
                IL_Main.MouseTextInner -= edit.SubscriptionWrapper;
            }, DrawTextWithFont).Apply();

            new ManagedILEdit("Draw Bestiary Text with Custom Fonts", Mod, edit =>
            {
                IL_NamePlateInfoElement.ProvideUIElement += edit.SubscriptionWrapper;
            }, edit =>
            {
                IL_NamePlateInfoElement.ProvideUIElement -= edit.SubscriptionWrapper;
            }, DrawBestiaryNameWithFont).Apply();

            new ManagedILEdit("Draw NPC Hover Text with Custom Fonts", Mod, edit =>
            {
                IL_Main.HoverOverNPCs += edit.SubscriptionWrapper;
            }, edit =>
            {
                IL_Main.HoverOverNPCs -= edit.SubscriptionWrapper;
            }, DrawHoverTextWithFont).Apply();

            On_UIBestiaryEntryButton.DrawSelf += DrawBestiaryHoverNameWithFont;
        });
    }

    private void DrawHealthBarsWithFont(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);

        // Search for the 'for (int i = 199; i >= 0; i--)' loop, attempting to find the local index of i.
        int iterationValueIndex = 0;
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcI4(199), i => i.MatchStloc(out iterationValueIndex)))
        {
            edit.LogFailure("The 'for (int i = 199; i >= 0; i--)' check could not be found! Boss health bars could not be applied with special fonts!");
            return;
        }

        // Search through every single instance of Asset<DynamicSpriteFont>.Value and perform dynamic replacements.
        while (cursor.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt(assetValueGetter)))
        {
            cursor.Emit(OpCodes.Ldloc, iterationValueIndex);
            cursor.EmitDelegate((int npcIndex) => Main.npc[npcIndex].type);
            cursor.EmitDelegate(DecideFontForNPCID);
        }
    }

    private void DrawTextWithFont(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);

        // Search through every single instance of Asset<DynamicSpriteFont>.Value and perform dynamic replacements.
        while (cursor.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt(assetValueGetter)))
        {
            cursor.Emit(OpCodes.Ldsfld, npcIDForMouseTextHackZoomField);
            cursor.EmitDelegate(DecideFontForNPCID);
        }
    }

    private void DrawBestiaryNameWithFont(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);

        FieldInfo? npcNetIDField = typeof(NamePlateInfoElement).GetField("_npcNetId", BindingFlags.NonPublic | BindingFlags.Instance);
        if (npcNetIDField is null)
        {
            edit.LogFailure("Could not find the _npcNetId field.");
            return;
        }

        FieldInfo? keyField = typeof(NamePlateInfoElement).GetField("_key", BindingFlags.NonPublic | BindingFlags.Instance);
        if (keyField is null)
        {
            edit.LogFailure("Could not find the _key field.");
            return;
        }

        cursor.GotoNext(MoveType.Before, i => i.MatchStloc0());

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldfld, npcNetIDField);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldfld, keyField);
        cursor.EmitDelegate((UIElement element, int npcID, string textKey) =>
        {
            if (specialFontRegistry.TryGetValue(npcID, out DynamicSpriteFont? specialFont))
                return new UITextDynamic(Language.GetText(textKey), Color.White, 0.6f, specialFont, new Vector2(0f, -0.5f));

            return element;
        });
    }

    private void DrawBestiaryHoverNameWithFont(On_UIBestiaryEntryButton.orig_DrawSelf orig, UIBestiaryEntryButton self, SpriteBatch spriteBatch)
    {
        if (self.IsMouseHovering)
        {
            object? iconWrapped = bestiaryIconField.GetValue(self);
            if (iconWrapped is not null)
            {
                UIBestiaryEntryIcon icon = (UIBestiaryEntryIcon)iconWrapped;
                if (specialFontNameRegistry.TryGetValue(icon.GetHoverText(), out int npcID))
                    npcIDForMouseTextHackZoom = npcID;
            }
        }

        orig(self, spriteBatch);
    }

    private void DrawHoverTextWithFont(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);

        // Search for the npc's given or type name.
        // Search for the 'for (int i = 0; i < Main.maxNPCs; i++)' loop, attempting to find the local index of i.
        int iterationValueIndex = 0;
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdsfld<Main>("maxNPCs") || i.MatchLdcI4(Main.maxNPCs)))
        {
            edit.LogFailure("The 'for (int i = 0; i < Main.maxNPCs; i++)' check could not be found! Hover text could not be applied with special fonts!");
            return;
        }

        if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchLdloc(out iterationValueIndex)))
        {
            edit.LogFailure("The i variable could not be found! Hover text could not be applied with special fonts!");
            return;
        }

        // Go to the GivenOrTypeName call.
        if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchCallOrCallvirt<NPC>("get_GivenOrTypeName")))
        {
            edit.LogFailure("The GivenOrTypeName getter call could not be found! Hover text could not be applied with special fonts!");
            return;
        }

        cursor.Emit(OpCodes.Ldloc, iterationValueIndex);
        cursor.EmitDelegate((int npcIndex) =>
        {
            npcIDForMouseTextHackZoom = Main.npc[npcIndex].type;
        });
    }

    // Useful shorthand for font replacements in IL edits.
    private static DynamicSpriteFont DecideFontForNPCID(DynamicSpriteFont originalFont, int npcID)
    {
        if (specialFontRegistry.TryGetValue(npcID, out DynamicSpriteFont? specialFont))
            return specialFont;

        return originalFont;
    }

    public override void UpdateUI(GameTime gameTime) => npcIDForMouseTextHackZoom = 0;

    public static void RegisterFontForNPCID(int npcID, string npcName, DynamicSpriteFont font)
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        specialFontRegistry[npcID] = font;
        specialFontNameRegistry[npcName] = npcID;
    }
}
