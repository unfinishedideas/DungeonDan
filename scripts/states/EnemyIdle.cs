using Godot;
using System;
using StateMachine;

public partial class EnemyIdle : EnemyState
{
    public string IdleAnimName;

    private SensorAreaComponent _sensorArea;
    private HealthComponent _healthComponent;

    public override void _Ready()
    {
        OnProcess += Process;
        OnEnter += Enter;
        OnExit += Exit;
        _sensorArea = Owner.GetNode<SensorAreaComponent>("%SensorAreaComponent"); 
        _healthComponent = Owner.GetNode<HealthComponent>("%HealthComponent"); 
    }

    private void Process(double delta){}

    private void Enter()
    {
        _sensorArea.TargetAcquired += TargetDetected;
        _healthComponent.TookDamage += TookDamage;
        _sensorArea.ClearCurrentTarget();
        AnimPlayer?.Play(IdleAnimName);
    }

    private void Exit()
    {
        _sensorArea.TargetAcquired -= TargetDetected;
        _healthComponent.TookDamage -= TookDamage;
    }

    private void TargetDetected()
    {
        StateMachine?.ChangeState(STATE_CHASE);
    }

    private void TookDamage()
    {
        StateMachine?.ChangeState(STATE_DAMAGED);
    }
}

