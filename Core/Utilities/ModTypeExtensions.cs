using Terraria.ModLoader;

namespace NoxusBoss.Core.Utilities;

public static partial class Utilities
{
    /// <summary>
    /// Returns the directory for a given <see cref="ModType"/> relative to its mod.
    /// </summary>
    /// <param name="m">The mod type to return the directory of.</param>
    public static string GetModRelativeDirectory(this ModType m)
    {
        string modPrefix = $"{m.Mod.Name}/";
        string? typeName = m.GetType().FullName;

        return typeName is null
            ? throw new NullReferenceException($"Could not retrieve the FullName of ModType '{m}'.")
            : $"{typeName.Replace(".", "/").Replace(modPrefix, string.Empty)}";
    }

    /// <summary>
    /// Return a shorthand path for a given texture content prefix and name.
    /// </summary>
    public static string GetAssetPath(string prefix, string name) =>
        $"NoxusBoss/Assets/Textures/{prefix}/{name}";
}
