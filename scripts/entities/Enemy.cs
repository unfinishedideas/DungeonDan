using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy : CharacterBody3D
{
    [Export]
    public float Speed = 5.0f;

    // Components
    [Export]
    public HealthComponent _healthComponent;
    [Export]
    public Label3D _hpLabel;
    [Export]
    public Label3D _stateLabel;

    [Export]
    public float JumpForce = 10f;
    [Export]
    public float BounceBackForce = 200f;
    [Export]
    public float MeleeAttackDamage = 10f;
    [Export]
    public float MeleeAttackKnockback = 0f;

    public Attack Attack1;

    private MeshInstance3D _mesh;
    private StateMachine.StateMachine _stateMachine;

    public override void _Ready()
    {
        _mesh =  GetNode<MeshInstance3D>("MeshInstance3D");
        _hpLabel = GetNode<Label3D>("%HPLabel");
        _stateLabel = GetNode<Label3D>("%StateLabel");
        _stateMachine = GetNode<StateMachine.StateMachine>("%StateMachine");
        Attack1 = new Attack(MeleeAttackDamage, MeleeAttackKnockback);
    }

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Process(double  delta)
    {
        base._Process(delta);
        _stateLabel.Text = _stateMachine.GetStateName();
        _hpLabel.Text = _healthComponent.Health.ToString();
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
