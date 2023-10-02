using Godot;

public partial class CutsceneInstancer : Area3D {
  [Export] private PackedScene _cutscene;

  [Export] private bool _startActive;
  private bool _isActive;

  public override void _Ready() {
    BodyEntered += (body) => {
      if (_isActive && body.IsInGroup("player")) {
        var node = _cutscene.Instantiate();
        AddChild(node);
        _isActive = false;
      }
    };
    if (_startActive) {
      _isActive = true;
    }
  }


  public void SetActive() => _isActive = true;
}
