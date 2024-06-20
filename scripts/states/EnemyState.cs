using Godot;
using StateMachine;

public partial class EnemyState : State
{
    protected Enemy _enemy;

    public override void _Ready()
    {
        base._Ready();
        _enemy = Owner as Enemy;
    }
}
