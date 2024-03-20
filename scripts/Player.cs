using Godot;
using System;
using System.Linq;

public partial class Player : CharacterBody3D
{
	public const float Speed = 10.0f;
	public const float JumpVelocity = 10.0f;
	[Export]
    public float MouseSensitivity = 0.005f;
	[Export]
	public float sync_weight = 0.5f;
	[Export]
	public PackedScene Bolt;

	private Vector3 syncPos = new Vector3(0,0,0);
	private Vector3 syncRot = new Vector3(0,0,0);

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = 20;
	// public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	private Camera3D _camera;
	private AnimationPlayer _animPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_camera = (Camera3D)GetNode("camera");
		_animPlayer = (AnimationPlayer)GetNode("AnimationPlayer");
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
		Bolt = GD.Load<PackedScene>("res://scenes/bolt_new.tscn");
	}

    public override void _UnhandledInput(InputEvent @event)
    {
		// TODO: Currently there is a bug where the host has control over the camera of any player who hasn't moved yet
		if(GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
		{
			_camera.Current = true;		// added to fix stealing cameras
			if (@event is InputEventMouseMotion eventMouseMotion)
			{
				// Mouse look
				this.RotateY(-eventMouseMotion.Relative.X * MouseSensitivity);
				_camera.RotateX(-eventMouseMotion.Relative.Y * MouseSensitivity);
				Vector3 cameraRot = _camera.RotationDegrees;
				cameraRot.X = Mathf.DegToRad(Mathf.Clamp(cameraRot.X, -70, 70));
				_camera.Rotation = cameraRot;
			}
			// Shooty
			if (Input.IsActionJustPressed("shoot") && _animPlayer.CurrentAnimation != "shoot")
			{
				Rpc("PlayShootEffects");
			}
		}
    }

    public override void _PhysicsProcess(double delta)
	{
		// Am i the local player?
		if(GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
		{
			Vector3 velocity = Velocity;

			// Add the gravity.
			if (!IsOnFloor())
				velocity.Y -= gravity * (float)delta;

			// Handle Jump.
			if (Input.IsActionJustPressed("jump") && IsOnFloor())
				velocity.Y = JumpVelocity;

			// Get the input direction and handle the movement/deceleration.
			Vector2 inputDir = Input.GetVector("strafe_left", "strafe_right", "forward", "back");
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
			// Animate the Crossbow
			if (_animPlayer.CurrentAnimation != "shoot")
			{
				if (inputDir != Vector2.Zero && IsOnFloor())
				{
					_animPlayer.Play("move");
				}
				else
				{
					_animPlayer.Play("idle");
				}
			}

			// Move the character
			Velocity = velocity;
			MoveAndSlide();

			// Sync for multiplayer
			syncPos = GlobalPosition;
			// TODO: SLERP ROTATION and also camera rotation for up down!
			// syncRot = Rotation;
		}
		// We're not the local player, lerp the position / rotation data
		else
		{
			GlobalPosition = GlobalPosition.Lerp(syncPos, sync_weight);
			// Rotation = syncRot.Lerp(Rotation.Normalized(), sync_weight);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void PlayShootEffects()
	{
		_animPlayer.Stop();
		_animPlayer.Play("shoot");
		ShootArrow();
	}

	private void ShootArrow()
	{
		Node3D b = Bolt.Instantiate<Node3D>();
		b.Position = GetNode<RayCast3D>("camera/crossbow/BoltSpawn").GlobalPosition;
		b.Basis = GetNode<RayCast3D>("camera/crossbow/BoltSpawn").GlobalTransform.Basis;
		GetTree().Root.AddChild(b);
	}

	private void _on_animation_player_animation_finished(string anim_name)
	{
		if (anim_name == "shoot")
		{
			_animPlayer.Play("idle");
		}
	}

	public void SetUpPlayer(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			name = "Incorgnito" + this.Name.ToString();
		}
		GetNode<Label3D>("nametag").Text = name;
	}
}