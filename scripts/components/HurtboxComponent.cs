using Godot;
using System;

public partial class HurtboxComponent : Area3D
{
    [Export]
    private HealthComponent _healthComponent;

    [Signal]
    public delegate void HitByProjectileEventHandler();

    [Signal]
    public delegate void HitByHitboxEventHandler();

    public void Damage(Attack attack)
    {
        _healthComponent.Damage(attack);
    }
}
