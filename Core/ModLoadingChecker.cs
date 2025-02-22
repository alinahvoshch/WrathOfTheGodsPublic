using System.Reflection;
using Terraria.ModLoader;

namespace NoxusBoss.Core;

public static class ModLoadingChecker
{
    private static readonly FieldInfo? isLoadingField = typeof(ModLoader).GetField("isLoading", UniversalBindingFlags);

    /// <summary>
    /// Whether mods are currently reloading or not.
    /// </summary>
    public static bool ModsReloading => (bool)(isLoadingField?.GetValue(null) ?? false);
}
