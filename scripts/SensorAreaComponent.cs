using Godot;
using System;
using System.Collections.Generic;

public partial class SensorAreaComponent : Area3D
{

    [Export]
    public NavigationAgent3D _navAgent;
    [Export]
    public RayCast3D _sightRay;

    public Vector3 Direction;

    //[Signal]
    //public delegate void TargetUpdatedEventHandler();
    //EmitSignal(SignalName.TargetUpdated);

    private Node3D _currentTarget;
    private List<Node3D> _targetList = new List<Node3D>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        this.BodyEntered += (Node3D body) => CustomBodyEntered(body);
        this.BodyExited += (Node3D body) => CustomBodyExited(body);
        Direction = Vector3.Zero;
	}

    public void CustomBodyEntered(Node3D body)
    {
        if (body.IsInGroup("players"))
        {
            if (!_targetList.Contains(body))
            {
                _targetList.Add(body);
                UpdateCurrentTarget();
            }
        }
    }

    public void CustomBodyExited(Node3D body)
    {
        if (body.IsInGroup("players"))
        {
            if (_targetList.Contains(body))
            {
                _targetList.Remove(body);
                UpdateCurrentTarget();
            }
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        // If we are targeting a player, and they haven't disconnected, nav toward them
        if (_targetList.Count != 0)
        {
            UpdateCurrentTarget();
            if (IsInstanceValid(_currentTarget) && _currentTarget != null)
            {
                UpdateTargetLocation(_currentTarget);
            }
        }
        Vector3 destination = _navAgent.GetNextPathPosition();
        Vector3 localDestination = destination - GlobalPosition;
        Direction = localDestination.Normalized();
    }

    private void UpdateCurrentTarget()
    {
        // enumerate potential targets and get the cloest one
        _currentTarget = null;
        float closestDistance = float.MaxValue;

        if (this.Monitoring)
        {
            foreach(Node3D target in _targetList)
            {
                float distanceFromTarget = this.GlobalPosition.DistanceTo(target.GlobalPosition);
                // Target the closest player that is in line of sight
                if (IsTargetInLineOfSight(target) && distanceFromTarget < closestDistance)
                {
                    closestDistance = distanceFromTarget;
                    _currentTarget = target;
                }
            }
        }
    }

    private bool IsTargetInLineOfSight(Node3D target)
    {
        Vector3 local_destination = target.GlobalPosition - GlobalPosition;
        Vector3 direction = local_destination.Normalized();
        float distanceFromTarget = this.GlobalPosition.DistanceTo(target.GlobalPosition);
        _sightRay.TargetPosition = direction * distanceFromTarget; 

        GodotObject collided_object = _sightRay.GetCollider();
        if (collided_object == target)
        {
            return true;
        }
        return false;
    }

    private void UpdateTargetLocation(Node3D target)
    {
        _navAgent.TargetPosition = target.GlobalPosition;

    }

    // used for debugging purposes
    private void PrintTargetList()
    {
        GD.Print("Current target list:");
        foreach (Node3D target in _targetList)
        {
            GD.Print($"{target.Name}");
        }
    }
}
