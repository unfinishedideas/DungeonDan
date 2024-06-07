using Godot;
using System;

public partial class killzone : Area3D
{
    private Timer _timer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
    }

    // SIGNALS
    public void _on_body_entered(Node3D body)
    {
        _timer.Start();
    }

    public void _on_timer_timeout()
    {
        GD.Print("Killzone time out");
    }
}
