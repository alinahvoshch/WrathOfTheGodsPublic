using Microsoft.Xna.Framework;
using NoxusBoss.Core.Data;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering;

public static class NamelessDeityArmsRegistry
{
    /// <summary>
    /// The set of all Nameless Deity arms.
    /// </summary>
    internal static Dictionary<string, ArmRenderData> ArmLookup => LocalDataManager.Read<ArmRenderData>("Content/NPCs/Bosses/NamelessDeity/Rendering/NamelessDeityArmRenderData.json");

    /// <summary>
    /// A representation of Nameless' arm offset data.
    /// </summary>
    /// <param name="ForearmOrigin">The origin pivot point for the forearm.</param>
    /// <param name="OffsetFactor">The offset factor for the arm.</param>
    public record ArmRenderData(Vector2 ForearmOrigin, float OffsetFactor);
}
