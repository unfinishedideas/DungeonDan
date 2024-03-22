using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

// Some reference from here: https://github.com/ImmersiveRPG/ExampleRaycastBullets/blob/master/project/src/Bullet/Bullet.gd
// https://www.youtube.com/watch?v=joMBVo_ZwKI

public partial class bolt : Node3D
{
    [Export]
    public const float SPEED = -260.0f;
    [Export]
    public float damage = 10f;

    private const float MIN_RAYCAST_DISTANCE = 0.05f;
    //[Signal] public delegate void BoltCollidedEventHandler(float damage);

    private Vector3 _velocity;
    private RayCast3D _ray;
    private GpuParticles3D _hitParticles;
    private GpuParticles3D _flyParticles;
    private MeshInstance3D _mesh;
    private MeshInstance3D _impactSphereMesh;
    private bool destroyed = false;


    public override void _Ready()
    {
        _velocity = this.Transform.Basis.Z * SPEED;
        _ray = GetNode<RayCast3D>("RayCast3D");
        _hitParticles = GetNode<GpuParticles3D>("ImpactParticles");
        _flyParticles = GetNode<GpuParticles3D>("Sparkles");
        _mesh = GetNode<MeshInstance3D>("arrow1");
        _impactSphereMesh = GetNode<MeshInstance3D>("ImpactSphereMesh");
    }

    public override void _PhysicsProcess(double delta)
	{
        if (!destroyed)
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
            if (distance > MIN_RAYCAST_DISTANCE)
            {
                targetPos.Z = -(float)distance;
                rayTransform.Origin.Z = (float)distance;
            }
            else
            {
                targetPos.Z = -MIN_RAYCAST_DISTANCE;
                rayTransform.Origin.Z = MIN_RAYCAST_DISTANCE;
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
                        //EmitSignal(SignalName.BoltCollided, damage);
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
        destroyed = true;
        _mesh.Visible = false;
        _hitParticles.Emitting = true;
        _flyParticles.Emitting = false;
        _impactSphereMesh.Visible = true;
    }

    // Delete the bolt after a timeout 
    public void _on_projectile_timeout_timeout()
    {
        QueueFree();
    }
}
