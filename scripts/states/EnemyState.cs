using Godot;
using StateMachine;

public partial class EnemyState : State
{
    [Export]
    public AnimationPlayer AnimPlayer;

    protected Enemy _enemy;
    public Vector3 _direction;
    private Vector3 _prevGlobalPosition;
    private Vector3 _originalGlobalPosition;
    private HealthComponent _healthComponent;
    public float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    // State names for state changes
    public string STATE_IDLE = "EnemyIdle";
    public string STATE_SEARCH = "EnemySearch";
    public string STATE_CHASE = "EnemyChase";
    public string STATE_ATTACK = "EnemyAttack";
    public string STATE_DAMAGED = "EnemyDamaged";
    public string STATE_DEAD = "EnemyDead";

    public override void _Ready()
    {
        base._Ready();
        _direction = Vector3.Zero;
        _enemy = Owner as Enemy;
        _prevGlobalPosition = _enemy.GlobalPosition;
        _originalGlobalPosition = _enemy.GlobalPosition;
        _healthComponent = Owner.GetNode<HealthComponent>("%HealthComponent");
        _healthComponent.DeathSignal += TimeToDie;
    }

    public void UpdateDirectionHandler(Vector3 direction, Vector3 targetPos)
    {
        _direction = direction;

        if (!_enemy.GlobalPosition.IsEqualApprox(targetPos))
        {
            // TODO: This still causes errors if position/targetPos is the same
            _enemy.LookAt(new Vector3(targetPos.X, _enemy.GlobalPosition.Y, targetPos.Z), new Vector3(0,1,0));
        }
    }

    public void MoveTowardTarget(double delta)
    {
        Vector3 velocity = _enemy.Velocity;
        if (_direction != Vector3.Zero)
        {
            velocity.X = _direction.X * _enemy.Speed;
            velocity.Z = _direction.Z * _enemy.Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(_enemy.Velocity.X, 0, _enemy.Speed);
            velocity.Z = Mathf.MoveToward(_enemy.Velocity.Z, 0, _enemy.Speed);
        }

        _prevGlobalPosition = _enemy.GlobalPosition;
        velocity.Y -= Gravity * (float)delta;
        _enemy.Velocity = velocity;
        _enemy.MoveAndSlide();
    }

    // TODO: This is getting called by every state at once!
    private void TimeToDie()
    {
        StateMachine?.ChangeState(STATE_DEAD);
    }
}

