using Godot;
using System;

namespace StateMachine
{
    public partial class State : Node
    {
        public Action<double> OnProcess { get; set; }
        public Action<double> OnPhysicsProcess { get; set; }
        public Action OnEnter { get; set; }
        public Action OnExit { get; set; }
        public Action<InputEvent> OnInput { get; set; }

        public StateMachine StateMachine { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

