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
    public delegate void HitSomethingSignalEventHandler();

    public Attack ProjectileAttack;
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

        ProjectileAttack = new Attack(Damage, Knockback);
        _velocity = this.Transform.Basis.Z * Speed;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_destroyed)
        {
            MoveBolt(delta);
        }
    }

    public void MoveBolt(double delta)
    {
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
        _ray.ForceRaycastUpdate();
        if (_ray.IsColliding())
        {   
            HitSomething();
        }
    }

    private void HitSomething(Node3D body = null)
    {
        var collider = _ray.GetCollider();

        // Move the projectile to where it collided
        Transform3D projectileTransform = this.Transform;
        projectileTransform.Origin = _ray.GetCollisionPoint();
        this.GlobalTransform = projectileTransform;

        // Determine if it has a hurtbox. note: component must have unique name selectable
        if (collider.GetClass() == "Area3D")
        {
            Area3D colliderArea3D = (Area3D)collider;
            HurtboxComponent hurtbox = colliderArea3D.GetNodeOrNull<HurtboxComponent>("%HurtboxComponent");
            hurtbox?.Damage(ProjectileAttack);
        }
        _destroyed = true;
        EmitSignal(SignalName.HitSomethingSignal);
    }

    public void ExpireFlightTime()
    {
        _destroyed = true;
        EmitSignal(SignalName.FlightTimeExpired);
    }
}

