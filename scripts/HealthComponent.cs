using Godot;
using System;

public partial class HealthComponent : Node
{
    [Export]
    public float MaxHealth = 100;
    private float _health;

    [Signal]
    public delegate void DeathSignalEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        _health = MaxHealth;
	}

    public void Damage(Attack attack)
    {
        _health -= attack.Damage;

        if (_health <= 0)
        {
            EmitSignal(SignalName.DeathSignal);
        }
    }
}
