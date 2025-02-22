namespace NoxusBoss.Content.NPCs.Bosses.Draedon.DraedonDialogue;

/// <summary>
/// Represents an instance of dialogue spoken between Solyn and Draedon.
/// </summary>
/// <param name="LocalizationKey">The localization key associated with this dialogue.</param>
/// <param name="TimeStartDelay">How long it takes for this dialogue to appear.</param>
/// <param name="Lifetime">How long this dialogue should exist for in the world.</param>
/// <param name="SpokenBySolyn">Whether this dialogue is spoken by Solyn or not.</param>
public record DialogueInstance(string LocalizationKey, int TimeStartDelay, int Lifetime, bool SpokenBySolyn);
