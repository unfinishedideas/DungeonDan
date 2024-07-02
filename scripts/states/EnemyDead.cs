using Godot;
using System;
using StateMachine;

public partial class EnemyDead : EnemyState
{
    public override void _Ready()
    {
        base._Ready();
        OnEnter += Enter;
    }

    private void Enter()
    {
        GD.Print($"{_enemy.Name.ToString()} has died!");
        if (AnimPlayer != null)
        {
            AnimPlayer?.Play("die");
        }
        else
        {
            Owner.QueueFree();
        }
    }
}

