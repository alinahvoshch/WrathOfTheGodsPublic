using System.Reflection;
using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.SoundSystems;

// Credits to Iban for these IL edits and the associated interface (but this should really just be a base TML feature lmao?)
public class TilePlaceSoundReplacerSystem : ModSystem
{
    public override void OnModLoad()
    {
        new ManagedILEdit("Use Custom Sounds in PlaceTile", Mod, edit =>
        {
            IL_WorldGen.PlaceTile += edit.SubscriptionWrapper;
        }, edit =>
        {
            IL_WorldGen.PlaceTile -= edit.SubscriptionWrapper;
        }, ReplacePlaceSounds_PlaceTile).Apply();

        new ManagedILEdit("Use Custom Sounds in PlaceObject", Mod, edit =>
        {
            IL_WorldGen.PlaceObject += edit.SubscriptionWrapper;
        }, edit =>
        {
            IL_WorldGen.PlaceObject -= edit.SubscriptionWrapper;
        }, ReplacePlaceSounds_PlaceObject).Apply();

        new ManagedILEdit("Use Custom Sounds in PlaceThing_Tiles_PlaceIt ", Mod, edit =>
        {
            IL_Player.PlaceThing_Tiles_PlaceIt += edit.SubscriptionWrapper;
        }, edit =>
        {
            IL_Player.PlaceThing_Tiles_PlaceIt -= edit.SubscriptionWrapper;
        }, ReplacePlaceSounds_PlayerPlacement).Apply();
    }

    private static void ReplacePlaceSounds_PlaceObject(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);
        ILLabel? endOfMethodBranch = null;

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdarg(3), i => i.MatchBrtrue(out endOfMethodBranch)) || endOfMethodBranch is null)
        {
            edit.LogFailure("The mute check, along with its associated branch, could not be found.");
            return;
        }

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.Emit(OpCodes.Ldarg_2);
        cursor.EmitDelegate(ReplaceSoundIfNecessary);
        cursor.Emit(OpCodes.Brtrue, endOfMethodBranch);
    }

    private static void ReplacePlaceSounds_PlaceTile(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);
        ILLabel returnLabel = cursor.DefineLabel();
        MethodInfo playSoundMethod = typeof(SoundEngine).GetMethod("PlaySound", BindingFlags.NonPublic | BindingFlags.Static, new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(float), typeof(float) })!;

        if (!cursor.TryGotoNext(MoveType.After,
            i => i.MatchLdarg(3),
            i => i.MatchBrtrue(out _),
            i => i.MatchLdsfld<WorldGen>("generatingWorld"),
            i => i.MatchBrtrue(out _)
            ))
        {
            edit.LogFailure("Could not locate the if (!mute && !worldgen) check.");
            return;
        }

        if (!cursor.TryGotoNext(MoveType.AfterLabel,
            i => i.MatchLdcI4(0),
            i => i.MatchLdarg(0),
            i => i.MatchLdcI4(16),
            i => i.MatchMul(),
            i => i.MatchLdarg(1),
            i => i.MatchLdcI4(16),
            i => i.MatchMul(),
            i => i.MatchLdcI4(1),
            i => i.MatchLdcR4(1),
            i => i.MatchLdcR4(0),

            // int type, int x = -1, int y = -1, int Style = 1, float volumeScale = 1f, float pitchOffset = 0f)
            i => i.MatchCall(playSoundMethod),
            i => i.MatchPop()
            ))
        {
            edit.LogFailure("Could not move before the PlaySound call.");
            return;
        }

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.Emit(OpCodes.Ldarg_2);
        cursor.EmitDelegate(ReplaceSoundIfNecessary);
        cursor.Emit(OpCodes.Brtrue, returnLabel);

        if (!cursor.TryGotoNext(MoveType.After,
            i => i.MatchLdcI4(0),
            i => i.MatchLdarg(0),
            i => i.MatchLdcI4(16),
            i => i.MatchMul(),
            i => i.MatchLdarg(1),
            i => i.MatchLdcI4(16),
            i => i.MatchMul(),
            i => i.MatchLdcI4(1),
            i => i.MatchLdcR4(1),
            i => i.MatchLdcR4(0),
            i => i.MatchCall(playSoundMethod),
            i => i.MatchPop()
            ))
        {
            edit.LogFailure("Could not move after the PlaySound call.");
            return;
        }

        cursor.MarkLabel(returnLabel);
    }

    private static void ReplacePlaceSounds_PlayerPlacement(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);
        ILLabel? returnLabel = null;
        ILLabel? soundPlayStartLabel = null;

        if (!cursor.TryGotoNext(MoveType.Before,
            i => i.MatchLdsfld<Main>("netMode"),
            i => i.MatchLdcI4(1),
            i => i.MatchBneUn(out soundPlayStartLabel),

            i => i.MatchLdsfld(typeof(TileID.Sets).GetField("IsAContainer", BindingFlags.Static | BindingFlags.Public)!),
            i => i.MatchLdarg(3),
            i => i.MatchLdelemU1(),
            i => i.MatchBrtrue(out returnLabel)
            ) || soundPlayStartLabel is null || returnLabel is null)
        {
            edit.LogFailure("Could not locate the (Main.netMode != NetmodeID.MultiplayerClient || !TileID.Sets.IsAContainer[tileToCreate]) check.");
            return;
        }

        cursor.GotoLabel(soundPlayStartLabel);

        cursor.Emit(OpCodes.Ldarg_3);
        cursor.EmitDelegate(ReplaceSoundIfNecessary_Player);
        cursor.Emit(OpCodes.Brtrue, returnLabel);
    }

    private static bool ReplaceSoundIfNecessary(int x, int y, int tileType)
    {
        if (TileLoader.GetTile(tileType) is ICustomPlacementSound c)
        {
            SoundEngine.PlaySound(c.PlaceSound, new Vector2(x * 16f, y * 16f));
            return true;
        }

        return false;
    }

    private static bool ReplaceSoundIfNecessary_Player(int tileType)
    {
        if (TileLoader.GetTile(tileType) is ICustomPlacementSound customSoundsTile)
        {
            SoundEngine.PlaySound(customSoundsTile.PlaceSound, new Vector2(Player.tileTargetX * 16f, Player.tileTargetY * 16f));
            return true;
        }
        return false;
    }
}
