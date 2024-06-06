using Godot;
using System;

public partial class HealthComponent : Node
{
    [Export]
    public float MaxHealth = 100;
    public float Health;

    [Signal]
    public delegate void DeathSignalEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Health = MaxHealth;
	}

    public void Damage(Attack attack)
    {
        Health -= attack.Damage;

        if (Health <= 0)
        {
            EmitSignal(SignalName.DeathSignal);
        }
    }
}
