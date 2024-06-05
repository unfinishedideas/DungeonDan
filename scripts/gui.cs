using Godot;
using System;

public partial class gui : Control
{
    [Export]
    public Label DebugLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        DebugLabel = GetNode<Label>("./debug_container/debug_label");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
