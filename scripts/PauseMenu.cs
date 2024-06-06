using Godot;
using System;

public partial class PauseMenu : Control
{
    [Signal]
    public delegate void ResumeGameEventHandler();
    [Signal]
    public delegate void QuitGameEventHandler();

    // Signals ----------------------------------------------------------------
    public void _on_resume_button_button_down()
    {
        EmitSignal(SignalName.ResumeGame);
    }

    public void _on_quit_button_button_down()
    {
        EmitSignal(SignalName.QuitGame);
    }
}
