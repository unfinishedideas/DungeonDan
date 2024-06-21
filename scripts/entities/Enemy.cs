using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy : CharacterBody3D
{
    // Components
    [Export]
    private StateMachine.StateMachine _stateMachine;
    [Export]
    public HealthComponent _healthComponent;
    [Export]
    public Label3D _hpLabel;
    [Export]
    public Label3D _stateLabel;

    [Export]
    public float Speed = 5.0f;
    [Export]
    public float JumpForce = 10f;
    [Export]
    public float BounceBackForce = 200f;
    [Export]
    public float MeleeAttackDamage = 10f;
    [Export]
    public float MeleeAttackKnockback = 0f;

    public Attack Attack1;

    public override void _Ready()
    {
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
}
