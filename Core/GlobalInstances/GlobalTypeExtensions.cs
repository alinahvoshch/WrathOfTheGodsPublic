using Terraria;

namespace NoxusBoss.Core.GlobalInstances;

public static class GlobalTypeExtensions
{
    /// <summary>
    /// Returns the global item instance for a given item.
    /// </summary>
    /// <param name="item"></param>
    public static InstancedGlobalItem Wrath(this Item item) =>
        item.GetGlobalItem<InstancedGlobalItem>();
}
