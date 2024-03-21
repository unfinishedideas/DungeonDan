using Godot;
using System;

public partial class enemy_blob : CharacterBody3D
{
	[Export]
	public const float Speed = 1.0f;
	[Export]
	public float MaxHp = 40f;
	public float CurrentHp;

	private Area3D _sensorArea;
	private Node3D _currentTarget;

    public override void _Ready()
    {
		_sensorArea = GetNode<Area3D>("SensorArea");
		_currentTarget = null;
		CurrentHp = MaxHp;
    }

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity. Nah :)
		//if (!IsOnFloor())
		//	velocity.Y -= gravity * (float)delta;

		// If we are targeting a player, and they haven't disconnected, move toward them
		if (IsInstanceValid(_currentTarget) && _currentTarget != null)
		{
			Vector3 direction = _currentTarget.GlobalPosition - this.GlobalPosition;
			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * Speed;
				velocity.Y = direction.Y * Speed;
				velocity.Z = direction.Z * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
				velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
			}

			Velocity = velocity;
			MoveAndSlide();
		}
    }

	private void determineTarget()
	{
		// enumerate potential targets and get the cloest one
		_currentTarget = null;
		float closestDistance = float.MaxValue;
        Godot.Collections.Array<Node3D> targets = _sensorArea.GetOverlappingBodies();
		
		foreach (var target in targets)
		{
			if (target.IsInGroup("Players"))
			{
				float distanceFromTarget = this.GlobalPosition.DistanceTo(target.GlobalPosition);
				if (distanceFromTarget < closestDistance)
				{
					closestDistance = distanceFromTarget;
					_currentTarget = target;
				}
            }
		}
		if (_currentTarget == null) 
		{
			GD.Print(this.Name.ToString() + ": Target lost");
		}
		else
		{
			GD.Print(this.Name.ToString() + ": targeting: " + _currentTarget.Name.ToString());
		}
    }

	public void takeDamage(Area3D area)
	{
		GD.Print("TAKING DAMAGE");
		//damage = body.
		float damage = 10f;
		CurrentHp -= damage;
		if (CurrentHp <= 0)
		{
			Die();
		}
    }

	public void Die()
	{
		GD.Print(this.Name.ToString() + ": has died!");
	}

	// signals
	public void _on_sensor_range_body_entered(Node3D body)
	{
        if (body.IsInGroup("Players"))
        {
			GD.Print(this.Name.ToString() + ": FOUND A PLAYER: " + body.Name.ToString());
			determineTarget();
		}
	}

	public void _on_sensor_area_body_exited(Node3D body)
	{
		if (body.IsInGroup("Players"))
		{
            GD.Print(this.Name.ToString() + ": FOUND A PLAYER: " + body.Name.ToString());
            determineTarget();
		}
	}

    public void _on_hitbox_body_entered(Node3D body)
	{
		if (body.IsInGroup("Players"))
		{
			GD.Print("You should get hurt here");
		}
	}

	public void _on_hitbox_area_entered(Area3D area)
	{

		if (area.IsInGroup("Projectiles"))
		{
			takeDamage(area);
		}
	}
}
