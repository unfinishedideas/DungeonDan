using Godot;
using System;

public partial class SceneManager : Node
{
	[Export]
	private PackedScene playerScene;

	// Called when the node enters the scene tree for the first time.
	/// <summary>
	/// this will spawn all the players into the game once it is started
	/// </summary>
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
            currentPlayer.Quit += _on_player_quit;

			if (GameManager.IsMultiplayerGame == true)
			{
				// loop through the spawn points and select one for the current player
				foreach (Node3D spawnPoint in GetTree().GetNodesInGroup("PlayerSpawnpoints"))
				{
					if(int.Parse(spawnPoint.Name) == index){
						currentPlayer.GlobalPosition = spawnPoint.GlobalPosition;
						currentPlayer.GlobalRotation = spawnPoint.GlobalRotation;
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
	}

    // Signals ----------------------------------------------------------------
    public void _on_player_quit()
    {
        GetTree().Quit();
    }
}
