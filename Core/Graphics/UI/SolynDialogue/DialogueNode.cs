using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace NoxusBoss.Core.Graphics.UI.SolynDialogue;

public sealed class DialogueNode
{
    internal bool EndActionHappened;

    /// <summary>
    /// The text of the dialog. This is an internal <see cref="LocalizedText"/> to ensure that the string can update on the fly when debugging.
    /// </summary>
    internal Func<LocalizedText> ResponseTextInternal
    {
        get;
        private set;
    }

    /// <summary>
    /// The localization key of the dialogue. This is used to determine if a given dialogue node has been seen already or not.
    /// </summary>
    public string IdentifierKey
    {
        get;
        private set;
    }

    /// <summary>
    /// Whether the dialog is spoken by the player or not.
    /// </summary>
    public bool SpokenByPlayer
    {
        get;
        private set;
    }

    /// <summary>
    /// The condition required for the dialog to appear.
    /// </summary>
    public Func<bool> Condition
    {
        get;
        private set;
    }

    /// <summary>
    /// The action to execute once this dialog has ended.
    /// </summary>
    public Action EndAction
    {
        get;
        private set;
    }

    /// <summary>
    /// The action to execute once this dialog is clicked away.
    /// </summary>
    public Action ClickAwayAction
    {
        get;
        private set;
    }

    /// <summary>
    /// The text of the dialog response.
    /// </summary>
    public string ResponseText => ResponseTextInternal().Value;

    /// <summary>
    /// The parent node of this node.
    /// </summary>
    public DialogueNode Parent
    {
        get;
        private set;
    }

    /// <summary>
    /// A function that may be used to override the text color for this node.
    /// </summary>
    public Func<Color>? ColorOverrideFunction
    {
        get;
        private set;
    }

    /// <summary>
    /// The children of this node.
    /// </summary>
    public List<DialogueNode> Children
    {
        get;
        private set;
    } = [];

    public DialogueNode(string textKey, bool spokenByPlayer = false, Func<bool>? condition = null, Func<Color>? colorOverrideFunction = null)
    {
        IdentifierKey = textKey;
        ResponseTextInternal = () => Language.GetText(IdentifierKey);
        SpokenByPlayer = spokenByPlayer;
        ClickAwayAction = () => SolynDialogRegistry.RegisterDialogueAsSeen(this);
        EndAction = () => SolynDialogRegistry.RegisterDialogueAsSeen(this);
        Condition = condition ?? (() => true);
        ColorOverrideFunction = colorOverrideFunction;
    }

    public DialogueNode(Func<LocalizedText> textSelector, string identifierKey, bool spokenByPlayer = false, Func<bool>? condition = null, Func<Color>? colorOverrideFunction = null)
    {
        IdentifierKey = identifierKey;
        ResponseTextInternal = textSelector;
        SpokenByPlayer = spokenByPlayer;
        EndAction = () => SolynDialogRegistry.RegisterDialogueAsSeen(this);
        Condition = condition ?? (() => true);
        ColorOverrideFunction = colorOverrideFunction;
    }

    /// <summary>
    /// Configures this node with a desired end action.
    /// </summary>
    /// <param name="endAction">The end action.</param>
    public DialogueNode AddEndAction(Action endAction)
    {
        EndAction += endAction;
        return this;
    }

    /// <summary>
    /// Configures this node with a desired click-away action.
    /// </summary>
    /// <param name="clickAwayAction">The end action.</param>
    public DialogueNode AddClickAwayAction(Action clickAwayAction)
    {
        ClickAwayAction += clickAwayAction;
        return this;
    }

    /// <summary>
    /// Adds child nodes.
    /// </summary>
    /// <param name="children">The child nodes to add.</param>
    public void AddChildren(params DialogueNode[] children)
    {
        for (int i = 0; i < children.Length; i++)
            children[i].Parent = this;

        Children.AddRange(children);
    }

    /// <summary>
    /// Invokes this node's end action.
    /// </summary>
    public void InvokeEndAction()
    {
        if (EndActionHappened)
            return;

        EndAction?.Invoke();
        EndActionHappened = true;
    }
}
