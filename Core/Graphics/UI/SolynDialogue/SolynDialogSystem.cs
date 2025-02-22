using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Friendly;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace NoxusBoss.Core.Graphics.UI.SolynDialogue;

public class SolynDialogSystem : ModSystem
{
    private UserInterface dialogUserInterface;

    /// <summary>
    /// The UI responsible for drawing dialogue and player responses to said dialogue.
    /// </summary>
    public SolynDialogUI DialogUI
    {
        get;
        internal set;
    }

    public override void Load()
    {
        dialogUserInterface = new UserInterface();

        // Initialize the underlying UI state.
        DialogUI = new SolynDialogUI();
        DialogUI.Activate();
    }

    public override void UpdateUI(GameTime gameTime)
    {
        // Disable the UI if the speaker is not present.
        if (dialogUserInterface.CurrentState is not null && !NPC.AnyNPCs(ModContent.NPCType<Solyn>()))
            HideUI();

        // Update the UI.
        if (dialogUserInterface?.CurrentState is not null)
            dialogUserInterface?.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        // Draw the Solyn dialogue UI.
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text", StringComparison.Ordinal));
        if (mouseTextIndex != -1)
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("Wrath of the Gods: Solyn Dialogue", DrawUIWrapper, InterfaceScaleType.None));
    }

    private bool DrawUIWrapper()
    {
        if (dialogUserInterface?.CurrentState is not null)
            dialogUserInterface.Draw(Main.spriteBatch, new GameTime());
        return true;
    }

    /// <summary>
    /// Chooses a random Solyn conversation to use.
    /// </summary>
    public static Conversation ChooseSolynConversation()
    {
        // Collect the initial list of possible conversations, and gradually whittle down from there, starting with manual condition criteria.
        var possibleConversations = SolynDialogRegistry.SolynConversations.Where(c => c.AppearanceCondition() && (!c.HasBeenSeen() || c.CanBeRepeatedCondition())).ToList();
        if (possibleConversations.Count == 0)
            return SolynDialogRegistry.SolynErrorDialogue;

        // Order by conversation priority.
        possibleConversations = possibleConversations.OrderByDescending(c => c.Priority).ToList();

        // Determine the highest found priority in the conversation list, and prune all list items that do not have that highest priority value.
        ConversationPriority highestPriority = possibleConversations.First().Priority;
        possibleConversations = possibleConversations.Where(c => c.Priority >= highestPriority).ToList();

        // Error case: If no start-able conversation could be found for some reason, display a special conversation that indicates that an error occurred.
        if (possibleConversations.Count == 0)
            return SolynDialogRegistry.SolynErrorDialogue;

        // Pick randomly from the remaining assortment of possible conversations.
        return Main.rand.Next(possibleConversations.ToList());
    }

    /// <summary>
    /// Rerolls Solyn's current conversation.
    /// </summary>
    public static void RerollConversationForSolyn()
    {
        int solynID = ModContent.NPCType<Solyn>();
        foreach (NPC solyn in Main.ActiveNPCs)
        {
            if (solyn.type != solynID)
                continue;

            solyn.As<Solyn>().CurrentConversation = ChooseSolynConversation();
            solyn.As<Solyn>().CurrentConversation.Start();
        }
    }

    /// <summary>
    /// Changes Solyn's current conversation.
    /// </summary>
    public static void ForceChangeConversationForSolyn(Conversation conversation)
    {
        int solynID = ModContent.NPCType<Solyn>();
        foreach (NPC solyn in Main.ActiveNPCs)
        {
            if (solyn.type != solynID)
                continue;

            solyn.As<Solyn>().CurrentConversation = conversation;
            solyn.As<Solyn>().CurrentConversation.Start();
        }
    }

    /// <summary>
    /// Shows the dialogue UI.
    /// </summary>
    public static void ShowUI()
    {
        var dialogSystem = ModContent.GetInstance<SolynDialogSystem>();
        if (dialogSystem.dialogUserInterface.CurrentState is not null)
            return;

        dialogSystem.DialogUI ??= new SolynDialogUI();
        dialogSystem.DialogUI.ResetUIElements();
        dialogSystem.DialogUI.Activate();
        dialogSystem.DialogUI.DialogueText = string.Empty;
        dialogSystem.DialogUI.DialogueTextUI.SetText(string.Empty);
        dialogSystem.DialogUI.PlayerTextUI.SetText(string.Empty);

        var ui = dialogSystem.dialogUserInterface;
        var uiState = dialogSystem.DialogUI;
        ui?.SetState(uiState);
    }

    /// <summary>
    /// Hides the dialogue UI.
    /// </summary>
    public static void HideUI()
    {
        var dialogSystem = ModContent.GetInstance<SolynDialogSystem>();
        if (dialogSystem.dialogUserInterface.CurrentState is null)
            return;

        dialogSystem.dialogUserInterface?.SetState(null);
        dialogSystem.DialogUI.CurrentDialogueNode = null;
        dialogSystem.DialogUI.DialogueText = string.Empty;
        dialogSystem.DialogUI.ResponseToSay = null;
    }
}
