using Godot;
using System;
using StateMachine;

// Signals to connect
// _on_sensor_area_component_target_acquired
// _on_sensor_area_component_nav_target_reached()

public partial class EnemySearch : EnemyState
{
    [Export]
    protected EnemyState ChasingState;
    [Export]
    protected EnemyState IdleState;

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
        OnPhysicsProcess += PhysicsProcess;
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

    // Signals ----------------------------------------------------------------
    public void _on_sensor_area_component_target_acquired()
    {
        StateMachine?.ChangeState(ChasingState);
    }

    public void _on_sensor_area_component_nav_target_reached()
    {
        GD.Print("Enemy Search nav reached");
        StateMachine?.ChangeState(IdleState);
    }

    public void _on_sensor_area_component_update_direction(Vector3 Direction)
    {
        _direction = Direction;
    }
}
