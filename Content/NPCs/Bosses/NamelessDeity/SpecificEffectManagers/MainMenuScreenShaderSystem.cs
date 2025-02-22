using Luminance.Core.Graphics;
using Luminance.Core.Hooking;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NoxusBoss.Core.Graphics.Shaders.Screen;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;

public class MainMenuScreenShaderSystem : ModSystem
{
    public override void OnModLoad()
    {
        Asset<Effect> s = Mod.Assets.Request<Effect>("Assets/AutoloadedEffects/Shaders/OverlayModifiers/MainMenuScreenShakeShader");
        Filters.Scene[MainMenuScreenShakeShaderData.ShaderKey] = new Filter(new MainMenuScreenShakeShaderData(s, ManagedShader.DefaultPassName), EffectPriority.VeryHigh);

        new ManagedILEdit("Allow screen shake on the main menu.", Mod, edit =>
        {
            IL_Main.DoDraw += edit.SubscriptionWrapper;
        }, edit =>
        {
            IL_Main.DoDraw -= edit.SubscriptionWrapper;
        }, AllowMainMenuShake).Apply(true);
    }

    private static void AllowMainMenuShake(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt<Lighting>("get_NotRetro")))
        {
            edit.LogFailure("The Lighting.NotRetro property could not be found.");
            return;
        }

        // Go to the "if (Main.gameMenu || Main.netMode == 2)" check that disallows screen shaders and disable the game menu check if the screen should be shook.
        if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchLdsfld<Main>("gameMenu")))
        {
            edit.LogFailure("The Main.gameMenu field load could not be found.");
            return;
        }

        cursor.EmitDelegate(() => MainMenuScreenShakeShaderData.ScreenShakeIntensity <= 0.01f);
        cursor.Emit(OpCodes.And);
    }
}
