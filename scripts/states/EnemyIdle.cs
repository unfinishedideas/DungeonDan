using Godot;
using System;
using StateMachine;

public partial class EnemyIdle : EnemyState
{
    [Export]
    protected EnemyState ChasingState;
    [Export]
    protected EnemyState EnemyDamageState;
    [Export]
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
        StateMachine?.ChangeState(ChasingState);
    }

    private void TookDamage()
    {
        StateMachine?.ChangeState(EnemyDamageState);
    }
}

