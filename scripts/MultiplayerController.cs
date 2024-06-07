using Godot;
using System;
using System.Linq;
// Ref FinePointCGI: https://www.youtube.com/watch?v=MOJVjFngOs4

public partial class MultiplayerController : Control
{
    [Export]
    private int _port = 8910;
    [Export]
    private string _address = "127.0.0.1"; 

    [Export]
    private int _maxClients = 4;

    private ENetMultiplayerPeer _peer;
    private ENetConnection.CompressionMode _compressionMode = ENetConnection.CompressionMode.Fastlz;

    private const string _localHost = "127.0.0.1";
    private bool _hosting = false;
    private bool _joining = false;

    private Label _reportingText;
    private Label _statusText;
    private Button _startGameButton;
    private Button _hostGameButton;
    private Button _joinGameButton;
    private LineEdit _ipEntryEdit;
    private LineEdit _nameEntryEdit;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Connecting signals for multiplayer
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;
        Multiplayer.ServerDisconnected += ServerDisconnected;

        _statusText = GetNode<Label>("Panel/StatusText");
        _reportingText = GetNode<Label>("Panel/ReportingText");
        _startGameButton = GetNode<Button>("Panel/StartGame");
        _hostGameButton = GetNode<Button>("Panel/Host");
        _joinGameButton = GetNode<Button>("Panel/Join");
        _ipEntryEdit = GetNode<LineEdit>("Panel/IpEntry");
        _nameEntryEdit = GetNode<LineEdit>("Panel/NameEntry");

        ClearTextFields();

        // if a dedicated server
        if (OS.GetCmdlineArgs().Contains("--server"))
        {
            HostGame();
        }
    }

    private void ClearTextFields()
    {
        _statusText.Text = "";
        _reportingText.Text = "";
    }

    // Multiplayer Event Signal Functions -------------------------------------
    /// <summary>
    /// runs when the connection fails and it runs only on the client
    /// </summary>
    private void ConnectionFailed()
    {
        GD.Print("connection failed!");
        ClearGameStatus();
    }

    /// <summary>
    /// runs when the connection is successful and only runs on the clients
    /// </summary>
    private void ConnectedToServer()
    {
        GD.Print("connected to server");
        // RPC ID makes the RPC to a specific ip. In this case, the server
        RpcId(1, "SendPlayerInformation", _nameEntryEdit.Text, Multiplayer.GetUniqueId());
    }

    /// <summary>
    /// runs when a player disconnects and runs on all _peers
    /// </summary>
    /// <param name="id">id of the player that disconnected</param>
    private void PeerDisconnected(long id)
    {
        GD.Print("player disconnected: " + id.ToString());
        GameManager.Players.Remove((PlayerInfo)GameManager.Players.Where(i => i.Id == id).First<PlayerInfo>());
        var players = GetTree().GetNodesInGroup("Players");

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
    /// runs when a player connects and runs on all _peers
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
        ClearGameStatus();
        // TODO: clear the game scene and load the menu?
    }

    // Functions --------------------------------------------------------------
    /// <summary>
    /// Starts the game, it loads the level and hides the multiplayer menu
    /// </summary>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartGame()
    {
        // Probably not needed but nice
        foreach (var item in GameManager.Players)
        {
            GD.Print(item.Name + " is playing");
        }
        var scene = ResourceLoader.Load<PackedScene>("res://scenes/levels/test_world_mp.tscn").Instantiate<Node>();
        GetTree().Root.AddChild(scene);
        Hide();
    }

    /// <summary>
    /// send player information to the game manager
    /// </summary>
    /// <param name="name"></param>
    /// <param name="id"></param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendPlayerInformation(string name, int id)
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
                Rpc("SendPlayerInformation", item.Name, item.Id);
            }
        }
    }

    /// <summary>
    /// Create a server and host the game
    /// </summary>
    private void HostGame()
    {
        _hosting = true;
        GameManager.IsMultiplayerGame = true;

        _peer = new ENetMultiplayerPeer();
        var error = _peer.CreateServer(_port, _maxClients);
        if (error != Error.Ok)
        {
            GD.Print("failed to host game! : " + error.ToString());
            _reportingText.Text = error.ToString();
            return;
        }
        _peer.Host.Compress(_compressionMode);
        Multiplayer.MultiplayerPeer = _peer;

        GD.Print("waiting for players!");
        SendPlayerInformation(_nameEntryEdit.Text, 1);

        // UI
        _reportingText.Text = "Waiting for players!";
        _statusText.Text = $"You are hosting! IP: {_address} Port: {_port}";
        _hostGameButton.Text = "Stop hosting";
        _joinGameButton.Disabled = true;
        _startGameButton.Disabled = false;
    }

    // Presently fails to close the server properly, trying to host again fails
    private void StopHostingGame()
    {
        GameManager.ClearStatus();
        _hosting = false;
        _peer.Close();

        // UI
        _reportingText.Text = "Stopped hosting";
        _statusText.Text = "";
        _hostGameButton.Text = "Host";
        _joinGameButton.Disabled = false;
        _startGameButton.Disabled = true;
    }

    private void JoinGame()
    {
        _peer = new ENetMultiplayerPeer();
        string entered_address = _ipEntryEdit.Text;
        _address = entered_address == "" ? _localHost : entered_address;
        GD.Print($"Joining with: {_address}");

        var error = _peer.CreateClient(_address, _port);
        if (error != Error.Ok)
        {
            GD.Print("failed to join game! : " + error.ToString());
            _reportingText.Text = error.ToString();
            return;
        }
        _peer.Host.Compress(_compressionMode);
        GameManager.IsMultiplayerGame = true;
        Multiplayer.MultiplayerPeer = _peer;
        GD.Print("joining game!");

        // UI
        _joining = true;
        _hostGameButton.Disabled = true;
        _joinGameButton.Text = "Stop Joining";
        _statusText.Text = $"Waiting for host to launch game! IP: {_address} Port: {_port}";
    }

    private void StopJoiningGame()
    {
        GameManager.ClearStatus();

        // UI
        _joining = false;
        _hostGameButton.Disabled = false;
        _joinGameButton.Text = "Join";
        _statusText.Text = "Stopped joining";
    }

    private void ClearGameStatus()
    {
        GameManager.ClearStatus();        
    }

    // Signals ----------------------------------------------------------------
    /// <summary>
    /// set up a server and then join it, registers host info
    /// </summary>
    public void _on_host_button_down()
    {
        switch (_hosting)
        {
            case true: StopHostingGame(); break;
            case false: HostGame(); break;
        }
    }

    /// <summary>
    /// create a client and join a game
    /// </summary>
    public void _on_join_button_down()
    {
        switch(_joining)
        {
            case true: StopJoiningGame(); break;
            case false: JoinGame(); break;
        }
    }

    public void _on_start_game_button_down()
    {
        Rpc("StartGame"); // syncs a start game event for all _peers
    }

    public void _on_back_button_down()
    {
        var scene = ResourceLoader.Load<PackedScene>("res://scenes/ui/main_menu.tscn").Instantiate<Node>();
        GameManager.IsMultiplayerGame = false;
        GetTree().Root.AddChild(scene);
        Hide();
    }
}
