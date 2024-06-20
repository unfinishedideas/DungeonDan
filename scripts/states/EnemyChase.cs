using Godot;
using System;
using StateMachine;

namespace Enemy.States
{
    public partial class EnemyChase : EnemyState
    {
        // States
        [Export]
        private EnemyState IdleState;

        public override void _Ready()
        {
            OnProcess += Process;
        }
        
        private void Process(double delta)
        {
            //GD.Print("Enemy chasing!!!");
        }

        public void _on_sensor_area_component_target_lost()
        {
            GD.Print("Signal: target lost");
            StateMachine?.ChangeState(IdleState);
        }
    }
}

