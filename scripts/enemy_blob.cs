using Godot;
using System;
using System.Collections.Generic;

public partial class enemy_blob : CharacterBody3D
{
    [Export]
    public float Speed = 5.0f;

    // Components
    [Export]
    private HealthComponent _healthComponent;
    [Export]
    private HitboxComponent _hitboxComponent;
    [Export]
    private SensorAreaComponent _sensorAreaComponent;
    [Export]
    private float JumpForce = 14f;

    public Attack Attack1 = new Attack(10f, 0f);

    private AnimationPlayer _player;
    private MeshInstance3D _mesh;
    private Label3D _hpLabel;
    private Vector3 _direction;
    private Vector3 _prevGlobalPosition;
    private Timer _stuckTimer;

    public override void _Ready()
    {
        _mesh =  GetNode<MeshInstance3D>("MeshInstance3D");
        _player = GetNode<AnimationPlayer>("AnimationPlayer");
        _hpLabel = GetNode<Label3D>("%HPLabel");
        _direction = Vector3.Zero;
        _prevGlobalPosition = this.GlobalPosition;

        _stuckTimer = new Timer();
        AddChild(_stuckTimer);
        _stuckTimer.WaitTime = 1f;
        _stuckTimer.OneShot = true;
        _stuckTimer.Timeout += () => StuckTimerExpire();
    }

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _PhysicsProcess(double delta)
    {
        _hpLabel.Text = _healthComponent.Health.ToString();
        MoveTowardTarget();
    }

    public void MoveTowardTarget()
    {
        Vector3 velocity = Velocity;
        if (_direction != Vector3.Zero)
        {
            velocity.X = _direction.X * Speed;
            velocity.Y = _direction.Y * Speed;
            velocity.Z = _direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        float distanceTravelled = _prevGlobalPosition.DistanceTo(this.GlobalPosition);
        if (_direction != Vector3.Zero && distanceTravelled <= 0.001 && _stuckTimer.IsStopped())
        {
            _stuckTimer.Start();
        }
        _prevGlobalPosition = this.GlobalPosition;
        Velocity = velocity;
        MoveAndSlide();
    }

    public void StuckTimerExpire()
    {
        // try to jump over the obstacle
        Vector3 velocity = Velocity;
        velocity.Y = JumpForce;
        Velocity = velocity;
        MoveAndSlide();
    }

    public void Die()
    {
        GD.Print(this.Name.ToString() + ": has died!");
        _player.Play("die");
    }

    // Darkens the blob's color as it takes damage
    private void UpdateBlobColor()
    {
        float value = _healthComponent.Health / _healthComponent.MaxHealth;
        Material meshMaterial  = _mesh.GetSurfaceOverrideMaterial(0);
        if (meshMaterial is StandardMaterial3D)
        {
            StandardMaterial3D meshSMaterial = (StandardMaterial3D)meshMaterial;
            Color newColor = meshSMaterial.AlbedoColor;
            newColor.V = value / 2;
            meshSMaterial.AlbedoColor = newColor;
            _mesh.SetSurfaceOverrideMaterial(0, meshSMaterial);
        }
    }

    // signals ----------------------------------------------------------------
    public void _on_health_component_death_signal()
    {
        Die();
    }

    public void _on_health_component_took_damage()
    {
        UpdateBlobColor();
    }

    public void _on_hitbox_component_body_entered(Node3D body)
    {
        HitboxComponent hitbox = body.GetNodeOrNull<HitboxComponent>("%HitboxComponent");
        if (hitbox != null)
        {
            hitbox.Damage(Attack1);
        }
    }

    public void _on_sensor_area_component_update_direction(Vector3 newDirection)
    {
        _direction = newDirection;
    }

    public void _on_sensor_area_component_nav_target_reached()
    {
        _direction = Vector3.Zero;
    }
}
