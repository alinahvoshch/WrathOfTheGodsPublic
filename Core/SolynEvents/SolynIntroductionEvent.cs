using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.DialogueSystem;

namespace NoxusBoss.Core.SolynEvents;

public class SolynIntroductionEvent : SolynEvent
{
    public override int TotalStages => 1;

    public override void OnModLoad()
    {
        DialogueManager.RegisterNew("SolynIntroduction", "Start").
            LinkFromStartToFinishExcluding("Repeat").
            WithAppearanceCondition(instance => !instance.Tree.HasBeenSeenBefore).
            MakeSpokenByPlayer("Question1", "Question2", "Question3").
            WithRerollCondition(_ => Finished).
            WithRootSelectionFunction(instance =>
            {
                if (instance.SeenBefore("Talk8"))
                    return instance.GetByRelativeKey("Repeat");

                return instance.GetByRelativeKey("Start");
            });

        ConversationSelector.PriorityConversationSelectionEvent += SelectIntroductionDialogue;
    }

    private Conversation? SelectIntroductionDialogue()
    {
        if (!Finished)
            return DialogueManager.FindByRelativePrefix("SolynIntroduction");

        return null;
    }

    public override void PostUpdateNPCs()
    {
        // Register this event as completed once Solyn has gone to sleep for the night.
        if (Solyn is not null && Solyn.CurrentState == SolynAIType.Eepy)
            Stage = 1;
    }
}
