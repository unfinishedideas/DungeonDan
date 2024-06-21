using Godot;
using System;
using StateMachine;

public partial class EnemyDamaged : EnemyState
{
    [Export]
    protected EnemyState SearchingState;
    [Export]
    protected EnemyState ChasingState;

    private HealthComponent _healthComponent;
    private SensorAreaComponent _sensorArea;

    public override void _Ready()
    {
        base._Ready();
        _healthComponent = Owner.GetNode<HealthComponent>("%HealthComponent");
        _sensorArea = Owner.GetNode<SensorAreaComponent>("%SensorAreaComponent"); 
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
        if (_sensorArea.IsCurrentTargetSet())
        {
            StateMachine?.ChangeState(ChasingState);
        }
        else
        {
            StateMachine?.ChangeState(SearchingState);
        }
    }
}

