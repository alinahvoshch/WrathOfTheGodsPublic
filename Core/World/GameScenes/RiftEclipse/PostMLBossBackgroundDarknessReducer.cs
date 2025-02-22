using System.Reflection;
using CalamityMod.Events;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.Skies;
using Luminance.Core.Hooking;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.RiftEclipse;

// Done to make the Avatar's rift slightly visible during certain post-ML battles.
[ExtendsFromMod(CalamityCompatibility.ModName)]
[JITWhenModsEnabled(CalamityCompatibility.ModName)]
public class PostMLBossBackgroundDarknessReducer : ModSystem
{
    public delegate float GetIntensityOrig(object instance);

    public delegate float GetIntensityHook(GetIntensityOrig orig, object instance);

    /// <summary>
    /// The amount by which darkness is reduced during Calamitas' battle.
    /// </summary>
    public static float DarknessReduction => RiftEclipseManagementSystem.RiftEclipseOngoing && !BossRushEvent.BossRushActive ? 0.167f : 0f;

    public override void OnModLoad()
    {
        TryToApplyDetour(typeof(DoGSky).GetMethod("GetIntensity", UniversalBindingFlags));
        TryToApplyDetour(typeof(SCalSky).GetMethod("GetIntensity", UniversalBindingFlags));
    }

    private static void TryToApplyDetour(MethodInfo? method)
    {
        if (method is null)
            return;

        HookHelper.ModifyMethodWithDetour(method, new GetIntensityHook((orig, instance) =>
        {
            return orig(instance) * (1f - DarknessReduction);
        }));
    }
}
