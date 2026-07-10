// ============================================================================
// StateMachineTemplate.cs
// Purpose: Complete finite state machine implementation template
// Dependencies: IState interface from Core
// Unity Version: 6000.3.18f1
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameName.Core.Patterns
{
    /// <summary>
    /// Defines the contract for a state in the finite state machine.
    /// </summary>
    public interface IState
    {
        /// <summary>Called once when entering this state.</summary>
        void Enter();

        /// <summary>Called every frame while in this state. Use for input and non-physics logic.</summary>
        void Tick();

        /// <summary>Called at fixed intervals while in this state. Use for physics logic.</summary>
        void FixedTick();

        /// <summary>Called once when leaving this state.</summary>
        void Exit();
    }

    /// <summary>
    /// Generic finite state machine with conditional transitions.
    /// Supports both specific-state transitions and any-state transitions.
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> Manages complex entity behavior through discrete states
    /// with automatic transition evaluation.</para>
    /// <para><b>Usage:</b></para>
    /// <code>
    /// // In your controller's Awake:
    /// _stateMachine = new StateMachine();
    /// var idle = new IdleState(this);
    /// var run = new RunState(this);
    /// var jump = new JumpState(this);
    ///
    /// // Add transitions
    /// _stateMachine.AddTransition(idle, run, () => _moveInput.magnitude > 0.1f);
    /// _stateMachine.AddTransition(run, idle, () => _moveInput.magnitude < 0.1f);
    /// _stateMachine.AddAnyTransition(jump, () => _jumpRequested &amp;&amp; _isGrounded);
    ///
    /// // Set initial state
    /// _stateMachine.SetState(idle);
    ///
    /// // In Update: _stateMachine.Tick();
    /// // In FixedUpdate: _stateMachine.FixedTick();
    /// </code>
    /// <para><b>Performance:</b> Transition evaluation is O(n) where n is the number
    /// of transitions for the current state plus any-state transitions.
    /// No GC allocations during Tick/FixedTick.</para>
    /// </remarks>
    public class StateMachine
    {
        #region Private Fields

        private IState _currentState;
        private readonly Dictionary<Type, List<Transition>> _transitions = new();
        private readonly List<Transition> _anyTransitions = new();
        private List<Transition> _currentTransitions = new();
        private static readonly List<Transition> EmptyTransitions = new(0);

        #endregion

        #region Properties

        /// <summary>Gets the currently active state.</summary>
        public IState CurrentState => _currentState;

        /// <summary>Gets the type of the currently active state.</summary>
        public Type CurrentStateType => _currentState?.GetType();

        #endregion

        #region Events

        /// <summary>Raised when a state transition occurs. Parameters: (previousState, newState).</summary>
        public event Action<IState, IState> OnStateChanged;

        #endregion

        #region Public Methods

        /// <summary>
        /// Evaluates transitions and ticks the current state.
        /// Call from MonoBehaviour.Update().
        /// </summary>
        public void Tick()
        {
            Transition triggeredTransition = GetTriggeredTransition();
            if (triggeredTransition != null)
            {
                SetState(triggeredTransition.To);
            }

            _currentState?.Tick();
        }

        /// <summary>
        /// Fixed-ticks the current state for physics operations.
        /// Call from MonoBehaviour.FixedUpdate().
        /// </summary>
        public void FixedTick()
        {
            _currentState?.FixedTick();
        }

        /// <summary>
        /// Forces a state change, calling Exit on the current state and Enter on the new state.
        /// </summary>
        /// <param name="state">The state to transition to.</param>
        public void SetState(IState state)
        {
            if (state == null)
            {
                Debug.LogError("[StateMachine] Cannot set null state.");
                return;
            }

            if (_currentState == state) return;

            IState previousState = _currentState;
            _currentState?.Exit();
            _currentState = state;

            // Load transitions for the new state
            _transitions.TryGetValue(_currentState.GetType(), out var transitions);
            _currentTransitions = transitions ?? EmptyTransitions;

            _currentState.Enter();
            OnStateChanged?.Invoke(previousState, _currentState);
        }

        /// <summary>
        /// Adds a conditional transition from one state to another.
        /// Evaluated every Tick when the 'from' state is active.
        /// </summary>
        /// <param name="from">The source state.</param>
        /// <param name="to">The destination state.</param>
        /// <param name="condition">The condition that triggers the transition.</param>
        public void AddTransition(IState from, IState to, Func<bool> condition)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            if (!_transitions.TryGetValue(from.GetType(), out var transitions))
            {
                transitions = new List<Transition>();
                _transitions[from.GetType()] = transitions;
            }

            transitions.Add(new Transition(to, condition));
        }

        /// <summary>
        /// Adds a transition that can trigger from ANY state.
        /// Evaluated every Tick regardless of current state.
        /// Use sparingly — checked before state-specific transitions.
        /// </summary>
        /// <param name="to">The destination state.</param>
        /// <param name="condition">The condition that triggers the transition.</param>
        public void AddAnyTransition(IState to, Func<bool> condition)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            _anyTransitions.Add(new Transition(to, condition));
        }

        #endregion

        #region Private Methods

        private Transition GetTriggeredTransition()
        {
            // Check any-state transitions first (higher priority)
            for (int i = 0; i < _anyTransitions.Count; i++)
            {
                if (_anyTransitions[i].Condition())
                {
                    return _anyTransitions[i];
                }
            }

            // Check current-state-specific transitions
            for (int i = 0; i < _currentTransitions.Count; i++)
            {
                if (_currentTransitions[i].Condition())
                {
                    return _currentTransitions[i];
                }
            }

            return null;
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Represents a conditional state transition.
        /// </summary>
        private sealed class Transition
        {
            /// <summary>Gets the target state of this transition.</summary>
            public IState To { get; }

            /// <summary>Gets the condition function that triggers this transition.</summary>
            public Func<bool> Condition { get; }

            public Transition(IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }

        #endregion
    }
}
