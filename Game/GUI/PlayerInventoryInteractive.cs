using System;
using Godot;
using Squiggles.Core.CharStats;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.Scenes.Character;
using Squiggles.Core.Scenes.Registration;

public partial class PlayerInventoryInteractive : Control {
  [Export] private PlayerInventorySlotDisplay[] _slots;

  private InventoryManager _inventory;
  public override void _Ready() {
    // Testing code
    // #if DEBUG
    //     // registers items for single scene testing
    //     RegistrationManager.RegisterRegistryType("Item", "res://Game/Registry/Item/");
    //     RegistrationManager.ReloadRegistries();
    // #endif

    // assigns index and clears value
    _slots[0].SetItem("", 0);
    _slots[1].SetItem("", 1);
    _slots[2].SetItem("", 2);

    VisibilityChanged += () => {
      if (Visible) {
        foreach (var slot in _slots) {
          slot.OnDeactivateSlot();
        }
      }
    };

    foreach (var slot in _slots) {
      slot.OnDrop += (slot, item) => {
        DropItem(item);
        _inventory.RemoveItem();
        _slots[slot].SetItem("");
      };
      slot.OnUse += (slot, item) => {
        ConsumeHealth(item);
        _inventory.RemoveItem();
        _slots[slot].SetItem("");
      };
      slot.OnSelected += (slot) => {
        _inventory.SetSelection(slot);
        for (var i = 0; i < _slots.Length; i++) {
          if (i != slot) {
            _slots[i].OnDeactivateSlot();
          }
        }
      };
    }
  }

  private void DropItem(string itemID) {
    if (GetTree().GetFirstNodeInGroup("player") is not Player player) { return; }
    var worldItem = GD.Load("res://Game/Items/world_item.tscn") as PackedScene;
    var item = worldItem.Instantiate() as WorldItem;
    item.SetItem(itemID);
    player.GetParent().AddChild(item);
    var random = new Random();
    var dist = 1.5f;
    item.GlobalPosition = player.GlobalPosition + new Vector3(random.NextGuass() * dist, 0, random.NextGuass() * dist);
  }

  private void ConsumeHealth(string itemID) {
    var item = RegistrationManager.GetResource<Item>(itemID);
    var playerStats = GetTree().GetFirstNodeInGroup("player")?.GetComponent<CharStatManager>();
    if (playerStats is null) { return; }

    playerStats.ModifyStaticStat("Health", item.HealValue);
    if (playerStats.GetStat("Health") > playerStats.GetStat("MaxHealth")) {
      playerStats.SetStaticStat("Health", playerStats.GetStat("MaxHealth"));
    }

  }

  public void ReadInventory(InventoryManager inventory) {
    _inventory = inventory;
    inventory.DoForSlots((slot, id, qty) => _slots[slot].SetItem(id ?? ""));
  }
}
