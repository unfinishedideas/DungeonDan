using Godot;
using System;
using StateMachine;

public partial class EnemyDamaged : EnemyState
{
    [Export]
    protected EnemyState SearchingState;
    [Export]
    protected EnemyState ChasingState;
    [Export]
    protected EnemyState AttackState;
    [Export]
    protected HitboxComponent HitboxComponent;
    [Export]
    protected String HurtAnimationName; // write the name of the hurt animation here

    private HealthComponent _healthComponent;
    private SensorAreaComponent _sensorArea;
    private Timer _resetTimer = null;  // used in case there is not a hurt animation to play to call CooldownTimeout()

    public override void _Ready()
    {
        base._Ready();
        _healthComponent = Owner.GetNode<HealthComponent>("%HealthComponent");
        _sensorArea = Owner.GetNode<SensorAreaComponent>("%SensorAreaComponent"); 

        if (AnimPlayer != null && HurtAnimationName != null)
        {
            AnimPlayer.AnimationFinished += (HurtAnimationName) => CooldownTimeout();
        }
        else
        {
            _resetTimer = new Timer();
            AddChild(_resetTimer);
            _resetTimer.WaitTime = _healthComponent.IFramesTime;
            _resetTimer.OneShot = true;
            _resetTimer.Timeout += () => CooldownTimeout();
        }

        OnEnter += Enter;
        OnExit += Exit;
    }

    private void Enter()
    {
        TakeDamage();
        _healthComponent.TookDamage += TakeDamage;
    }

    private void Exit()
    {
        _healthComponent.TookDamage -= TakeDamage;
    }

    public void TakeDamage()
    {
        GD.Print("EnemyDamaged: OW!");

        if (AnimPlayer != null && HurtAnimationName != null)
        {
            AnimPlayer.Play(HurtAnimationName);
        }
        else
        {
            _resetTimer.Start();
        }
    }

    public void CooldownTimeout()
    {
        if (HitboxComponent.IsTargetInRange())
        {
            StateMachine?.ChangeState(AttackState);
        }
        else if (_sensorArea.IsCurrentTargetSet())
        {
            StateMachine?.ChangeState(ChasingState);
        }
        else
        {
            StateMachine?.ChangeState(SearchingState);
        }
    }
}

