using Godot;
using System;

public partial class HurtboxComponent : Area3D
{
    [Export]
    private HealthComponent _healthComponent;

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 2;
    }

    public void Damage(Attack attack)
    {
        _healthComponent.Damage(attack);
    }
}
