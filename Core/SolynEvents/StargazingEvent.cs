using Luminance.Core.Cutscenes;
using Luminance.Core.Graphics;
using NoxusBoss.Content.Items;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Content.Tiles.TileEntities;
using NoxusBoss.Core.DialogueSystem;
using NoxusBoss.Core.World.GameScenes.Stargazing;
using NoxusBoss.Core.World.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Core.SolynEvents;

public class StargazingEvent : SolynEvent
{
    public override int TotalStages => 3;

    public override void OnModLoad()
    {
        // Part 1.
        DialogueManager.RegisterNew("StargazeQuest", "Solyn1").
            LinkFromStartToFinishExcluding("Repeat").
            WithAppearanceCondition(instance => ModContent.GetInstance<SolynIntroductionEvent>().Finished).
            MakeSpokenByPlayer("Player1", "Player2").
            WithRerollCondition(_ => Stage >= 1).
            WithRootSelectionFunction(instance =>
            {
                if (instance.SeenBefore("Solyn3"))
                    return instance.GetByRelativeKey("Repeat");

                return instance.GetByRelativeKey("Solyn1");
            });

        // Part 2.
        DialogueManager.RegisterNew("StargazeQuestCompletion", "Response1").
            WithAppearanceCondition(instance => Stage == 1).
            WithRerollCondition(_ => Stage >= 2);
        DialogueManager.FindByRelativePrefix("StargazeQuestCompletion").GetByRelativeKey("Response1").EndAction += seenBefore =>
        {
            BlockerSystem.Start(true, false, () => !Finished);
            Main.LocalPlayer.SetTalkNPC(-1);

            CutsceneManager.QueueCutscene(ModContent.GetInstance<BecomeDuskScene>());
            BecomeDuskScene.EndAction = () =>
            {
                CutsceneManager.QueueCutscene(ModContent.GetInstance<UseTelescopeScene>());
            };
        };

        // Part 3.
        DialogueManager.RegisterNew("StargazeQuestSawRift", "Talk1").
            LinkFromStartToFinish().
            MakeSpokenByPlayer("Response1", "Response2").
            WithAppearanceCondition(instance => Stage == 2).
            WithRerollCondition(_ => Finished);
        DialogueManager.FindByRelativePrefix("StargazeQuestSawRift").GetByRelativeKey("Talk8").ClickAction += seenBefore =>
        {
            if (!seenBefore)
                Main.LocalPlayer.QuickSpawnItem(new EntitySource_WorldEvent(), ModContent.ItemType<StarCharm>());
        };
        DialogueManager.FindByRelativePrefix("StargazeQuestSawRift").GetByRelativeKey("Talk10").EndAction += seenBefore =>
        {
            SafeSetStage(3);
            Solyn?.SwitchState(SolynAIType.StandStill);
        };

        ConversationSelector.PriorityConversationSelectionEvent += SelectStargazingDialogue;
    }

    public override void PostUpdateNPCs()
    {
        // Check if the telescope got repaired anywhere in the world. If it did, advance the quest.
        // This exists for backwards compatibility with old worlds.
        if (Stage == 0 && !SubworldSystem.IsActive<EternalGardenNew>())
        {
            foreach (TileEntity te in TileEntity.ByID.Values)
            {
                if (te is TESolynTelescope telescope && telescope.IsRepaired)
                {
                    SafeSetStage(1);
                    break;
                }
            }
        }

        if (Stage >= 1 && !Finished && Solyn is not null)
        {
            if (Solyn.CurrentState != SolynAIType.PuppeteeredByQuest)
                Solyn.SwitchState(SolynAIType.PuppeteeredByQuest);
            Solyn.PerformStandardFraming();
            Solyn.NPC.spriteDirection = -1;
            Solyn.NPC.velocity.X = 0f;
        }
    }

    private Conversation? SelectStargazingDialogue()
    {
        if (SubworldSystem.IsActive<EternalGardenNew>())
            return null;

        // Wait for the introduction event to conclude before enabling this event.
        if (!ModContent.GetInstance<SolynIntroductionEvent>().Finished)
            return null;

        if (!Finished)
        {
            if (Stage == 0)
                return DialogueManager.FindByRelativePrefix("StargazeQuest");
            if (Stage == 1)
                return DialogueManager.FindByRelativePrefix("StargazeQuestCompletion");
            if (Stage == 2)
                return DialogueManager.FindByRelativePrefix("StargazeQuestSawRift");
        }

        return null;
    }
}
