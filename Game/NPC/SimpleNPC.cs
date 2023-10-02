using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Events;

public partial class SimpleNPC : Node3D {
  [Export] private Texture2D _spriteTexture;
  [Export] private string _dialogTrack;

  [Export] private Sprite3D _sprite;

  private Node _dialogic;
  public override void _Ready() {
    _dialogic = GetNode("/root/Dialogic");
    _sprite.Texture = _spriteTexture;
  }

  public void OnInteract() {
    Print.Debug($"{Name} starting fialog track: {_dialogTrack}");
    EventBus.Gameplay.TriggerRequestPlayerAbleToMove(false);
    _dialogic.Call("start", _dialogTrack);
    _dialogic.Connect("timeline_ended", Callable.From(() => EventBus.Gameplay.TriggerRequestPlayerAbleToMove(true)), (uint)ConnectFlags.OneShot);
  }
}
