using Godot;
using System;

public partial class SceneManager : Node
{
	[Export]
	private PackedScene playerScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		int index = 0;
		int max_spawn = GetTree().GetNodesInGroup("PlayerSpawnpoints").Count - 1;
		foreach (var item in GameManager.Players)
		{
			Player currentPlayer = playerScene.Instantiate<Player>();
			currentPlayer.Name = item.Id.ToString();
			currentPlayer.SetUpPlayer(item.Name);
			AddChild(currentPlayer);
			foreach (Node3D spawnPoint in GetTree().GetNodesInGroup("PlayerSpawnpoints"))
			{
				if(int.Parse(spawnPoint.Name) == index){
					currentPlayer.GlobalPosition = spawnPoint.GlobalPosition;
					GD.Print("Spawning player: " + currentPlayer.Name.ToString() + " at : " + index.ToString());
					break;
				}
			}
			index++;
			if (index > max_spawn)
			{
				index = 0;
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("quit"))
		{
			GetTree().Quit();
		}
	}
}
