using Godot;
using System;
using StateMachine;

namespace Enemy.States
{
    public partial class EnemyIdle : EnemyState
    {
        // States
        [Export]
        private EnemyState ChasingState;

        public override void _Ready()
        {
            OnProcess += Process;
        }
        
        private void Process(double delta)
        {
            //GD.Print("Enemy idling!!!");
        }

        public void _on_sensor_area_component_target_acquired()
        {
            GD.Print("Signal: target acquired");
            StateMachine?.ChangeState(ChasingState);
        }
    }
}

