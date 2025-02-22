using Luminance.Common.StateMachines;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon;

public partial class MarsBody : ModNPC
{
    /// <summary>
    /// Backing field for Mars' state machine.
    /// </summary>
    private PushdownAutomata<EntityAIState<MarsAIType>, MarsAIType> stateMachine;

    /// <summary>
    /// Mars' current state.
    /// </summary>
    public MarsAIType CurrentState => StateMachine.CurrentState.Identifier;

    /// <summary>
    /// The AI timer for Mars' current state.
    /// </summary>
    /// <remarks>
    /// Notably, <i>AI timers are local to a given state</i>.
    /// </remarks>
    public ref int AITimer => ref StateMachine.CurrentState.Time;

    /// <summary>
    /// Mars' state machine. This is responsible for all handling of his AI, such as behavior management, AI timers, etc.
    /// </summary>
    public PushdownAutomata<EntityAIState<MarsAIType>, MarsAIType> StateMachine
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
    /// Loads Mars' state machine, registering transition requirements and actions pertaining to it.
    /// </summary>
    public void LoadStateMachine()
    {
        // Initialize the AI state machine.
        stateMachine = new(new(MarsAIType.SpawnAnimation));
        stateMachine.OnStateTransition += PrepareForNextState;

        // Register all Mars states in the machine.
        for (int i = 0; i < (int)MarsAIType.Count; i++)
            stateMachine.RegisterState(new((MarsAIType)i));

        // Load state transitions.
        AutomatedMethodInvokeAttribute.InvokeWithAttribute(this);
    }

    /// <summary>
    /// Prepares for the upcoming state via resetting AI state variables, opacity, targeting, etc.
    /// </summary>
    /// <param name="stateWasPopped">Whether the previous state was popped from the pushdown stack (aka is finished, rather than something to continue later).</param>
    /// <param name="oldState">The old state.</param>
    public void PrepareForNextState(bool stateWasPopped, EntityAIState<MarsAIType> oldState)
    {
        bool preserveState = oldState?.Identifier == MarsAIType.BrutalBarrageGrindForcefield && CurrentState == MarsAIType.BrutalBarrage;
        if (stateWasPopped && !preserveState)
        {
            NPC.ai[0] = 0f;
            NPC.ai[1] = 0f;
            NPC.ai[2] = 0f;
            NPC.ai[3] = 0f;
        }

        NPC.Opacity = 1f;
        NPC.TargetClosest();
        NPC.netUpdate = true;
    }
}
