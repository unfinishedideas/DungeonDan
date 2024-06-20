using Godot;
using System;

public partial class HurtboxComponent : Area3D
{
    [Export]
    private HealthComponent _healthComponent;


    public override void _Ready()
    {
        // Leaving this for reference, a tutorial did this for some reason
        //CollisionLayer = 0;
        //CollisionMask = 2;
    }

    // This is sometimes called from other places
    public void Damage(Attack attack)
    {
        _healthComponent.Damage(attack);
    }

    // If this doesn't fire, ensure that the signal is connected to root node
    public void _on_area_entered(Area3D area)
    {
        if (area is HitboxComponent && !Owner.IsAncestorOf(area))
        {
            HitboxComponent box = (HitboxComponent)area;
            Damage(box.HitboxAttack);
        }
    }
}
