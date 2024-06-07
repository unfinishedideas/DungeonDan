using Godot;
using System;

public partial class HealthComponent : Node
{
    [Export]
    public float MaxHealth = 100;
    [Export]
    public float Health;

    [Signal]
    public delegate void DeathSignalEventHandler();
    [Signal]
    public delegate void TookDamageEventHandler();
    [Signal]
    public delegate void HealedDamageEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Health = MaxHealth;
	}

    public void Damage(Attack attack)
    {
        EmitSignal(SignalName.TookDamage);
        Health -= attack.Damage;

        if (Health <= 0)
        {
            Health = 0;
            EmitSignal(SignalName.DeathSignal);
        }
    }

    public void Heal(float amount)
    {
        EmitSignal(SignalName.HealedDamage);
        Health = Mathf.Clamp(Health+=amount, 0, MaxHealth);
    }
}
