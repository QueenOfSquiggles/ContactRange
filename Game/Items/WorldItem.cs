using Godot;
using Squiggles.Core.Extension;
using Squiggles.Core.Scenes.Character;
using Squiggles.Core.Scenes.Registration;

public partial class WorldItem : Node3D {

  [Export] private string _itemID;
  [Export] private Sprite3D _sprite;

  public override void _Ready() {
    var item = RegistrationManager.GetResource<Item>(_itemID);
    if (item is not null) {
      _sprite.Texture = item.Texture;
    }
  }


  public void HandleInteract() {
    if (GetTree().GetFirstNodeInGroup("player") is not Player player) { return; }
    var inv = player.GetComponent<InventoryManager>();
    if (inv is null) { return; }
    if (inv.TryAddItem(_itemID, 1)) {
      QueueFree();
    }
  }

}
