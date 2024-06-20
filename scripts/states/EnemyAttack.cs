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
    public float AttackCooldownTime = 1f;

    protected Timer CooldownTimer;
    private HealthComponent _healthComponent;
    private bool tookDamage = false;

    public override void _Ready()
    {
        base._Ready();
        CooldownTimer = new Timer();
        AddChild(CooldownTimer);
        CooldownTimer.WaitTime = AttackCooldownTime;
        CooldownTimer.OneShot = true;
        CooldownTimer.Timeout += () => CooldownTimeout();
        _healthComponent = Owner.GetNode<HealthComponent>("%HealthComponent"); 
        OnEnter += Enter;
        OnExit += Exit;
    }

    private void Enter()
    {
        Attack();
        _healthComponent.TookDamage += TookDamage;
    }

    private void Exit()
    {
        _healthComponent.TookDamage -= TookDamage;
    }

    private void TookDamage()
    {
        tookDamage = true;
    }

    public void Attack()
    {
        GD.Print("ATTACK!");
        CooldownTimer.Start();
    }

    public void CooldownTimeout()
    {
        if (tookDamage == true)
        {
            StateMachine?.ChangeState(EnemyDamageState);
        }
        else
        {
            StateMachine?.ChangeState(IdleState);
        }
    }
}

