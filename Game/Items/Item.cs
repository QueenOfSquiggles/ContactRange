using Godot;
using Squiggles.Core.Data;
using System;

[GlobalClass]
public partial class Item : Resource, IRegistryID {
  [Export] public string Name;
  [Export] public Texture2D Texture;
  [Export] public bool IsWeapon;
  [Export] public bool IsHealth;
  [Export] public int DamageValue;
  [Export] public int HealValue;

  public string GetRegistryID() => Name;
}
