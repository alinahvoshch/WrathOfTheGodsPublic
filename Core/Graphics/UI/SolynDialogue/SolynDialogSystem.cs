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
