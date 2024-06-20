using Godot;
using System;
using StateMachine;

public partial class EnemyDamaged : EnemyState
{
    [Export]
    protected EnemyState IdleState;

    private HealthComponent _healthComponent;

    public override void _Ready()
    {
        base._Ready();
        _healthComponent = Owner.GetNode<HealthComponent>("%HealthComponent");
        OnEnter += Enter;
        OnExit += Exit;
    }

    private void Enter()
    {
        TakeDamage();
        _healthComponent.TookDamage += TakeDamage;
        _healthComponent.IFramesExpired += CooldownTimeout;
    }

    private void Exit()
    {
        _healthComponent.TookDamage -= TakeDamage;
        _healthComponent.IFramesExpired -= CooldownTimeout;
    }

    public void TakeDamage()
    {
        GD.Print("OW!");
    }

    public void CooldownTimeout()
    {
        StateMachine?.ChangeState(IdleState);
    }
}

