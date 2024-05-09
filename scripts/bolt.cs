using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

// Some reference from here: https://github.com/ImmersiveRPG/ExampleRaycastBullets/blob/master/project/src/Bullet/Bullet.gd
// https://www.youtube.com/watch?v=joMBVo_ZwKI

public partial class bolt : Node3D
{
    [Export]
    public const float Speed = -80.0f; // original value 100
    [Export]
    public float Damage = 10f;

    private const float MinRaycastDistance = 0.05f;
    private Vector3 _velocity;
    private RayCast3D _ray;
    private MeshInstance3D _impactSphereMesh;
    private bool _destroyed = false;
    private AnimationPlayer _player;

    //[Signal] public delegate void BoltCollidedEventHandler(float damage);

    public override void _Ready()
    {
        _velocity = this.Transform.Basis.Z * Speed;
        _ray = GetNode<RayCast3D>("RayCast3D");
        _impactSphereMesh = GetNode<MeshInstance3D>("ImpactSphereMesh");
        _player = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void _PhysicsProcess(double delta)
	{
        if (!_destroyed)
        {
            // Move the bolt
            var distance = _velocity.Length() * delta;
            Vector3 delta_velocity = new Vector3(_velocity.X * (float)delta, _velocity.Y * (float)delta, _velocity.Z * (float)delta);

            this.Transform = this.Transform.Translated(delta_velocity);

            // Change the ray start position to the bullets's previous position
            // NOTE: The ray is backwards, so if it is hitting multiple targets, we
            // get the target closest to the bullet start position.
            // Also make the ray at least the min length
            Vector3 targetPos = _ray.TargetPosition;
            Transform3D rayTransform = _ray.Transform;
            if (distance > MinRaycastDistance)
            {
                targetPos.Z = -(float)distance;
                rayTransform.Origin.Z = (float)distance;
            }
            else
            {
                targetPos.Z = -MinRaycastDistance;
                rayTransform.Origin.Z = MinRaycastDistance;
            }
            _ray.TargetPosition = targetPos;
            _ray.Transform = rayTransform;

            // Check if bolt hit something
            _ray.ForceRaycastUpdate();
            if (_ray.IsColliding())
            {
                var collider = (Node)_ray.GetCollider();
                if (!collider.IsInGroup("enemy_sensor_area"))
                {
                    if (collider.IsInGroup("enemy_hitbox"))
                    {
                        GD.Print("HIT ENEMY");
                        //EmitSignal(SignalName.BoltCollided, Damage);
                    }
                    // move bolt back to the impact point
                    Transform3D boltTransform = this.Transform;
                    boltTransform.Origin = _ray.GetCollisionPoint();
                    this.GlobalTransform = boltTransform;
                    _impactSphereMesh.GlobalTransform = boltTransform;
                    // Hit object
                    PlayHitEffects();                    
                }
            }
        }
    }

    public void PlayHitEffects()
    {
        _player.Play("destroy");
        _destroyed = true;
    }

    public void _on_bolt_life_timer_timeout()
    {
        _player.Play("queue_free");
    }
}
