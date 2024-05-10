using Godot;
using System;
using System.Linq;

public partial class Player : CharacterBody3D
{
    [Export]
    public float MaxSpeed = 7.0f;
    [Export]
    public float AccelerationSpeed = 1.0f;
    [Export]
    public float SprintMultiplier = 1.5f;
    [Export]
    public float Friction = 1.0f;
    [Export]

    public float JumpVelocity = 10.0f;
    [Export]
    public float WindResistance = .005f;
    [Export]
    public float CoyoteTime = 1.0f;
    [Export]
    public float AirControl = 0.2f;

    public float MouseSensitivity = 0.005f;
    [Export]

    public float SyncWeight = 0.5f;
    [Export]

    public PackedScene Bolt;

    public float Gravity = 20;
    // Default: Get the Gravity from the project settings to be synced with RigidBody nodes.
    // public float Gravity = ProjectSettings.GetSetting("physics/3d/default_Gravity").AsSingle();

    public Vector3 SyncPos = new Vector3(0,0,0);
    public Vector3 SyncRot = new Vector3(0,0,0);	// presently unused

    private float _currentSpeed = 0f;
    private Camera3D _camera;
    private RayCast3D _projectileRaycast;
    private RayCast3D _boltSpawn;
    private AnimationPlayer _animPlayer;
    private Marker3D _aimMarker;
    private Label3D _nametag;
    private AudioStreamPlayer3D _boltSFX;
        
    public override void _EnterTree()
    {
        base._EnterTree();
            if (GameManager.IsMultiplayerGame == true)
            {
                _nametag = GetNode<Label3D>("nametag");
                _nametag.Visible = true;
                GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
            }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
            _camera = (Camera3D)GetNode("camera");
            _animPlayer = (AnimationPlayer)GetNode("AnimationPlayer");
            _boltSpawn = GetNode<RayCast3D>("camera/crossbow/BoltSpawn");
            _projectileRaycast = GetNode<RayCast3D>("camera/ProjectileRaycast");
            _aimMarker = GetNode<Marker3D>("camera/AimMarker");
            _boltSFX = GetNode<AudioStreamPlayer3D>("SFX/BoltFire");
            Bolt = GD.Load<PackedScene>("res://scenes/weapons/bolt.tscn");

            if (IsCurrentPlayerMPAuth())
            {
                _camera.Current = true;
            }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsCurrentPlayerMPAuth())	
            return;
                
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

    private bool IsCurrentPlayerMPAuth()
    {
        return GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Are we the local player?
        if(GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
        {
            Vector3 velocity = Velocity;

            // Add the Gravity.
            if (!IsOnFloor())
                velocity.Y -= Gravity * (float)delta;

            // Handle Jump.
            if (Input.IsActionJustPressed("jump") && IsOnFloor())
                velocity.Y = JumpVelocity;

            // Get the input direction and handle the movement/deceleration.
            Vector2 inputDir = Input.GetVector("strafe_left", "strafe_right", "forward", "back");
            Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
            float tempAirControl = IsOnFloor() switch
            {
                true => 1.0f,
                false => AirControl, 
            };
            float tempWindRes = IsOnFloor() switch
            {
                true => 1.0f,
                false => WindResistance,
            };

            if (direction != Vector3.Zero)
            {
                if(IsOnFloor())
                {
                    float modifiedMaxSpeed = MaxSpeed;
                    if (Input.IsActionPressed("sprint")) 
                    {
                        modifiedMaxSpeed *= SprintMultiplier;
                    }
                    _currentSpeed = Mathf.MoveToward(_currentSpeed, modifiedMaxSpeed, AccelerationSpeed);
                }
                else
                {
                    _currentSpeed = Mathf.MoveToward(_currentSpeed, 0, Friction * tempWindRes);
                }
                // eehhh this is messed up and it's late, we'll fix tomorrow
                velocity.X = (direction.X * tempAirControl) * _currentSpeed;
                velocity.Z = (direction.Z * tempAirControl) * _currentSpeed;
            }
            else
            {
                velocity.X = Mathf.MoveToward(Velocity.X, 0, Friction * tempWindRes); 
                velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Friction * tempWindRes);
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
            SyncPos = GlobalPosition;
            //SyncRot = Rotation;	// doesn't work properly yet, implement before serious multiplayer code
        }
        // We're not the local player, lerp the position / rotation data
        else
        {
            GlobalPosition = GlobalPosition.Lerp(SyncPos, SyncWeight);
            //Rotation = SyncRot.Lerp(Rotation.Normalized(), SyncWeight);
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
        // TODO: Figure out how to ignore the enemy sensor area and still get raycasting
        var collider = (Node)_projectileRaycast.GetCollider();
        Vector3 targetPos = _aimMarker.GlobalPosition;
        if (_projectileRaycast.IsColliding() && !collider.IsInGroup("enemy_sensor_area"))
        {
            targetPos = _projectileRaycast.GetCollisionPoint();
        }

        Node3D b = Bolt.Instantiate<Node3D>();
        b.Position = _boltSpawn.GlobalPosition;
        b.LookAtFromPosition(_boltSpawn.GlobalPosition, targetPos);
        GetTree().Root.GetNode("World").AddChild(b);
        _boltSFX.Play();
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
