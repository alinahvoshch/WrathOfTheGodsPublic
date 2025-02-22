using System.Reflection;
using NoxusBoss.Core.CrossCompatibility.Inbound;

namespace NoxusBoss.Core.Utilities;

public static partial class Utilities
{
    private static FieldInfo? calConfigInstanceField;

    /// <summary>
    /// Useful way of acquiring information from Calamity's config without a strong reference.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName">The name of the config value. Should correspond to the property name in Calamity's source code. If it doesn't exist for some reason the default value will be used.</param>
    /// <param name="defaultValue">The default value to use if the config data could not be accessed. Normally is the default for whatever data type is requested (such as zero for integers), but can be manually specified.</param>
    public static T GetFromCalamityConfig<T>(string propertyName, T defaultValue = default) where T : struct
    {
        // Immediately return the default value if Calamity is not enabled.
        if (ModReferences.Calamity is null)
            return defaultValue;

        // Immediately return the default value if Calamity's config doesn't exist for some reason.
        Type? calConfigType = ModReferences.Calamity.Code.GetType("CalamityMod.CalamityConfig") ?? ModReferences.Calamity.Code.GetType("CalamityMod.CalamityClientConfig");
        if (calConfigType is null)
            return defaultValue;

        // Use reflection to access the property's data. If this fails, return the default value.
        calConfigInstanceField ??= calConfigType.GetField("Instance");
        object? calConfig = calConfigInstanceField?.GetValue(null) ?? null;
        if (calConfig is null)
            return defaultValue;

        PropertyInfo? property = calConfig?.GetType()?.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)(property?.GetValue(calConfig) ?? defaultValue!);
    }
}
