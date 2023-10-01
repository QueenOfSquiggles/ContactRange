using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.Scenes.Character;
using Squiggles.Core.Scenes.Registration;

public partial class WorldItem : Node3D {

  [Export] public string ItemID;
  [Export] private Sprite3D _sprite;

  public override void _Ready() {
    Print.Debug($"{Name} loading item data for '{ItemID}'");
    var item = RegistrationManager.GetResource<Item>(ItemID);
    Print.Debug($"{Name} item data: '{item?.Name}' {item?.Texture?.ResourcePath}");
    _sprite.Texture = item?.Texture;
  }


  public void HandleInteract() {
    if (GetTree().GetFirstNodeInGroup("player") is not Player player) { return; }
    var inv = player.GetComponent<InventoryManager>();
    if (inv is null) { return; }
    if (inv.TryAddItem(ItemID)) {
      CallDeferred(MethodName.QueueFree);
    }
  }

}
