using Godot;
using System;
using System.Linq;
// Ref FinePointCGI: https://www.youtube.com/watch?v=MOJVjFngOs4

public partial class MultiplayerController : Control
{
	[Export]
	private int port = 8910;
	[Export]
	private string address = "127.0.0.1"; 
	[Export]
	private int max_clients = 16;
	
	private ENetMultiplayerPeer peer;
	private ENetConnection.CompressionMode compression_mode = ENetConnection.CompressionMode.Fastlz;

	private bool hosting = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Connecting signals for multiplayer
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		Multiplayer.ServerDisconnected += ServerDisconnected;

		// if a dedicated server
		if (OS.GetCmdlineArgs().Contains("--server"))
		{
			hostGame();
		}
	}

	// Multiplayer Event Functions ---------------------------------------------------------------------------------------
	/// <summary>
	/// runs when the connection fails and it runs only on the client
	/// </summary>
	private void ConnectionFailed()
    {
		GD.Print("connection failed!");
    }

	/// <summary>
	/// runs when the connection is successful and only runs on the clients
	/// </summary>
    private void ConnectedToServer()
    {
		GD.Print("connected to server");
		// RPC ID makes the RPC to a specific ip. In this case, the server
		RpcId(1, "sendPlayerInformation", GetNode<LineEdit>("Panel/NameEntry").Text, Multiplayer.GetUniqueId());
    }

	/// <summary>
	/// runs when a player disconnects and runs on all peers
	/// </summary>
	/// <param name="id">id of the player that disconnected</param>
    private void PeerDisconnected(long id)
    {
		GD.Print("player disconnected: " + id.ToString());
		GameManager.Players.Remove((PlayerInfo)GameManager.Players.Where(i => i.Id == id).First<PlayerInfo>());
		var players = GetTree().GetNodesInGroup("Player");

		// look through all the players, remove the one who disconnected
		foreach(var item in players)
		{
			if(item.Name == id.ToString())
			{	
				item.QueueFree();
			}
		}

    }

	/// <summary>
	/// runs when a player connects and runs on all peers
	/// </summary>
	/// <param name="id">id of the player that connected</param>
    private void PeerConnected(long id)
    {
		GD.Print("player connected: " + id.ToString());
    }

	/// <summary>
	/// runs when the server disconnects, runs only emitted on clients
	/// </summary>
	/// <param name="id"></param>
	private void ServerDisconnected()
	{
		GameManager.Players.Clear();
		GD.Print("Lost connection to server!");
		// TODO: clear the game scene and load the menu?
	}

	// Functions ---------------------------------------------------------------------------------------------------------
	/// <summary>
	/// Starts the game, it loads the level and hides the multiplayer menu
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void startGame()
	{
		// Probably not needed but nice
		foreach (var item in GameManager.Players)
		{
			GD.Print(item.Name + " is playing");
		}
		var scene = ResourceLoader.Load<PackedScene>("res://scenes/test_world.tscn").Instantiate<Node>();
		GetTree().Root.AddChild(scene);
		Hide();
	}

	/// <summary>
	/// send player information to the game manager
	/// </summary>
	/// <param name="name"></param>
	/// <param name="id"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void sendPlayerInformation(string name, int id)
	{
		PlayerInfo playerInfo = new PlayerInfo(){
			Name = name,
			Id = id,
		};

		// Check to see if the GameManager already has the player by id.
		// Note: Players.Contains(playerInfo) did not work and duplicated players with 3 or more instances
		var playerInList = GameManager.Players.FirstOrDefault(i => i.Id == id);
		if (playerInList == null)
		{
			GameManager.Players.Add(playerInfo);
		}

		if (Multiplayer.IsServer())
		{
			foreach (var item in GameManager.Players)
			{
				Rpc("sendPlayerInformation", item.Name, item.Id);
			}
		}
	}

	/// <summary>
	/// Create a server and host the game
	/// </summary>
	private void hostGame()
	{
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, max_clients);
		if (error != Error.Ok)
		{
			GD.Print("failed to host game! : " + error.ToString());
			return;
		}
		peer.Host.Compress(compression_mode);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("waiting for players!");
	}

	// Button down signals -----------------------------------------------------------------------------------------------
	/// <summary>
	/// set up a server and then join it, registers host info
	/// </summary>
	public void _on_host_button_down()
	{
		if (hosting == false)
		{
			hosting = true;
			hostGame();
			sendPlayerInformation(GetNode<LineEdit>("Panel/NameEntry").Text, 1);
			GetNode<Label>("Panel/StatusText").Text = "You are hosting!";
			GetNode<Button>("Panel/Join").Disabled = true;
			GetNode<Button>("Panel/StartGame").Disabled = false;
		}
		else
		{
			hosting = false;
		}
	}

	/// <summary>
	/// create a client and join a game
	/// </summary>
	public void _on_join_button_down()
	{
		peer = new ENetMultiplayerPeer();
		string entered_address = GetNode<LineEdit>("Panel/IpEntry").Text;
		if (entered_address != null)
		{
			address = entered_address;
		}
		var error = peer.CreateClient(address, port);
		if (error != Error.Ok)
		{
			GD.Print("failed to join game! : " + error.ToString());
			return;
		}
		peer.Host.Compress(compression_mode);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("joining game!");
	}

	public void _on_start_game_button_down()
	{
		Rpc("startGame"); // syncs a start game event for all peers
	}

	public void _on_back_button_down()
	{
		var scene = ResourceLoader.Load<PackedScene>("res://scenes/ui/main_menu.tscn").Instantiate<Node>();
		GetTree().Root.AddChild(scene);
		Hide();
	}
}
