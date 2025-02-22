using System.Reflection;
using Luminance.Core.Graphics;
using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using NoxusBoss.Assets;
using Terraria;
using Terraria.ModLoader;
using static NoxusBoss.Core.Autoloaders.SolynBooks.SolynBookAutoloader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    private static void LoadBookOfShaders()
    {
        new ManagedILEdit("Use shader on Book of Shaders tooltip background", ModContent.GetInstance<NoxusBoss>(), edit =>
        {
            IL_Main.MouseText_DrawItemTooltip += edit.SubscriptionWrapper;
        }, edit =>
        {
            IL_Main.MouseText_DrawItemTooltip -= edit.SubscriptionWrapper;
        }, UseBookOfShadersBgShader).Apply();
    }

    private static void UseBookOfShadersBgShader(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);
        MethodInfo drawInventoryBgMethod = typeof(Utils).GetMethod("DrawInvBG", UniversalBindingFlags, new Type[]
        {
            typeof(SpriteBatch),
            typeof(Rectangle),
            typeof(Color)
        })!;
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchCallOrCallvirt(drawInventoryBgMethod)))
        {
            edit.LogFailure("Could not find the DrawInvBG call.");
            return;
        }
        cursor.EmitDelegate(() =>
        {
            if (Main.HoverItem.type == Books["TheBookOfShaders"].Type && !IsUnobtainedItemInUI(Main.HoverItem))
            {
                Main.spriteBatch.PrepareForShaders(null, true);

                ManagedShader bookOfShadersShader = ShaderManager.GetShader("NoxusBoss.BookOfShadersBackgroundShader");
                bookOfShadersShader.SetTexture(GennedAssets.Textures.Extra.Psychedelic, 1, SamplerState.LinearWrap);
                bookOfShadersShader.SetTexture(GennedAssets.Textures.Extra.PsychedelicWingTextureOffsetMap, 2, SamplerState.LinearWrap);
                bookOfShadersShader.Apply();
            }
        });

        cursor.Goto(0);
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt(drawInventoryBgMethod)))
        {
            edit.LogFailure("Could not find the DrawInvBG call.");
            return;
        }
        cursor.EmitDelegate(() =>
        {
            if (Main.HoverItem.type == Books["TheBookOfShaders"].Type && !IsUnobtainedItemInUI(Main.HoverItem))
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            }
        });
    }
}
