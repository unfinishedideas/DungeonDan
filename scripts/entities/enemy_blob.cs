using Godot;
using System;
using System.Collections.Generic;

public partial class enemy_blob : Enemy
{
    [Export]
    public MeshInstance3D _mesh;

    private AnimationPlayer _player;
    private Timer _stuckTimer;

    public override void _Ready()
    {
        _player = GetNode<AnimationPlayer>("AnimationPlayer");

        _stuckTimer = new Timer();
        AddChild(_stuckTimer);
        _stuckTimer.WaitTime = 1f;
        _stuckTimer.OneShot = true;
        _stuckTimer.Timeout += () => StuckTimerExpire();
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

    // try to jump over small obstacles
    public void StuckTimerExpire()
    {
        Vector3 velocity = Velocity;
        velocity.Y = JumpForce;
        Velocity = velocity;
        MoveAndSlide();
    }

    /*
    // Bounce back when hitting an enemy
    private void BounceBack()
    {
    Vector3 backDirection = new Vector3(_direction.X * -1, _direction.Y, _direction.Z * -1);

    Vector3 velocity = Velocity;
    velocity.X = backDirection.X * BounceBackForce;
    velocity.Z = backDirection.Z * BounceBackForce;
    Velocity = velocity;

    MoveAndSlide();
    }
    */

    // signals ----------------------------------------------------------------
    public void _on_health_component_took_damage()
    {
        UpdateBlobColor();
    }
}
