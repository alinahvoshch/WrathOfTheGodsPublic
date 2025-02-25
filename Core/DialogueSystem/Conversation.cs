namespace NoxusBoss.Core.DialogueSystem;

public class Conversation
{
    /// <summary>
    /// The function which dictates whether this conversation can be selected or not.
    /// </summary>
    public Func<bool> AppearanceCondition
    {
        get;
        set;
    } = () => true;

    /// <summary>
    /// The function which dictates whether this conversation should be rerolled if in use or not.
    /// </summary>
    public Func<bool> RerollCondition
    {
        get;
        set;
    } = () => false;

    /// <summary>
    /// The function which dictates which dialogue instanced should be selected as the root node.
    /// </summary>
    public Func<Dialogue> RootSelectionFunction
    {
        get;
        set;
    }

    /// <summary>
    /// The dialogue tree associated with this conversation.
    /// </summary>
    public readonly DialogueTree Tree;

    public Conversation(string localizationPrefix, string rootNodeKey)
    {
        Tree = new DialogueTree(localizationPrefix, rootNodeKey);
        RootSelectionFunction = () => Tree.Root;
    }

    /// <summary>
    /// Determines whether a given snippet of dialogue in the conversation with a given relative key has been seen before or not.
    /// </summary>
    public bool SeenBefore(string relativeKey) => GetByRelativeKey(relativeKey).SeenBefore;

    /// <summary>
    /// Gets a given dialogue instance based on a relative key.
    /// </summary>
    public Dialogue GetByRelativeKey(string key) => Tree.GetByRelativeKey(key);

    /// <summary>
    /// Links chains of dialogue together.
    /// </summary>
    public void LinkChain(params string[] identifiers) => Tree.LinkChain(identifiers);

    /// <summary>
    /// A convenient function that links all dialogue in the tree based on their position in the dictionary (aka the order they're loaded in the JSON).
    /// </summary>
    public Conversation LinkFromStartToFinish()
    {
        string[] orderedChain = Tree.PossibleDialogue.Select(d => d.Key).ToArray();
        Tree.LinkChain(orderedChain);
        return this;
    }

    /// <summary>
    /// A convenient function that links all dialogue in the tree based on their position in the dictionary (aka the order they're loaded in the JSON), excluding a given set of keys.
    /// </summary>
    public Conversation LinkFromStartToFinishExcluding(params string[] keysToExclude)
    {
        string[] orderedChain = Tree.PossibleDialogue.Select(d => d.Key).Where(k => !keysToExclude.Contains(k)).ToArray();
        Tree.LinkChain(orderedChain);
        return this;
    }

    /// <summary>
    /// A convenient method that allows a user to select a given root selection function and get the instance back, for method chaining purposes.
    /// </summary>
    public Conversation WithRootSelectionFunction(Func<Conversation, Dialogue> function)
    {
        RootSelectionFunction = () => function(this);
        return this;
    }

    /// <summary>
    /// A convenient method that allows a user to select a given root selection function and get the instance back, for method chaining purposes.
    /// </summary>
    public Conversation MakeSpokenByPlayer(params string[] relativeKeys)
    {
        foreach (string key in relativeKeys)
            GetByRelativeKey(key).SpokenByPlayer = true;

        return this;
    }

    /// <summary>
    /// A convenient method that allows a user to select a given appearance condition function and get the instance back, for method chaining purposes.
    /// </summary>
    public Conversation WithAppearanceCondition(Func<Conversation, bool> condition)
    {
        AppearanceCondition = () => condition(this);
        return this;
    }

    /// <summary>
    /// A convenient method that allows a user to select a given reroll condition function and get the instance back, for method chaining purposes.
    /// </summary>
    public Conversation WithRerollCondition(Func<Conversation, bool> condition)
    {
        RerollCondition = () => condition(this);
        return this;
    }
}
