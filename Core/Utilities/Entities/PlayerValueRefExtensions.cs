using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.GlobalInstances;
using Terraria;

namespace NoxusBoss.Core.Utilities;

public static partial class Utilities
{
    public static Referenced<T> GetValueRef<T>(this Player player, string key) where T : new()
    {
        if (player.TryGetModPlayer(out PlayerDataManager modPlayer))
            return modPlayer.valueRegistry.GetValueRef<T>(key);

        return new(() => new T(), _ => { });
    }

    public static Referenced<T> GetValueRef<T>(this PlayerDataManager player, string key) =>
        player.valueRegistry.GetValueRef<T>(key);
}
