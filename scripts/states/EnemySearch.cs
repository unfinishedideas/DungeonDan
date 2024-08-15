using Godot;
using System;
using StateMachine;

public partial class EnemySearch : EnemyState
{
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
        StateMachine?.ChangeState(STATE_CHASE);
    }

    private void TargetDetected()
    {
        StateMachine?.ChangeState(STATE_CHASE);
    }

    private void TookDamage()
    {
        StateMachine?.ChangeState(STATE_DAMAGED);
    }

    public void NavTargetReachedHandler()
    {
        _direction = Vector3.Zero;
        StateMachine?.ChangeState(STATE_IDLE);
    }
}

