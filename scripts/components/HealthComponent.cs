using Godot;
using System;

public partial class HealthComponent : Node
{
    [Export]
    public float MaxHealth = 100;
    [Export]
    public float Health;
    [Export]
    public float IFramesTime = 1f;

    [Signal]
    public delegate void DeathSignalEventHandler();
    [Signal]
    public delegate void TookDamageEventHandler();
    [Signal]
    public delegate void HealedDamageEventHandler();
    [Signal]
    public delegate void IFramesExpiredEventHandler();

    public bool IFramesActive = false;
    private Timer _iFramesTimer; 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Health = MaxHealth;
        _iFramesTimer = new Timer();
        AddChild(_iFramesTimer);
        _iFramesTimer.WaitTime = IFramesTime;
        _iFramesTimer.OneShot = true;
        _iFramesTimer.Timeout += () => IFramesTimeout();
	}

    public void Damage(Attack attack)
    {
        if (IFramesActive == false)
        {
            _iFramesTimer.Start();
            IFramesActive = true;
            EmitSignal(SignalName.TookDamage);
            Health -= attack.Damage;

            if (Health <= 0)
            {
                Health = 0;
                EmitSignal(SignalName.DeathSignal);
            }
        }
    }

    public void Heal(float amount)
    {
        EmitSignal(SignalName.HealedDamage);
        Health = Mathf.Clamp(Health+=amount, 0, MaxHealth);
    }

    public void IFramesTimeout()
    {
        IFramesActive = false;
        EmitSignal(SignalName.IFramesExpired);
    }
}
