using Godot;
using System;

public partial class VelocityComponent : Node3D
{
    public Node rootNode;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        rootNode = this.Owner;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
