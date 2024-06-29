using Godot;
using System;
using StateMachine;

public partial class EnemyChase : EnemyState
{
    [Export]
    protected EnemyState SearchState;
    [Export]
    protected EnemyState EnemyDamageState;
    [Export]
    protected Node3D EnemyMesh;
    [Export]
    protected EnemyState AttackState;

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

    private void Enter()
    {
        _sensorArea.TargetLost += TargetLost;
        _sensorArea.UpdateDirection += UpdateDirectionHandler;
        _sensorArea.NavTargetReached += NavTargetReachedHandler;
        _healthComponent.TookDamage += TookDamage;
    }

    private void Exit()
    {
        _sensorArea.TargetLost -= TargetLost;
        _sensorArea.UpdateDirection -= UpdateDirectionHandler;
        _sensorArea.NavTargetReached -= NavTargetReachedHandler;
        _healthComponent.TookDamage -= TookDamage;
    }

    private void TargetLost()
    {
        StateMachine?.ChangeState(SearchState);
    }

    private void TookDamage()
    {
        StateMachine?.ChangeState(EnemyDamageState);
    }

    private void PhysicsProcess(double delta)
    {
        MoveTowardTarget(delta);
    }

    public void NavTargetReachedHandler()
    {
        _direction = Vector3.Zero;
        StateMachine?.ChangeState(AttackState);
    }
}
