using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.Scenes.Character;
using Squiggles.Core.Scenes.Interactables;
using System;

public partial class SensorArrayInterface : Node3D {


  private InteractiveTrigger _interactions;
  public override void _Ready() {
    _interactions = this.GetComponent<InteractiveTrigger>();
    _interactions.IsActive = !ShipManager.GetFlagFor("sensor_array_active");
  }

  public void OnInteract() {
    if (GetTree().GetFirstNodeInGroup("player")?.GetComponent<InventoryManager>() is not InventoryManager items) { return; }
    var slot = -1;
    items.DoForSlots((index, item, qty) => {
      if (item == "Sensor Reconfiguration Disc") {
        slot = index;
      }
    });
    if (slot < 0) {
      Print.Debug("Sensor array failed to find sensor array disk");
      return;
    }
    items.SetSelection(slot);
    if (items.RemoveItem()) {
      ShipManager.UpdateFlag("sensor_array_active", true);
      _interactions.IsActive = false;
      Print.Debug("Sensor array is now online!!!");
    }
  }

}
