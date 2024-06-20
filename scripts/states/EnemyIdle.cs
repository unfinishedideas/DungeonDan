using Godot;
using System;
using StateMachine;

// Signals to connect
// _on_sensor_area_component_target_acquired

public partial class EnemyIdle : EnemyState
{
    [Export]
    protected EnemyState ChasingState;

    public override void _Ready()
    {
        OnProcess += Process;
    }

    private void Process(double delta){}

    public void _on_sensor_area_component_target_acquired()
    {
        StateMachine?.ChangeState(ChasingState);
    }
}
