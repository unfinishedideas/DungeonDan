using Godot;
using System;
using System.Collections.Generic;

public partial class HitboxComponent : Area3D
{
    [Export]
    public float Damage = 10f;
    [Export]
    public float Knockback = 0f;

    // How long to keep the hitbox active
    [Export]
    public float AttackTime = 0.25f;
    // Determines if hitbox is "active" to deal damage, leave default unless debugging
    [Export]
    public bool Attacking {get; set;} = false;

    [Signal]
    public delegate void DetectedHurtboxEventHandler();

    public Attack HitboxAttack;


    private Timer _attackTimer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        HitboxAttack = new Attack(Damage, Knockback);
        _attackTimer = new Timer();
        AddChild(_attackTimer);
        _attackTimer.WaitTime = AttackTime;
        _attackTimer.OneShot = true;
        _attackTimer.Timeout += () => AttackReset();
        CollisionLayer = 2;
        CollisionMask = 4;
	}

    public override void _Process(double delta)
    {
        if (Attacking == true)
        {
            DealDamage();
        }
    }

    private void AttackReset()
    {
        Attacking = false;
    }

    public void Attack()
    {
        Attacking = true;
        _attackTimer.Start();
    }

    public void DealDamage()
    {
        var areas = GetOverlappingAreas();
        foreach (Area3D area in areas)
        {
            if (area is HurtboxComponent hurtbox && !Owner.IsAncestorOf(hurtbox))
            {
                hurtbox.Damage(HitboxAttack);
            }
        }
    }

    public void _on_area_entered(Area3D area)
    {
        EmitSignal(SignalName.DetectedHurtbox);
    }
}

