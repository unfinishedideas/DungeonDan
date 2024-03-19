using Godot;
using System;
using System.Xml.Resolvers;

public partial class test_world : Node
{
	private PackedScene PlayerScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PlayerScene = GD.Load<PackedScene>("res://scenes/player.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("quit"))
		{
			GetTree().Quit();
		}
	}

	private void add_player(int peer_id)
	{
		var Player = PlayerScene.Instantiate<Player>();
		Player.Name = peer_id.ToString();
		AddChild(Player);
	}
}
