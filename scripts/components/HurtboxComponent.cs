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

    public void _on_area_entered(Area3D area)
    {
        if (area is HitboxComponent)
        {
           HitboxComponent box = (HitboxComponent)area;
           _healthComponent.Damage(box.HitboxAttack);
        }
    }
}
