using Godot;
using System;
using StateMachine;

// Signals to connect
// _on_sensor_area_component_target_lost()
// _on_sensor_area_component_update_direction(Vector3 Direction)
// _on_sensor_area_component_nav_target_reached()

public partial class EnemyChase : EnemyState
{
    [Export]
    protected EnemyState SearchState;
    [Export]
    protected EnemyState AttackState;

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
    public void _on_sensor_area_component_target_lost()
    {
        StateMachine?.ChangeState(SearchState);
    }

    public void _on_sensor_area_component_update_direction(Vector3 Direction)
    {
        _direction = Direction;
    }

    public void _on_sensor_area_component_nav_target_reached()
    {
        GD.Print("EnemyChase Nav target reached");
        _direction = Vector3.Zero;
        StateMachine?.ChangeState(AttackState);
    }
}
