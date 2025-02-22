using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness : ModNPC
{
    /// <summary>
    /// Backing field for the Avatar's state machine.
    /// </summary>
    private PushdownAutomata<EntityAIState<AvatarAIType>, AvatarAIType> stateMachine;

    /// <summary>
    /// The previous attack the Avatar performed.
    /// </summary>
    public AvatarAIType PreviousState
    {
        get;
        set;
    }

    /// <summary>
    /// The previous attack pattern the Avatar performed.
    /// </summary>
    public List<AvatarAIType> PreviousPattern
    {
        get;
        set;
    }

    /// <summary>
    /// The set of all states that the Avatar should not perform a teleport at the start of if in the background.
    /// </summary>
    public List<AvatarAIType> StatesToNotStartTeleportDuring
    {
        get;
        private set;
    } = [];

    /// <summary>
    /// The lookup table that determines which dimension attack states map to.
    /// </summary>
    public static Dictionary<AvatarAIType, AvatarDimensionVariant> AttackDimensionRelationship
    {
        get;
        private set;
    } = [];

    /// <summary>
    /// The AI timer for the Avatar's current state.
    /// </summary>
    /// <remarks>
    /// Notably, <i>AI timers are local to a given state</i>.
    /// </remarks>
    public ref int AITimer => ref StateMachine.CurrentState.Time;

    /// <summary>
    /// the Avatar's state machine. This is responsible for all handling of the Avatar's AI, such as behavior management, AI timers, etc.
    /// </summary>
    public PushdownAutomata<EntityAIState<AvatarAIType>, AvatarAIType> StateMachine
    {
        get
        {
            if (stateMachine is null)
                LoadStateMachine();
            return stateMachine!;
        }
        private set => stateMachine = value;
    }

    /// <summary>
    /// Loads the Avatar's state machine, registering transition requirements and actions pertaining to it.
    /// </summary>
    public void LoadStateMachine()
    {
        StatesToNotStartTeleportDuring.Clear();
        AttackDimensionRelationship.Clear();

        StatesToNotStartTeleportDuring.Add(AvatarAIType.ResetCycle);

        // Initialize the AI state machine.
        StateMachine = new(new(AvatarAIType.Awaken_RiftSizeIncrease));
        StateMachine.OnStatePop += StoreOldState;
        StateMachine.OnStateTransition += PrepareForNextState;

        // Register all Avatar states in the machine.
        for (int i = 0; i < (int)AvatarAIType.Count; i++)
            StateMachine.RegisterState(new((AvatarAIType)i));

        // Load state transitions.
        AutomatedMethodInvokeAttribute.InvokeWithAttribute(this);
    }

    /// <summary>
    /// Records the Avatar's previous state.
    /// </summary>
    /// <param name="previousState">The previous state.</param>
    public void StoreOldState(EntityAIState<AvatarAIType> previousState)
    {
        if (previousState.Identifier != AvatarAIType.ResetCycle && previousState.Identifier != AvatarAIType.Teleport && previousState.Identifier != AvatarAIType.TeleportAbovePlayer)
            PreviousState = previousState.Identifier;
    }

    /// <summary>
    /// Prepares for the upcoming state via resetting AI state variables, opacity, and entering the foreground again if necessary.
    /// </summary>
    /// <param name="stateWasPopped">Whether the previous state was popped from the pushdown stack (aka is finished, rather than something to continue later).</param>
    /// <param name="oldState">The old state.</param>
    public void PrepareForNextState(bool stateWasPopped, EntityAIState<AvatarAIType> oldState)
    {
        // Reset generic AI variables and the Z position if the state was popped.
        if (stateWasPopped)
        {
            // If the Avatar was in the background and isn't translucent/invisible prepare to enter the foreground via a teleport.
            if (ZPosition != 0f && NPC.Opacity >= 0.5f && !StatesToNotStartTeleportDuring.Contains(CurrentState) && oldState.Identifier != AvatarAIType.SendPlayerToMyUniverse && oldState.Identifier != AvatarAIType.AbsoluteZeroOutburstPunishment)
            {
                StartTeleportAnimation(() =>
                {
                    ZPosition = 0f;
                    return Target.Center - Vector2.UnitY * 350f;
                });
            }
        }

        // Reset opacity.
        NPC.Opacity = 1f;

        TargetClosest();
        NPC.netUpdate = true;
    }
}
