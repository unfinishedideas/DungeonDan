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

    private Vector3 _direction;
    private Vector3 _prevGlobalPosition;
    private Vector3 _originalGlobalPosition;
    public float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
    {
        base._Ready();
        _direction = Vector3.Zero;
        _prevGlobalPosition = _enemy.GlobalPosition;
        _originalGlobalPosition = _enemy.GlobalPosition;
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
        _sensorArea.NavTargetReached += NavTargetReached;
        _sensorArea.UpdateDirection += UpdateDirection;
        _healthComponent.TookDamage += TookDamage;
    }

    private void Exit()
    {
        _sensorArea.TargetAcquired -= TargetAcquired;
        _sensorArea.NavTargetReached -= NavTargetReached;
        _sensorArea.UpdateDirection -= UpdateDirection;
        _healthComponent.TookDamage -= TookDamage;
    }

    private void TargetAcquired()
    {
        StateMachine?.ChangeState(ChasingState);
    }

    private void NavTargetReached()
    {
        StateMachine?.ChangeState(IdleState);
    }

    private void UpdateDirection(Vector3 direction)
    {
        _direction = direction;
    }
    private void TargetDetected()
    {
        StateMachine?.ChangeState(ChasingState);
    }

    private void TookDamage()
    {
        StateMachine?.ChangeState(EnemyDamageState);
    }

    public void MoveTowardTarget(double delta)
    {
        Vector3 velocity = _enemy.Velocity;
        if (_direction != Vector3.Zero)
        {
            velocity.X = _direction.X * _enemy.Speed;
            velocity.Z = _direction.Z * _enemy.Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(_enemy.Velocity.X, 0, _enemy.Speed);
            velocity.Z = Mathf.MoveToward(_enemy.Velocity.Z, 0, _enemy.Speed);
        }
        _prevGlobalPosition = _enemy.GlobalPosition;
        velocity.Y -= Gravity * (float)delta;
        _enemy.Velocity = velocity;
        _enemy.MoveAndSlide();
    }
}

