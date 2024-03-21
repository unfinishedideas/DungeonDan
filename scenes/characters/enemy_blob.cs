using Godot;
using System;

public partial class enemy_blob : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	private Area3D _sensorArea;
	private Node3D _currentTarget;
	private bool targeting = false;

    public override void _Ready()
    {
		_sensorArea = GetNode<Area3D>("SensorArea");
    }

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void determineTarget()
	{
		// enumerate potential targets and get the cloest one
		Node3D closestTarget = new Node3D();
		float closest_distance = float.MaxValue;
		Godot.Collections.Array<Node3D> targets = _sensorArea.GetOverlappingBodies();
		targets.Remove(this);
		if (targets.Count > 0)
		{
			foreach (var target in targets) {
				if (target != this && !isTargetWorld(target))
				{
					float current_distance = this.GlobalPosition.DistanceTo(target.GlobalPosition);
					if (current_distance < closest_distance)
					{
						targeting = true;
						_currentTarget = target;
						closestTarget = target;
					}
				}
			}
			GD.Print(this.Name.ToString() + ": Targeting: " + closestTarget.Name.ToString());
		}
		else
		{
			targeting = false;
			_currentTarget = null;
			GD.Print(this.Name.ToString() + ": No longer targeting");
		}
	}

	// signals
	public void _on_sensor_range_body_entered(Node3D body)
	{
		if (body != this && !isTargetWorld(this))
		{
			GD.Print(this.Name.ToString() + ": BODY ENTERED: " + body.Name.ToString());
			determineTarget();
		}
	}

	public void _on_sensor_area_body_exited(Node3D body)
	{
		if (body != this && !isTargetWorld(body))
		{
            GD.Print(this.Name.ToString() + ": BODY EXITED: " + body.Name.ToString());
            determineTarget();
        }
	}

    // TODO: Fix ground check to something more generic/useful than "Plane"
    /// <summary>
    /// Takes in a Node3D and determines if the node is part of the world geometry or not
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool isTargetWorld(Node3D target)
	{
		return target.GetParent().Name.ToString() == "Plane";
		//return target.GetOwner<Node3D>().Name.ToString() == "env_test"; // doesn't work
	}
}
