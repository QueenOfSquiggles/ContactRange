using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Events;
using Squiggles.Core.Extension;
using Squiggles.Core.Scenes.Registration;
using System;
using System.Linq;

public partial class PlayerInventoryDisplay : VBoxContainer {

  [Export] private float _displayTime = 3.0f;
  private TextureRect[] _slots = Array.Empty<TextureRect>();
  private Tween _tween;
  private Vector2 _showPos;
  private Vector2 _hidePos;
  public override void _Ready() {
    EventBus.GUI.UpdatePlayerInventoryDisplay += HandleInventoryUpdate;
    _slots = GetChildren().Where((node) => node is TextureRect).Cast<TextureRect>().ToArray();
    foreach (var slot in _slots) {
      slot.Texture = null; // clear slots
    }
    _showPos = Position - new Vector2(Size.X, 0f);
    _hidePos = Position + new Vector2(Size.X, 0f);
    Position = _hidePos;
  }

  private void HandleInventoryUpdate(int slot, string item, int quantity) {
    Print.Debug($"Player inventory display update: slot #{slot} '{item}'");
    var data = RegistrationManager.GetResource<Item>(item);
    if (data is null) {
      Print.Debug($"Resource data was found null for Item(\"{item}\")");
    }
    if (!IsInstanceValid(_slots[slot])) { return; }
    _slots[slot].Texture = data?.Texture; // null prop means null item just clears the texture as well!
    Popup();
  }

  private void Popup() {
    _tween?.Kill();
    _tween = GetTree().CreateTween().SetSC4XStyle();
    _tween.TweenProperty(this, "position", _showPos, 0.1f);
    _tween.TweenInterval(_displayTime);
    _tween.TweenProperty(this, "position", _hidePos, 0.1f);
  }
}
