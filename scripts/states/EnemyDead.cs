using Godot;
using System;
using StateMachine;

public partial class EnemyDead : EnemyState
{
    [Export]
    public AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        base._Ready();
        OnEnter += Enter;
    }

    private void Enter()
    {
        GD.Print($"{_enemy.Name.ToString()} has died!");
        if (_animationPlayer != null)
        {
            _animationPlayer?.Play("die");
        }
        else
        {
            Owner.QueueFree();
        }
    }
}

