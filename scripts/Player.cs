using Godot;
using System;
using System.Linq;
using System.Net.NetworkInformation;

public partial class Player : CharacterBody3D
{
    public const float MAX_SPEED = 32.0f;
    public const float WALK_SPEED = 12f;
    //public const float STOP_SPEED = 10f;
    public float GRAVITY = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    public const float ACCELERATE = 100f;
    //public const float AIR_ACCELERATE = 100f;
    //public const float WATER_ACCELERATE = 0.15f;
    public const float FRICTION = 80f;
    //public const float WATER_FRICTION = 12f;
    [Export]
    public float JUMP_FORCE = 14f;
    public const float AIR_CONTROL = .8f;
    //public const float STEP_SIZE = 1.8f;
    //public const float MAX_HANG = 0.2f;
    //public const float PLAYER_HEIGHT = 3.6f;
    //public const float CROUCH_HEIGHT = 2.0f;

    public float MouseSensitivity = 0.005f;

    [Export]
    public float SyncWeight = 0.5f;
    [Export]
    public PackedScene Bolt;
    [Export]
    public Control PauseMenu;

    [Signal]
    public delegate void QuitEventHandler();

    //public float Gravity = 20;
    // Default: Get the Gravity from the project settings to be synced with RigidBody nodes.
    // public float Gravity = ProjectSettings.GetSetting("physics/3d/default_Gravity").AsSingle();

    public Vector3 SyncPos = new Vector3(0, 0, 0);
    public Vector3 SyncRot = new Vector3(0, 0, 0);	// presently unused

    private Camera3D _camera;
    private RayCast3D _projectileRaycast;
    private RayCast3D _boltSpawn;
    private AnimationPlayer _animPlayer;
    private Marker3D _aimMarker;
    private Label3D _nametag;
    private AudioStreamPlayer3D _boltSFX;
    private HealthComponent _healthComponent;
    private bool _inGameControl = true;

    // UI nodes
    private Control _gui;
    private Label _debugLabel;
    private Label _hpLabel;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _gui = GetNode<Control>("%gui");
        _debugLabel = _gui.GetNode<Label>("%debug_label");
        _hpLabel = _gui.GetNode<Label>("%hp_label");
        _healthComponent = GetNode<HealthComponent>("%HealthComponent");

        if (OS.IsDebugBuild() == true)
        {
            _debugLabel.Visible = true;
        }

        if (GameManager.IsMultiplayerGame == true)
        {
            _nametag = GetNode<Label3D>("nametag");
            _nametag.Visible = true;
            GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
        }
        Input.MouseMode = Input.MouseModeEnum.Captured;
        _camera = (Camera3D)GetNode("camera");
        _animPlayer = (AnimationPlayer)GetNode("AnimationPlayer");
        _boltSpawn = GetNode<RayCast3D>("camera/crossbow/BoltSpawn");
        _projectileRaycast = GetNode<RayCast3D>("camera/ProjectileRaycast");
        _aimMarker = GetNode<Marker3D>("camera/AimMarker");
        _boltSFX = GetNode<AudioStreamPlayer3D>("SFX/BoltFire");
        Bolt = GD.Load<PackedScene>("res://scenes/weapons/Bolt.tscn");

        if (IsCurrentPlayerMPAuth())
        {
            _camera.Current = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Are we the local player?
        if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
        {
            _debugLabel.Text = $"{Multiplayer.GetUniqueId()}";
            if (_inGameControl == true)
            {
                GetControlInput(delta);
            }
            // Update HUD
            _hpLabel.Text = $"{_healthComponent.Health:###}";

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

    public override void _Input(InputEvent @event)
    {
        if (!IsCurrentPlayerMPAuth())
            return;
        if (_inGameControl == true)
        {
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

        // Menu toggle
        if (Input.IsActionJustPressed("menu_toggle"))
        {
            TogglePauseMenu();
        }

        // Quit shortcut
        if (Input.IsActionJustPressed("quit"))
        {
            EmitSignal(SignalName.Quit);
        }
    }

    private bool IsCurrentPlayerMPAuth()
    {
        return GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId();
    }

    private void GetControlInput(double delta)
    {
        // Movement code ---------------------------------------------------
        Vector2 inputDir = Input.GetVector("strafe_left", "strafe_right", "forward", "back");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        Vector3 velocity = IsOnFloor() ? _GroundMove(delta, direction) : _AirMove(delta, direction);

        _debugLabel.Text += $"\nVelocity: {Velocity}";
        _debugLabel.Text += $"\nDirection: {direction}";

        // Animate the Crossbow --------------------------------------------
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
    }

    private Vector3 _GroundMove(double delta, Vector3 direction)
    {
        Vector3 velocity = Velocity;

        // Handle Jump.
        if (Input.IsActionJustPressed("jump"))
        {
            velocity.Y = JUMP_FORCE;
        }
        if (direction != Vector3.Zero)
        {
            velocity.X = Mathf.MoveToward(velocity.X, direction.X * WALK_SPEED, (float)delta * ACCELERATE);
            velocity.Z = Mathf.MoveToward(velocity.Z, direction.Z * WALK_SPEED, (float)delta * ACCELERATE);
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, (float)delta * FRICTION);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, (float)delta * FRICTION);
        }
        return velocity;
    }

    private Vector3 _AirMove(double delta, Vector3 direction)
    {
        Vector3 velocity = Velocity;

        // Add the Gravity.
        velocity.Y -= GRAVITY * (float)delta;
        // Give the player limited Air Control
        if (direction != Vector3.Zero)
        {
            // something about this still feels off   
            velocity.X = Mathf.Clamp(velocity.X + direction.X * AIR_CONTROL, -WALK_SPEED, WALK_SPEED);
            velocity.Z = Mathf.Clamp(velocity.Z + direction.Z * AIR_CONTROL, -WALK_SPEED, WALK_SPEED);
            //velocity.X = Mathf.MoveToward(velocity.X, direction.X * WALK_SPEED * AIR_CONTROL, (float)delta * AIR_ACCELERATE);
            //velocity.Z = Mathf.MoveToward(velocity.Z, direction.Z * WALK_SPEED * AIR_CONTROL, (float)delta * AIR_ACCELERATE);
        }
        return velocity;
    }

    private void ToggleMouseMode()
    {
        if(Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            _inGameControl = false;
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            _inGameControl = true;
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

    public void TogglePauseMenu()
    {
        ToggleMouseMode();
        _gui.Visible = !_gui.Visible;
        PauseMenu.Visible = !PauseMenu.Visible;
    }

    // Signals ----------------------------------------------------------------
    public void _on_pause_menu_resume_game()
    {
        TogglePauseMenu();
    }

    public void _on_pause_menu_quit_game()
    {
        EmitSignal(SignalName.Quit);
    }
}
