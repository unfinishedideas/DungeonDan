using Godot;
using System;

public partial class HurtboxComponent : Area3D
{
    [Export]
    private HealthComponent _healthComponent;

    [Signal]
    public delegate void HitByProjectileEventHandler();

    [Signal]
    public delegate void HitByHurtboxEventHandler();

    public void Damage(Attack attack)
    {
        _healthComponent.Damage(attack);
    }

    public void _on_hurtbox_component_body_entered(Node3D body)
    {
        GD.Print($"Hurtbox body entered: {body}");
        EmitSignal(SignalName.HitByHurtbox);
    }

    public void _on_hurtbox_component_area_entered(Area3D area)
    {
        GD.Print($"Hurtbox area entered: {area}");
    }
}
