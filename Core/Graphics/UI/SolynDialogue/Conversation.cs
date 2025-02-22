using Microsoft.Xna.Framework;
using NoxusBoss.Core.Netcode;
using NoxusBoss.Core.Netcode.Packets;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace NoxusBoss.Core.Graphics.UI.SolynDialogue;

public sealed class Conversation
{
    /// <summary>
    /// A condition which dictates whether this conversation should be rerolled in favor of another one by Solyn.
    /// </summary>
    internal Func<bool> RerollCondition = () => false;

    /// <summary>
    /// The entire tree of the conversation's dialogue.
    /// </summary>
    public readonly Dictionary<string, DialogueNode> DialogueTree = [];

    /// <summary>
    /// The condition that determines whether this conversation can be repeated or not.
    /// </summary>
    public readonly Func<bool> CanBeRepeatedCondition;

    /// <summary>
    /// The localization key of the conversation. This is used to determine if the conversation has been seen already or not.
    /// </summary>
    public readonly string IdentifierKey;

    /// <summary>
    /// The priority of the conversation. Used to determine which conversation should be selected, based on importance.
    /// </summary>
    public readonly ConversationPriority Priority;

    /// <summary>
    /// The requirement for the conversation to be available.
    /// </summary>
    public readonly Func<bool> AppearanceCondition;

    /// <summary>
    /// The function responsible for deciding which dialogue the conversation should start with.
    /// </summary>
    public readonly Func<string> DialogueStartSelector;

    /// <summary>
    /// The prefix for localization keys for conversations.
    /// </summary>
    public const string KeyPrefix = "Mods.NoxusBoss.Solyn.";

    internal Conversation(string identifierKey, Func<string> dialogueStartSelector, ConversationPriority priority, Func<bool>? appearanceCondition = null, Func<bool>? repeatCondition = null)
    {
        AppearanceCondition = appearanceCondition ?? (() => true);
        IdentifierKey = identifierKey;
        DialogueStartSelector = dialogueStartSelector;
        Priority = priority;
        CanBeRepeatedCondition = repeatCondition ?? (() => false);
    }

    /// <summary>
    /// Registers a reroll condition for this dialogue instance.
    /// </summary>
    public Conversation WithRerollCondition(Func<bool> rerollCondition)
    {
        RerollCondition = rerollCondition;
        return this;
    }

    /// <summary>
    /// Whether a this conversation has been seen before.
    /// </summary>
    public bool HasBeenSeen() => ConversationDataSaveSystem.seenConversations.Contains(IdentifierKey);

    /// <summary>
    /// Determines whether a given player has seen a certain dialogue type.
    /// </summary>
    /// <param name="p">The player to check the conversation status of.</param>
    /// <param name="identifierKey">The identifier key of the dialogue to check for.</param>
    public bool NodeSeen(string identifierKey) =>
        ConversationDataSaveSystem.seenDialogue.Contains($"{KeyPrefix}{IdentifierKey}.{identifierKey}");

    /// <summary>
    /// Registers a conversation as having been seen.
    /// </summary>
    public void RegisterAsSeen()
    {
        if (!ConversationDataSaveSystem.seenConversations.Contains(IdentifierKey))
            ConversationDataSaveSystem.seenConversations.Add(IdentifierKey);
        if (Main.netMode == NetmodeID.MultiplayerClient)
            PacketManager.SendPacket<SeenConversationPacket>(IdentifierKey);
    }

    /// <summary>
    /// Starts the conversation.
    /// </summary>
    public void Start() => StartFromNode(DialogueStartSelector());

    /// <summary>
    /// Starts a conversation with Solyn based on the desired node identification key.
    /// </summary>
    /// <param name="nodeIdentifierKey">The node's identifier.</param>
    public void StartFromNode(string nodeIdentifierKey)
    {
        if (!DialogueTree.TryGetValue(nodeIdentifierKey, out DialogueNode? dialogue))
            return;

        RegisterAsSeen();
        SolynDialogUI.ConversationStartingNode = dialogue;
    }

    /// <summary>
    /// Generates a simple <see cref="DialogueNode"/> based on certain text attributes.
    /// </summary>
    /// <param name="textKey">The localization key that is associated with the dialogue.</param>
    /// <param name="spokenByPlayer">Whether the dialogue is something the player says.</param>
    /// <param name="condition">The appearance condition for the dialogue. Defaults to always being true.</param>
    /// <param name="colorOverrideFunction">An optional function that allows for the overriding of the text color.</param>
    public DialogueNode CreateFromKey(string textKey, bool spokenByPlayer, Func<bool>? condition = null, Func<Color>? colorOverrideFunction = null)
    {
        DialogueNode node = new DialogueNode($"{KeyPrefix}{IdentifierKey}.{textKey}", spokenByPlayer, condition, colorOverrideFunction);
        DialogueTree[textKey] = node;
        return node;
    }

    /// <summary>
    /// Generates a simple <see cref="DialogueNode"/> based on certain text attributes.
    /// </summary>
    /// <param name="textSelector">The function responsible for selecting dialogue localization text.</param>
    /// <param name="textKey">The localization key that is associated with the dialogue.</param>
    /// <param name="spokenByPlayer">Whether the dialogue is something the player says.</param>
    /// <param name="condition">The appearance condition for the dialogue. Defaults to always being true.</param>
    /// <param name="colorOverrideFunction">An optional function that allows for the overriding of the text color.</param>
    public DialogueNode CreateFromKey(Func<LocalizedText> textSelector, string textKey, bool spokenByPlayer, Func<bool>? condition = null, Func<Color>? colorOverrideFunction = null)
    {
        DialogueNode node = new DialogueNode(textSelector, $"{KeyPrefix}{IdentifierKey}.{textKey}", spokenByPlayer, condition, colorOverrideFunction);
        DialogueTree[textKey] = node;
        return node;
    }
}
