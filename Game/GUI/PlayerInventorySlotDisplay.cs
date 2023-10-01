using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.Scenes.Registration;
using System;

public partial class PlayerInventorySlotDisplay : Button {

  [Signal] public delegate void OnDropEventHandler(int slot, string item);
  [Signal] public delegate void OnUseEventHandler(int slot, string item);
  [Signal] public delegate void OnSelectedEventHandler(int slot);
  [Export] private Control _buttonsRoot;
  [Export] private Label _label;
  [Export] private Button _btnUse;

  private int _slot;
  private string _item = "";
  private Vector2 _btnsShow;
  private Vector2 _btnsHide;
  private Tween _tween;

  public override void _Ready() {
    SetItem("");
    Pressed += OnActivateSlot;
    _btnsShow = _buttonsRoot.Position;
    _btnsHide = _btnsShow + new Vector2(0f, _buttonsRoot.Size.Y);
    OnDeactivateSlot();
  }

  public void SetItem(string id, int slot = -1) {
    _item = id;
    if (slot >= 0) {
      _slot = slot;
    }
    Icon = null;
    _label.Text = "";

    if (id != "") {
      var item = RegistrationManager.GetResource<Item>(_item);
      if (item is not null) {
        Icon = item.Texture;
        _label.Text = item.Name;
        _btnUse.Visible = item.IsHealth;
      }
    }
    Disabled = id == "";
    OnDeactivateSlot();
  }

  public void OnActivateSlot() {
    foreach (var ch in _buttonsRoot.GetChildren()) {
      if (ch is Button btn) {
        btn.Disabled = false;
      }
    }
    _buttonsRoot.Visible = true;
    SlideButtons(_btnsShow);
    EmitSignal(nameof(OnSelected), _slot);
  }
  public void OnDeactivateSlot() {
    foreach (var ch in _buttonsRoot.GetChildren()) {
      if (ch is Button btn) {
        btn.Disabled = true;
      }
    }
    SlideButtons(_btnsHide, true);
  }

  private void SlideButtons(Vector2 dest, bool hideAtEnd = false) {
    _tween?.Kill();
    _tween = GetTree().CreateTween().SetSC4XStyle();
    _tween.TweenProperty(_buttonsRoot, "position", dest, 0.2f);
    if (hideAtEnd) {
      _tween.TweenCallback(Callable.From(() => _buttonsRoot.Visible = false));
    }
  }

  private void DoUse() => EmitSignal(nameof(OnUse), _slot, _item);

  // heh dewdrop
  private void DoDrop() => EmitSignal(nameof(OnDrop), _slot, _item);




}
