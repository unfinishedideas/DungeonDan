using Godot;
using System;
using StateMachine;

public partial class EnemyChase : EnemyState
{
    [Export]
    protected EnemyState SearchState;
    [Export]
    protected EnemyState AttackState;

    private SensorAreaComponent _sensorArea;
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
        OnPhysicsProcess += PhysicsProcess;
        OnEnter += Enter;
        OnExit += Exit;
    }

    private void Enter()
    {
        _sensorArea.TargetLost += TargetLost;
        _sensorArea.UpdateDirection += UpdateDirection;
        _sensorArea.NavTargetReached += NavTargetReached;
    }

    private void Exit()
    {
        _sensorArea.TargetLost -= TargetLost;
        _sensorArea.UpdateDirection -= UpdateDirection;
        _sensorArea.NavTargetReached -= NavTargetReached;
    }

    private void TargetLost()
    {
        StateMachine?.ChangeState(SearchState);
    }

    private void UpdateDirection(Vector3 direction)
    {
        _direction = direction;
    }

    private void NavTargetReached()
    {
        _direction = Vector3.Zero;
        StateMachine?.ChangeState(AttackState);
    }

    private void PhysicsProcess(double delta)
    {
        MoveTowardTarget(delta);
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
