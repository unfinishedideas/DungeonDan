using Godot;
using System;

public partial class Bolt : Projectile
{
    [Export]
    private MeshInstance3D _impactSphereMesh;
    [Export]
    private AnimationPlayer _player;

    public void PlayHitEffects()
    {
        _player = GetNode<AnimationPlayer>("AnimationPlayer");
        _player.Play("destroy");
    }

    public void _on_flight_time_expired()
    {
        _player.Play("queue_free");
    }

    public void _on_hit_something()
    {
        _impactSphereMesh.GlobalTransform = this.Transform;
        PlayHitEffects();
    }
}

