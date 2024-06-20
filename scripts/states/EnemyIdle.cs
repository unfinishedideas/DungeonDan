using Godot;
using System;
using StateMachine;

// Signals to connect
// _on_sensor_area_component_target_acquired

public partial class EnemyIdle : EnemyState
{
    [Export]
    protected EnemyState ChasingState;
    private SensorAreaComponent _sensorArea;

    public override void _Ready()
    {
        OnProcess += Process;
        OnEnter += Enter;
        OnExit += Exit;
       _sensorArea = Owner.GetNode<SensorAreaComponent>("%SensorAreaComponent"); 
    }

    private void Process(double delta){}

    private void Enter()
    {
       _sensorArea.TargetAcquired += TargetDetected;
    }

    private void Exit()
    {
        _sensorArea.TargetAcquired -= TargetDetected;
    }

    private void TargetDetected()
    {
        StateMachine?.ChangeState(ChasingState);
    }
}
