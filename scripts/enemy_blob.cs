using Godot;
using System;
using System.Collections.Generic;

public partial class enemy_blob : CharacterBody3D
{
	[Export]
	public float Speed = 5.0f;
	[Export]
	public float MaxHp = 40f;
	public float CurrentHp;

	private Area3D _sensorArea;
	private Node3D _currentTarget;
    private List<Node3D> _targetList = new List<Node3D>();
	private AnimationPlayer _player;
    private NavigationAgent3D _navAgent;
    private RayCast3D _sightRay;

    public override void _Ready()
    {
		_sensorArea = GetNode<Area3D>("SensorArea");
		_currentTarget = null;
		CurrentHp = MaxHp;
		_player = GetNode<AnimationPlayer>("AnimationPlayer");
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        _sightRay = GetNode<RayCast3D>("SightRay");
    }

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// If we are targeting a player, and they haven't disconnected, nav toward them
		if (IsInstanceValid(_currentTarget) && _currentTarget != null)
        {
            if (IsTargetInLineOfSight(_currentTarget))
            {
                UpdateTargetLocation(_currentTarget);
            }
            else
            {
                UpdateCurrentTarget();
            }
        }
        Vector3 destination = _navAgent.GetNextPathPosition();
        Vector3 local_destination = destination - GlobalPosition;
        Vector3 direction = local_destination.Normalized();
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

	private void UpdateCurrentTarget()
	{
		// enumerate potential targets and get the cloest one
		_currentTarget = null;
		float closestDistance = float.MaxValue;

        if (_sensorArea.Monitoring)
        {
            foreach(Node3D target in _targetList)
            {
                float distanceFromTarget = this.GlobalPosition.DistanceTo(target.GlobalPosition);
                // Target the closest player that is in line of sight
                if (IsTargetInLineOfSight(target) && distanceFromTarget < closestDistance)
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

    private bool IsTargetInLineOfSight(Node3D target)
    {
        Vector3 local_destination = target.GlobalPosition - GlobalPosition;
        Vector3 direction = local_destination.Normalized();
        _sightRay.TargetPosition = direction;
        if (_sightRay.IsColliding())
        {
            GD.Print("Colliding!");
            //return true;
        }
        else
        {
            GD.Print("Not Colliding!");
            //return false;
        }
        return true;
    }

    private void UpdateTargetLocation(Node3D target)
    {
        _navAgent.TargetPosition = target.GlobalPosition;
    }

	public void applyDamage(float damage)
	{
		GD.Print("TAKING DAMAGE: " + damage.ToString());
		CurrentHp -= damage;
		if (CurrentHp <= 0)
		{
			Die();
		}
    }

	public void Die()
	{
		GD.Print(this.Name.ToString() + ": has died!");
		_player.Play("die");
	}

	// signals
	public void _on_sensor_range_body_entered(Node3D body)
	{
        if (body.IsInGroup("players"))
        {
            if (!_targetList.Contains(body))
            {
                _targetList.Add(body);
                UpdateCurrentTarget();
            }
		}
	}

	public void _on_sensor_area_body_exited(Node3D body)
	{
		if (body.IsInGroup("players"))
		{
            if (_targetList.Contains(body))
            {
                _targetList.Remove(body);
                UpdateCurrentTarget();
            }
		}
	}

    // used for debugging purposes
    private void PrintTargetList()
    {
        GD.Print("Current target list:");
        foreach (Node3D target in _targetList)
        {
            GD.Print($"{target.Name}");
        }
    }

    public void _on_hitbox_body_entered(Node3D body)
	{
		if (body.IsInGroup("players"))
		{
			GD.Print("You should get hurt here");
		}
	}

	public void _on_hitbox_area_entered(Area3D area)
	{
		if (area.IsInGroup("projectiles"))
		{
			float damage = 0f;
			switch(area.GetOwner<Node3D>().GetType().ToString())
			{
				case "bolt":
					bolt temp = new bolt();
					damage = temp.Damage;
					break;
				default:
					break;
			}
			applyDamage(damage);
		}
	}
	
}
