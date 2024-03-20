using Godot;
using System;

public partial class bolt : Node3D
{

	[Export]
	const float SPEED = 100.0f;

	private Node3D mesh;
	private RayCast3D raycast;
	private GpuParticles3D particles;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mesh = (Node3D)GetNode("arrow_mesh");
		raycast = (RayCast3D)GetNode("RayCast3D");
		particles = GetNode<GpuParticles3D>("GPUParticles3D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// TODO: Make sure this is an okay cast for delta
		Translate(new Vector3(0,0,-SPEED * (float)delta));
		if (raycast.IsColliding())
		{
			mesh.Visible = false;
			particles.Emitting = true;
		}
	}

	private async void d()
	{
		await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
		QueueFree();
	}

	// delete the bullet when timer is up
	private void _on_timer_timeout()
	{
		QueueFree();
	}
}
