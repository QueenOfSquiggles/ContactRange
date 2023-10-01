using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.Scenes.Character;
using Squiggles.Core.Scenes.Interactables;
using System;

public partial class ThrusterTurbine : Node3D {
  [Export] private GpuParticles3D[] _fireParticles = Array.Empty<GpuParticles3D>();

  [ExportGroup("Flag Keys", "_flagName")]
  [Export] private string _flagNameFire;
  [Export] private string _flagNameFuel;

  [ExportGroup("Item Names", "_itemName")]
  [Export] private string _itemNameFuel;
  [Export] private string _itemNameFire;

  private bool _isExtinguished;
  private bool _isRefueled;
  private InteractiveTrigger _interaction;
  private Label3D _label;

  public override void _Ready() {
    _interaction = this.GetComponent<InteractiveTrigger>();
    _label = this.GetComponent<Label3D>();
    _label.Text = "";
    RefreshFlags();
  }

  private void RefreshFlags() {
    _isExtinguished = ShipManager.GetFlagFor(_flagNameFire);
    _isRefueled = ShipManager.GetFlagFor(_flagNameFuel);
    _interaction.IsActive = !(_isExtinguished && _isRefueled);
    _label.Visible = _interaction.IsActive;
    foreach (var particle in _fireParticles) {
      particle.Emitting = !_isExtinguished;
    }

    _label.Text = _isExtinguished ?
      (_isRefueled ?
        ""
        : "Fuel Required")
      : "Extinguish Fire";
  }

  public void OnInteract() {
    if (!_isExtinguished && TryConsumeItem(_itemNameFire)) {
      Print.Debug("Checking for extinguishing fire");
      ShipManager.UpdateFlag(_flagNameFire, true);
      foreach (var particle in _fireParticles) {
        particle.Emitting = false;
      }
      RefreshFlags();
    }
    else if (!_isRefueled && TryConsumeItem(_itemNameFuel)) {
      Print.Debug("Checking for refueling");
      ShipManager.UpdateFlag(_flagNameFuel, true);
      RefreshFlags();
    }
    else {
      Print.Warn("Unhandled edge case", this);
    }

  }

  private bool TryConsumeItem(string itemName) {
    if (GetTree().GetFirstNodeInGroup("player")?.GetComponent<InventoryManager>() is not InventoryManager items) {
      Print.Warn("failed to get player inventory manager");
      return false;
    }
    var slot = -1;
    items.DoForSlots((index, item, qty) => {
      if (item == itemName) {
        slot = index;
      }
    });
    if (slot < 0) {
      Print.Warn("Sensor array failed to find sensor array disk");
      return false;
    }
    items.SetSelection(slot);
    var result = items.RemoveItem();
    Print.Debug($"Tried to consume item {itemName} from player. Was successful? {result}");
    return result;
  }

}
