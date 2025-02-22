using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers.SolynDialogue;
using Terraria;
using Terraria.Audio;
using Terraria.Localization;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.DraedonDialogue;

public class DialogueChain
{
    private readonly List<DialogueInstance> instances = [];

    /// <summary>
    /// How much of a pausing period there is between dialogue switching.
    /// </summary>
    public int DelayBetweenDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// The overall duration of the dialogue.
    /// </summary>
    public int Duration
    {
        get
        {
            if (instances.Count <= 0)
                return 0;

            DialogueInstance instance = instances.Last();
            return instance.TimeStartDelay + instance.Lifetime + DelayBetweenDialogue;
        }
    }

    public DialogueChain(int delayBetweenDialogue) => DelayBetweenDialogue = delayBetweenDialogue;

    /// <summary>
    /// Chains a new dialogue instance.
    /// </summary>
    /// <param name="localizationKey">The localization key associated with this dialogue.</param>
    /// <param name="lifetime">How long this dialogue should exist for in the world.</param>
    /// <param name="spokenBySolyn">Whether this dialogue is spoken by Solyn or not.</param>
    public DialogueChain Add(string localizationKey, int lifetime, bool spokenBySolyn)
    {
        int timeStartDelay = instances.Count >= 1 ? instances.Last().TimeStartDelay + instances.Last().Lifetime + DelayBetweenDialogue : 0;
        instances.Add(new(localizationKey, timeStartDelay, lifetime, spokenBySolyn));
        return this;
    }

    /// <summary>
    /// Chains a new dialogue instance with automatic timing.
    /// </summary>
    /// <param name="localizationKey">The localization key associated with this dialogue.</param>
    /// <param name="spokenBySolyn">Whether this dialogue is spoken by Solyn or not.</param>
    public DialogueChain AddWithAutomaticTiming(string localizationKey, bool spokenBySolyn)
    {
        int lifetime = (int)(Language.GetTextValue(localizationKey).Length * 2.3f) + 85;
        int timeStartDelay = instances.Count >= 1 ? instances.Last().TimeStartDelay + instances.Last().Lifetime + DelayBetweenDialogue : 0;
        instances.Add(new(localizationKey, timeStartDelay, lifetime, spokenBySolyn));
        return this;
    }

    /// <summary>
    /// Updates this dialogue chain.
    /// </summary>
    /// <param name="time">The current time value.</param>
    /// <param name="solyn">Solyn's NPC instance.</param>
    /// <param name="draedon">Draedon's NPC instance.</param>
    public void Update(int time, NPC? solyn, NPC draedon)
    {
        foreach (DialogueInstance instance in instances)
        {
            if (instance.SpokenBySolyn && solyn is null)
                continue;

            if (time == instance.TimeStartDelay)
            {
                NPC speaker = (instance.SpokenBySolyn ? solyn : draedon)!;
                Vector2 dialoguePosition = speaker.Top - Vector2.UnitY * 20f;
                if (instance.SpokenBySolyn)
                    SolynWorldDialogueManager.CreateNew(instance.LocalizationKey, speaker.spriteDirection, dialoguePosition - Vector2.UnitX * speaker.spriteDirection * 130f, instance.Lifetime, false);
                else
                    DraedonWorldDialogueManager.CreateNew(instance.LocalizationKey, speaker.spriteDirection, dialoguePosition - Vector2.UnitX * speaker.spriteDirection * 80f, instance.Lifetime);
            }

            if (time >= instance.TimeStartDelay && time <= instance.TimeStartDelay + 40 && time % 3 == 2 && instance.SpokenBySolyn && solyn is not null)
                SoundEngine.PlaySound(GennedAssets.Sounds.Solyn.Speak with { Volume = 0.4f, MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }, solyn.Center);
        }
    }
}
