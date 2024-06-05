using Godot;
using System;

// Some reference from here: https://github.com/ImmersiveRPG/ExampleRaycastBullets/blob/master/project/src/Bullet/Bullet.gd
// https://www.youtube.com/watch?v=joMBVo_ZwKI

public partial class Projectile : Node3D
{
    [Export]
    public float Speed = -80.0f;
    [Export]
    public float Damage = 10f;
    [Export]
    public float Knockback = 5f;
    [Export]
    public float FlightTime = 10f;
    [Export]
    private RayCast3D _ray;

    [Signal]
    public delegate void FlightTimeExpiredEventHandler();
    [Signal]
    public delegate void HitSomethingEventHandler();

    public Attack Attack;
    private Timer FlightTimer;
    private const float MinRaycastDistance = 0.05f;
    private Vector3 _velocity;
    private bool _destroyed = false;

    public override void _Ready()
    {
        FlightTimer = new Timer();
        AddChild(FlightTimer);
        FlightTimer.WaitTime = FlightTime;
        FlightTimer.OneShot = true;
        FlightTimer.Timeout += () => ExpireFlightTime();
        FlightTimer.Start();

        Attack = new Attack(Damage, Knockback);
        _velocity = this.Transform.Basis.Z * Speed;
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
            // update this!
            _ray.ForceRaycastUpdate();
            if (_ray.IsColliding())
            {
                var collider = (Node)_ray.GetCollider();
                if (!collider.IsInGroup("enemy_sensor_area"))
                {
                    if (collider.IsInGroup("enemy_hitbox"))
                    {
                        GD.Print("HIT ENEMY");
                    }
                    if (collider.IsInGroup("players"))
                    {
                        GD.Print("That's a player, dog");
                    }
                    // move bolt back to the impact point
                    Transform3D boltTransform = this.Transform;
                    boltTransform.Origin = _ray.GetCollisionPoint();
                    this.GlobalTransform = boltTransform;
                    EmitSignal(SignalName.HitSomething);
                    _destroyed = true;
                }
            }
        }
    }

    public void _on_area_3d_body_entered(Node3D body)
    {
        HitboxComponent hitbox = body.GetNodeOrNull<HitboxComponent>("Components/HitboxComponent");
        if (hitbox != null)
        {
            hitbox.Damage(Attack);
        }

    }

    public void ExpireFlightTime()
    {
        _destroyed = true;
        EmitSignal(SignalName.FlightTimeExpired);
    }

}

