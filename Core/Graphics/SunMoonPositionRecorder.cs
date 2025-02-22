using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics;

public class SunMoonPositionRecorder : ModSystem
{
    /// <summary>
    /// The position of the sun in the sky.
    /// </summary>
    public static Vector2 SunPosition
    {
        get;
        private set;
    }

    /// <summary>
    /// The position of the moon in the sky.
    /// </summary>
    public static Vector2 MoonPosition
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        new ManagedILEdit("Record the Sun and Moon Draw Positions", Mod, edit =>
        {
            IL_Main.DrawSunAndMoon += edit.SubscriptionWrapper;
        }, edit =>
        {
            IL_Main.DrawSunAndMoon -= edit.SubscriptionWrapper;
        }, RecordSunAndMoonPositions).Apply();
    }

    private static void RecordSunAndMoonPositions(ILContext context, ManagedILEdit edit)
    {
        int sunPositionIndex = 0;
        int moonPositionIndex = 0;
        ILCursor cursor = new ILCursor(context);
        if (!cursor.TryGotoNext(i => i.MatchLdsfld<Main>("sunModY")))
        {
            edit.LogFailure("The Main.sunModY load could not be found.");
            return;
        }
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchStloc(out sunPositionIndex)))
        {
            edit.LogFailure("The sun position local variable storage could not be found.");
            return;
        }

        // Store the sun's draw position.
        cursor.Emit(OpCodes.Ldloc, sunPositionIndex);
        cursor.EmitDelegate<Action<Vector2>>(sunPosition => SunPosition = sunPosition);

        if (!cursor.TryGotoNext(i => i.MatchLdsfld<Main>("moonModY")))
        {
            edit.LogFailure("The Main.moonModY load could not be found.");
            return;
        }
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchStloc(out moonPositionIndex)))
        {
            edit.LogFailure("The moon position local variable storage could not be found.");
            return;
        }

        cursor.Emit(OpCodes.Ldloc, moonPositionIndex);
        cursor.EmitDelegate<Action<Vector2>>(moonPosition => MoonPosition = moonPosition);
    }
}
