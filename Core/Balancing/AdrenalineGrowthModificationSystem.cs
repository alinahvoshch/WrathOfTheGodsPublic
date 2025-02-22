using System.Reflection;
using CalamityMod.CalPlayer;
using Luminance.Core.Hooking;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Balancing;

[JITWhenModsEnabled(CalamityCompatibility.ModName)]
[ExtendsFromMod(CalamityCompatibility.ModName)]
public sealed class AdrenalineGrowthModificationSystem : ModSystem
{
    /// <summary>
    /// The IL edit responsible for the modifying per-frame adrenaline yields.
    /// </summary>
    public static ILHook AdrenalineChargeUpHook
    {
        get;
        private set;
    }

    /// <summary>
    /// The amount by which adrenaline yields are multiplied. This only applies if adrenaline is charging up, rather than down.
    /// </summary>
    public static float AdrenalineYieldFactor
    {
        get;
        set;
    }

    public override void OnModLoad()
    {
        MethodInfo? updateRippersMethod = typeof(CalamityPlayer).GetMethod("UpdateRippers", UniversalBindingFlags);
        if (updateRippersMethod is null)
        {
            Mod.Logger.Warn("Could not find the UpdateRippers method in CalamityPlayer.");
            return;
        }

        new ManagedILEdit("Tweak Per-frame Adrenaline Yields", Mod, edit =>
        {
            AdrenalineChargeUpHook = new(updateRippersMethod, edit.SubscriptionWrapper);
        }, _ =>
        {
            AdrenalineChargeUpHook?.Undo();
        }, UpdateAdrenalineYields).Apply();
    }

    public override void PreUpdateNPCs() => AdrenalineYieldFactor = 1f;

    private static void UpdateAdrenalineYields(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);

        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchStfld<CalamityPlayer>("adrenaline")))
        {
            edit.LogFailure("The CalamityPlayer.adrenaline storage could not be found.");
            return;
        }
        if (!cursor.TryGotoPrev(MoveType.Before, i => i.MatchAdd()))
        {
            edit.LogFailure("The Add opcode could not be found.");
            return;
        }

        cursor.EmitDelegate((float originalAdrenalineDifference) =>
        {
            if (originalAdrenalineDifference > 0f)
                originalAdrenalineDifference *= AdrenalineYieldFactor;

            return originalAdrenalineDifference;
        });
    }
}
