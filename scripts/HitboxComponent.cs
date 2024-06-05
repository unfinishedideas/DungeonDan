using Godot;
using System;

public partial class HitboxComponent : Area3D
{
    [Export]
    private HealthComponent _healthComponent;

    public void Damage(Attack attack)
    {
        _healthComponent.Damage(attack);
    }
}
