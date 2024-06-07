using Godot;
using System;

public partial class main_menu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_single_player_button_down()
	{
		GameManager.Players.Add(new PlayerInfo() { Id = 1, Name = "Player", Score = 0 });
		var scene = ResourceLoader.Load<PackedScene>("res://scenes/levels/test_world.tscn").Instantiate<Node>();
		GetTree().Root.AddChild(scene);
		Hide();
	}

	public void _on_multi_player_button_down()
	{
		var scene = ResourceLoader.Load<PackedScene>("res://scenes/ui/MultiplayerController.tscn").Instantiate<Node>();
		GetTree().Root.AddChild(scene);
		Hide();
	}

	public void _on_quit_button_down()
	{
		GetTree().Quit();
	}
}
