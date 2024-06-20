using Godot;
using System;
using StateMachine;

// Signals to connect
// _on_sensor_area_component_target_acquired

public partial class EnemyIdle : EnemyState
{
    //protected Enemy _enemy;

    [Export]
    private EnemyState ChasingState;

    public override void _Ready()
    {
        //_enemy = Owner as Enemy;
        OnProcess += Process;
    }

    private void Process(double delta){}

    public void _on_sensor_area_component_target_acquired()
    {
        GD.Print("Signal: target acquired");
        StateMachine?.ChangeState(ChasingState);
    }
}
