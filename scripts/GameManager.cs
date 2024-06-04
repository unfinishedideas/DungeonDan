using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
	public static List<PlayerInfo> Players = new List<PlayerInfo>();
	public static bool IsMultiplayerGame = false;
	public static bool DebugMode = true;
}
