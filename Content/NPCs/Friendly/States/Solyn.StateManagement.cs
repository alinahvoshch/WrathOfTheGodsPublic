using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Friendly;

public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    /// <summary>
    /// Backing field for Solyn's state machine.
    /// </summary>
    private PushdownAutomata<EntityAIState<SolynAIType>, SolynAIType> stateMachine;

    /// <summary>
    /// Solyn's state machine. This is responsible for all handling of Solyn's AI, such as behavior management, AI timers, etc.
    /// </summary>
    public PushdownAutomata<EntityAIState<SolynAIType>, SolynAIType> StateMachine
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
    /// The current AI state Solyn is using. This uses the <see cref="StateMachine"/> under the hood.
    /// </summary>
    public SolynAIType CurrentState
    {
        get
        {
            return StateMachine?.CurrentState?.Identifier ?? SolynAIType.WanderAbout;
        }
    }

    /// <summary>
    /// The AI timer for Solyn's current state.
    /// </summary>
    /// <remarks>
    /// Notably, <i>AI timers are local to a given state</i>.
    /// </remarks>
    public ref int AITimer => ref StateMachine.CurrentState.Time;

    /// <summary>
    /// Loads Solyn's state machine, registering transition requirements and actions pertaining to it.
    /// </summary>
    public void LoadStateMachine()
    {
        // Initialize the AI state machine.
        StateMachine = new(new(SolynAIType.StandStill));
        StateMachine.OnStateTransition += PrepareForNextState;

        // Register all Solyn states in the machine.
        for (int i = 0; i < (int)SolynAIType.Count; i++)
            StateMachine.RegisterState(new((SolynAIType)i));

        // Load state transitions.
        AutomatedMethodInvokeAttribute.InvokeWithAttribute(this);
    }

    /// <summary>
    /// Ensures the safety of Solyn's state stack, adding a fallback state if necessary.
    /// </summary>
    public void PerformStateSafetyCheck()
    {
        if (StateMachine.StateStack.Count <= 0)
            StateMachine.StateStack.Push(StateMachine.StateRegistry[SolynAIType.StandStill]);
    }

    /// <summary>
    /// Prepares for the upcoming state via resetting AI state variables and opacity if necessary.
    /// </summary>
    /// <param name="stateWasPopped">Whether the previous state was popped from the pushdown stack (aka is finished, rather than something to continue later).</param>
    /// <param name="oldState">The old state.</param>
    public void PrepareForNextState(bool stateWasPopped, EntityAIState<SolynAIType> oldState)
    {
        NPC.Opacity = 1f;
        NPC.netUpdate = true;
    }
}
