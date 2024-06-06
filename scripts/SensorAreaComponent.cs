using Godot;
using System;
using System.Collections.Generic;

public partial class SensorAreaComponent : Area3D
{
    [Export]
    public NavigationAgent3D _navAgent;
    [Export]
    public RayCast3D _sightRay;

    private Vector3 _direction;

    private Node3D _currentTarget;
    private List<Node3D> _targetList = new List<Node3D>();

    [Signal]
    public delegate void UpdateDirectionEventHandler(Vector3 Direction);
    [Signal]
    public delegate void NavTargetReachedEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        this.BodyEntered += (Node3D body) => CustomBodyEntered(body);
        this.BodyExited += (Node3D body) => CustomBodyExited(body);
        _direction = Vector3.Zero;
	}

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
        if (_navAgent.IsNavigationFinished())
        {
            EmitSignal(SignalName.NavTargetReached);
        }
        else
        {
            Vector3 destination = _navAgent.GetNextPathPosition();
            Vector3 localDestination = destination - GlobalPosition;
            _direction = localDestination.Normalized();
            EmitSignal(SignalName.UpdateDirection, _direction);
        }
    }

    private void UpdateCurrentTarget()
    {
        // enumerate potential targets and get the cloest one
        _currentTarget = null;
        float closestDistance = float.MaxValue;

        if (this.Monitoring)
        {
            if (_targetList.Count > 0)
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

    // Signals ----------------------------------------------------------------
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
}
