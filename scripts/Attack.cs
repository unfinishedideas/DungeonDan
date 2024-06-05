using System;
using Godot;

public class Attack 
{
   [Export]
   public float Damage = 10f;

   [Export]
   public float KnockbackForce = 5f;

   public Attack(){}

   public Attack(float damage, float knockback)
   {
       Damage = damage;
       KnockbackForce = knockback;
   }
}
