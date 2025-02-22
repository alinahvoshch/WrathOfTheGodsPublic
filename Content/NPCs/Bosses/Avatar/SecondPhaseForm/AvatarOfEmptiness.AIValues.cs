using System.Runtime.CompilerServices;
using NoxusBoss.Core.Data;
using NoxusBoss.Core.DataStructures;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    public const string AIValuesPath = $"Content/NPCs/Bosses/Avatar/AvatarOfEmptinessAIValues.json";

    /// <summary>
    /// Retrives a stored AI integer value with a given name.
    /// </summary>
    /// <param name="name">The value's named key.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetAIInt(string name) => (int)Round(GetAIFloat(name));

    /// <summary>
    /// Retrives a stored AI floating point value with a given name.
    /// </summary>
    /// <param name="name">The value's named key.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetAIFloat(string name) => LocalDataManager.Read<DifficultyValue<float>>(AIValuesPath)[name];
}
