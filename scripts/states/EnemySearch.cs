using Godot;
using System;
using StateMachine;

public partial class EnemySearch : EnemyState
{
    [Export]
    protected EnemyState ChasingState;
    [Export]
    protected EnemyState IdleState;
    [Export]
    protected EnemyState EnemyDamageState;

    private SensorAreaComponent _sensorArea;
    private HealthComponent _healthComponent;

    public override void _Ready()
    {
        base._Ready();
        _sensorArea = Owner.GetNode<SensorAreaComponent>("%SensorAreaComponent"); 
        _healthComponent = Owner.GetNode<HealthComponent>("%HealthComponent"); 
        OnPhysicsProcess += PhysicsProcess;
        OnEnter += Enter;
        OnExit += Exit;
    }

    private void PhysicsProcess(double delta)
    {
        MoveTowardTarget(delta);
    }

    private void Enter()
    {
        _sensorArea.TargetAcquired += TargetAcquired;
        _sensorArea.NavTargetReached += NavTargetReachedHandler;
        _sensorArea.UpdateDirection += UpdateDirectionHandler;
        _healthComponent.TookDamage += TookDamage;
    }

    private void Exit()
    {
        _sensorArea.TargetAcquired -= TargetAcquired;
        _sensorArea.NavTargetReached -= NavTargetReachedHandler;
        _sensorArea.UpdateDirection -= UpdateDirectionHandler;
        _healthComponent.TookDamage -= TookDamage;
    }

    private void TargetAcquired()
    {
        StateMachine?.ChangeState(ChasingState);
    }

    private void TargetDetected()
    {
        StateMachine?.ChangeState(ChasingState);
    }

    private void TookDamage()
    {
        StateMachine?.ChangeState(EnemyDamageState);
    }

    public void NavTargetReachedHandler()
    {
        _direction = Vector3.Zero;
        StateMachine?.ChangeState(IdleState);
    }
}

