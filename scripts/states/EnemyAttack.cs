using Godot;
using System;
using StateMachine;

public partial class EnemyAttack : EnemyState
{
    [Export]
    protected EnemyState IdleState;

    [Export]
    public float AttackCooldownTime = 1f;

    protected Timer CooldownTimer;

    public override void _Ready()
    {
        base._Ready();
        CooldownTimer = new Timer();
        AddChild(CooldownTimer);
        CooldownTimer.WaitTime = AttackCooldownTime;
        CooldownTimer.OneShot = true;
        CooldownTimer.Timeout += () => CooldownTimeout();
        OnEnter += Enter;
    }

    private void Enter()
    {
        Attack();
    }

    public void Attack()
    {
        GD.Print("ATTACK!");
        CooldownTimer.Start();
    }

    public void CooldownTimeout()
    {
        StateMachine?.ChangeState(IdleState);
    }
}

