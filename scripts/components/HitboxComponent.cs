using Godot;
using System;
using System.Collections.Generic;

public partial class HitboxComponent : Area3D
{
    [Export]
    public float Damage = 10f;
    [Export]
    public float Knockback = 0f;

    public Attack HitboxAttack;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        HitboxAttack = new Attack(Damage, Knockback);
        CollisionLayer = 2;
        CollisionMask = 0;
	}
}

