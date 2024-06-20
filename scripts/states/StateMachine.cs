using Godot;
using System;
using System.Collections.Generic;

// Reference taken from here: https://github.com/spaceyjase/sr-6/blob/main/StateMachine/StateMachine.cs

namespace StateMachine
{
    public partial class StateMachine : Node
    {
        private readonly Dictionary<string, State> States = new Dictionary<string, State>();
        private State CurrentState { get; set; }
        private State InitialState { get; set; }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            foreach (var child in GetChildren())
            {
                // Check to see if it's a state, set it to initial if there are no states presently
                if (!(child is State state)) continue;

                if (States.Count == 0)
                {
                    InitialState = state;
                }
                // add state
                States[state.Name] = state;
                state.StateMachine = this;
            }
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (States.Count == 0 || InitialState == null)
            {
                GD.PrintErr("State machine is empty! Check that all states are connected");
                return;
            }

            if (CurrentState == null)
            {
                ChangeState(InitialState);
            }

            CurrentState.OnProcess?.Invoke(delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            if (States.Count == 0 || InitialState == null)
            {
                GD.PrintErr("State machine is empty!");
                return;
            }

            if (CurrentState == null)
            {
                ChangeState(InitialState);
            }

            CurrentState.OnPhysicsProcess?.Invoke(delta);
        }
        
        public void ChangeState(State newState)
        {
            if (newState == null)
            {
                GD.PrintErr("Cannot transition to a new state!");
                return;
            }
            CurrentState?.OnExit?.Invoke();
            CurrentState = newState;
            newState.OnEnter?.Invoke();
        }

        public void ChangeState(string name)
        {
            if (States.ContainsKey(name) == false)
            {
                GD.PrintErr($"State {name} does not exist!");
                return;
            }
            ChangeState(States[name]);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            base._UnhandledInput(@event);
            CurrentState?.OnInput?.Invoke(@event);
        }

        public String GetStateName()
        {
            if (States.Count == 0 || InitialState == null)
            {
                return "State Machine has no state!";
            }
            return CurrentState.Name;
        }
    }
}

