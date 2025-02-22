using Newtonsoft.Json;
using NoxusBoss.Core.CrossCompatibility.Inbound;
using Terraria;

namespace NoxusBoss.Core.DataStructures;

// I would ordinarily make this all readonly, but that causes problems with JSON deserialization.

/// <summary>
/// Represents a value that should vary based on the four main-line modes: Normal, Expert, Revengeance, and Death.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public struct DifficultyValue<TValue> where TValue : struct
{
    /// <summary>
    /// The value that should be selected in Normal mode.
    /// </summary>
    public TValue NormalValue;

    /// <summary>
    /// The value that should be selected in Expert mode.
    /// </summary>
    public TValue ExpertValue;

    /// <summary>
    /// The value that should be selected in Revengeance mode.
    /// </summary>
    public TValue RevengeanceValue;

    /// <summary>
    /// The value that should be selected in Death mode.
    /// </summary>
    public TValue DeathValue;

    /// <summary>
    /// The value that should be selected in the Get fixed boi secret seed.
    /// </summary>
    public TValue GFBValue;

    /// <summary>
    /// The value that should be selected.
    /// </summary>
    [JsonIgnore]
    public readonly TValue Value
    {
        get
        {
            if (Main.zenithWorld)
                return GFBValue;
            if (CommonCalamityVariables.DeathModeActive)
                return DeathValue;
            if (CommonCalamityVariables.RevengeanceModeActive)
                return RevengeanceValue;
            if (Main.expertMode)
                return ExpertValue;

            return NormalValue;
        }
    }

    public DifficultyValue(TValue universal)
    {
        NormalValue = universal;
        ExpertValue = universal;
        RevengeanceValue = universal;
        DeathValue = universal;
        GFBValue = universal;
    }

    public DifficultyValue(TValue normal, TValue expertAndUp)
    {
        NormalValue = normal;
        ExpertValue = expertAndUp;
        RevengeanceValue = expertAndUp;
        DeathValue = expertAndUp;
        GFBValue = expertAndUp;
    }

    public DifficultyValue(TValue normal, TValue expert, TValue revengeanceAndUp)
    {
        NormalValue = normal;
        ExpertValue = expert;
        RevengeanceValue = revengeanceAndUp;
        DeathValue = revengeanceAndUp;
        GFBValue = revengeanceAndUp;
    }

    public DifficultyValue(TValue normal, TValue expert, TValue revengeance, TValue deathAndUp)
    {
        NormalValue = normal;
        ExpertValue = expert;
        RevengeanceValue = revengeance;
        DeathValue = deathAndUp;
        GFBValue = deathAndUp;
    }

    public DifficultyValue(TValue normal, TValue expert, TValue revengeance, TValue death, TValue gfb)
    {
        NormalValue = normal;
        ExpertValue = expert;
        RevengeanceValue = revengeance;
        DeathValue = death;
        GFBValue = gfb;
    }

    public static implicit operator TValue(DifficultyValue<TValue> difficultyValue) => difficultyValue.Value;
}
