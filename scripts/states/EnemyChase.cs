using Godot;
using System;
using StateMachine;

public partial class EnemyChase : EnemyState
{
    [Export]
    protected Node3D EnemyMesh;
    [Export]
    protected float AttackWindupTime = 0.25f;
    [Export]
    protected string AttackChargeAnimationName;

    private SensorAreaComponent _sensorArea;
    private HealthComponent _healthComponent;
    private HitboxComponent _hitbox;
    private Timer _attackTimer;

    public override void _Ready()
    {
        base._Ready();

        if (AnimPlayer != null && AttackChargeAnimationName != null)
        {
            AnimPlayer.AnimationFinished += (AttackChargeAnimationName) => AttackTarget();
        }
        else
        {
            _attackTimer = new Timer();
            AddChild(_attackTimer);
            _attackTimer.WaitTime = AttackWindupTime;
            _attackTimer.OneShot = true;
            _attackTimer.Timeout += () => AttackTarget();
        }

        _sensorArea = Owner.GetNode<SensorAreaComponent>("%SensorAreaComponent"); 
        _healthComponent = Owner.GetNode<HealthComponent>("%HealthComponent"); 
        _hitbox = Owner.GetNode<HitboxComponent>("%HitboxComponent");
        OnPhysicsProcess += PhysicsProcess;
        OnEnter += Enter;
        OnExit += Exit;
    }

    private void Enter()
    {
        _sensorArea.TargetLost += TargetLost;
        _sensorArea.UpdateDirection += UpdateDirectionHandler;
        _sensorArea.NavTargetReached += NavTargetReachedHandler;
        _healthComponent.TookDamage += TookDamage;
        _hitbox.DetectedHurtbox += StartAttackTimer;
    }

    private void Exit()
    {
        _sensorArea.TargetLost -= TargetLost;
        _sensorArea.UpdateDirection -= UpdateDirectionHandler;
        _sensorArea.NavTargetReached -= NavTargetReachedHandler;
        _healthComponent.TookDamage -= TookDamage;
        _hitbox.DetectedHurtbox -= StartAttackTimer;
    }

    private void StartAttackTimer()
    {
        if (AnimPlayer != null && AttackChargeAnimationName != null)
        {
            AnimPlayer.Play(AttackChargeAnimationName);
        }
        else
        {
            _attackTimer.Start();
        }
    }

    private void AttackTarget()
    {
        StateMachine?.ChangeState(STATE_ATTACK);
    }

    private void TargetLost()
    {
        StateMachine?.ChangeState(STATE_SEARCH);
    }

    private void TookDamage()
    {
        StateMachine?.ChangeState(STATE_DAMAGED);
    }

    private void PhysicsProcess(double delta)
    {
        MoveTowardTarget(delta);
    }

    public void NavTargetReachedHandler()
    {
        _direction = Vector3.Zero;
        StateMachine?.ChangeState(STATE_ATTACK);
    }
}
