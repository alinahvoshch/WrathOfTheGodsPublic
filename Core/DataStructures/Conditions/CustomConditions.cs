using NoxusBoss.Content.Items;
using NoxusBoss.Core.World.GameScenes.SolynEventHandlers;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;

namespace NoxusBoss.Core.DataStructures.Conditions;

/// <summary>
/// Represents a collection of conditions for use by things such as loot, town NPC shops, etc.
/// </summary>
public static class CustomConditions
{
    /// <summary>
    /// A condition that indicates whether Solyn's books are obtainable or not.
    /// </summary>
    public static readonly Condition BooksObtainable = new Condition("Mods.NoxusBoss.Conditions.BooksObtainable", () => SolynBooksSystem.BooksObtainable);

    /// <summary>
    /// A condition that indicates whether the current world was generated before the Avatar update or not.
    /// </summary>
    public static readonly Condition PreAvatarUpdateWorld = new Condition("Mods.NoxusBoss.Conditions.PreAvatarUpdateWorld", () => WorldVersionSystem.PreAvatarUpdateWorld);

    /// <summary>
    /// A condition that indicates whether Solyn has appeared in the world or not.
    /// </summary>
    public static readonly Condition SolynHasAppeared = new Condition("Mods.NoxusBoss.Conditions.AfterSolynAppearance", () => RandomSolynSpawnSystem.SolynHasAppearedBefore);
}
