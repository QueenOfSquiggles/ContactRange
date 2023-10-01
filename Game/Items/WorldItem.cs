using System;
using System.Threading.Tasks;
using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.Scenes.Character;
using Squiggles.Core.Scenes.Registration;

public partial class WorldItem : Node3D {

  [Export] private Item _item;
  public string ItemID => _item?.GetRegistryID() ?? "";
  [Export] private Sprite3D _sprite;

  public override void _Ready() {
    _sprite.Texture = _item?.Texture;
    WhatIsTextureLater();
  }

  private async void WhatIsTextureLater() {
    await Task.Delay(1000);
    Print.Debug($"{Name}'s sprite: {_sprite.Texture?.ResourcePath ?? "null"}");
  }

  public void HandleInteract() {
    if (GetTree().GetFirstNodeInGroup("player") is not Player player) { return; }
    var inv = player.GetComponent<InventoryManager>();
    if (inv is null) { return; }
    if (inv.TryAddItem(_item.GetRegistryID())) {
      CallDeferred(MethodName.QueueFree);
    }
  }

  public void SetItem(string itemID) {
    var itemRes = RegistrationManager.GetResource<Item>(itemID);
    if (itemRes is not null) {
      _item = itemRes;
    }
  }
}
