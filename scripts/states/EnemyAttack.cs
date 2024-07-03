using Godot;
using System;
using StateMachine;

public partial class EnemyAttack : EnemyState
{
    [Export]
    protected EnemyState IdleState;
    [Export]
    protected EnemyState EnemyDamageState;

    [Export]
    public float AttackCooldownTime = 1f;   // time to wait before changing to another state (if no animation present)

    [Export]
    public float TimeToHit = .25f;      // the time to wait before actually dealing damage during the animation
    [Export]
    public string AttackAnimationName;

    [Export]
    public HitboxComponent HitboxComponent;

    protected Timer _cooldownTimer;
    protected Timer _timeToHitTimer;
    private HealthComponent _healthComponent;
    private bool _tookDamage = false;

    public override void _Ready()
    {
        base._Ready();

        if (AnimPlayer != null && AttackAnimationName != null)
        {
            AnimPlayer.AnimationFinished += (AttackAnimationName) => CooldownTimeout();
        }
        else
        {
            _cooldownTimer = new Timer();
            AddChild(_cooldownTimer);
            _cooldownTimer.WaitTime = AttackCooldownTime;
            _cooldownTimer.OneShot = true;
            _cooldownTimer.Timeout += () => CooldownTimeout();
        }

        _timeToHitTimer = new Timer();
        AddChild(_timeToHitTimer);
        _timeToHitTimer.WaitTime = TimeToHit;
        _timeToHitTimer.OneShot = true;
        _timeToHitTimer.Timeout += () => DealDamage();

        _healthComponent = Owner.GetNode<HealthComponent>("%HealthComponent"); 
        OnEnter += Enter;
        OnExit += Exit;
    }

    private void Enter()
    {
        Attack();
        _tookDamage = false;
        _healthComponent.TookDamage += TookDamage;
    }

    private void Exit()
    {
        _healthComponent.TookDamage -= TookDamage;
    }

    // Begin the attack procedures
    public void Attack()
    {
        AnimPlayer?.Play(AttackAnimationName);
        _timeToHitTimer.Start();
    }

    // Actually deal damage once time to hit times out
    private void DealDamage()
    {
        _cooldownTimer?.Start();
        HitboxComponent.Attack();
    }

    // If the enemy takes damage during the attack
    private void TookDamage()
    {
        _tookDamage = true;
    }

    // Once the attack has finished
    public void CooldownTimeout()
    {
        if (_tookDamage == true)
        {
            _tookDamage = false;
            StateMachine?.ChangeState(EnemyDamageState);
        }
        // Attack again if there is still a target in range
        else if (HitboxComponent.IsTargetInRange() == true)
        {
            Attack();
        }
        else
        {
            StateMachine?.ChangeState(IdleState);
        }
    }
}

