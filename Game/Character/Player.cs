using System;
using Godot;
using Squiggles.Core.CharStats;
using Squiggles.Core.Error;
using Squiggles.Core.Events;
using Squiggles.Core.Extension;
using Squiggles.Core.FSM;
using Squiggles.Core.Scenes.Character;
using Squiggles.Core.Scenes.Registration;
using Squiggles.Core.Scenes.Utility;

public partial class Player : CharacterBody3D {

  [Export] private Light3D _playerLight;
  [Export] private CharStatManager _stats;
  [Export] private InventoryManager _inventory;
  [Export] private Sprite3D _heldWeapon;
  [Export] private AudioStreamPlayer3D _hitSfx;
  [Export] private AudioStreamPlayer3D _itemPickupSfx;
  [Export] private AudioStreamPlayer3D _itemDropSfx;

  [ExportGroup("Player FSM", "_player")]
  [Export] private FiniteStateMachine _playerFSM;
  [Export] private State _playerStateExplore;
  [Export] private PlayerStateCombat _playerStateCombat;
  [Export] private State _playerStateCutscene;

  [ExportGroup("Camera FSM", "_camera")]
  [Export] private FiniteStateMachine _cameraFSM;
  [Export] private State _cameraStateIdle;
  [Export] private State _cameraStateMoving;
  [Export] private State _cameraStateCombat;
  [Export] private State _cameraStateCutscene;
  [Export(PropertyHint.File, "*.tscn")] private string _gameOverScene;

  private Tween _tween;
  private bool _hasDoneCombat;
  private bool _hasLooked;
  private bool _hasMoved;
  private bool _hasOpenedInventory;
  private bool _hasInteracted;

  public override void _Ready() {
    EventBus.Gameplay.RequestPlayerAbleToMove += (can_move) => {
      Print.Debug($"Player received 'request able to move' event. Requested: {(can_move ? "can move" : "hold still")}");
      if (can_move) {
        _playerFSM.ChangeState(_playerStateExplore);
        _cameraFSM.ChangeState(_cameraStateMoving);
      }
      else {
        _playerFSM.ChangeState(_playerStateCutscene);
        _cameraFSM.ChangeState(_cameraStateCutscene);
      }
    };
    _cameraStateIdle.OnStateFinished += () => _cameraFSM.ChangeState(_cameraStateMoving);
    _cameraStateMoving.OnStateFinished += () => _cameraFSM.ChangeState(_cameraStateIdle);

    Input.MouseMode = Input.MouseModeEnum.Captured;
    // dynamically react to changes in scene values.
    _stats.CreateDynamicStat("air", _stats.GetStat("AirMax"), _stats.GetStat("AirMax"), _stats.GetStat("AirRegen"), "AirMax", "AirRegen");

    _inventory.ResizeInventory(3);
    _inventory.MaxItemsPerSlot = 1;
    _heldWeapon.Texture = null;
    DoTweenLoop();
  }

  private void DoTweenLoop() {
    _tween?.Kill();
    _tween = GetTree().CreateTween();
    _tween.TweenInterval(5f);
    _tween.TweenCallback(Callable.From(() => {
      var msg = "";
      // priority high (first) to low (last)
      if (!_hasOpenedInventory) { msg = "Use the I key or the North button (controller) to open the inventory"; }
      if (!_hasInteracted) { msg = "Click, Press E, or use the south button (controller) to interact."; }
      if (!_hasMoved) { msg = "Use the WASD keys or the right joystick to move"; }
      if (!_hasLooked) { msg = "Use the mouse or left joystick to look around"; }
      if (msg != "") {
        // not all conditions met, do more tutorialization.
        EventBus.GUI.TriggerRequestAlert(msg);
        DoTweenLoop();
      }
      else {
        EventBus.GUI.TriggerRequestAlert(""); // request to clear alert pane
      }
    }));
  }

  public void OnEnterRoom(RoomManager roomManager) {
    if (!_hasDoneCombat) {
      EventBus.GUI.TriggerRequestAlert("attack with same as interact");
      _hasDoneCombat = true;
      _tween?.Kill();
      _tween = GetTree().CreateTween();
      _tween.TweenInterval(2.0f);
      _tween.TweenCallback(Callable.From(() => {
        EventBus.GUI.TriggerRequestAlert("");
        DoTweenLoop(); // just in case we addicentally interrupted
      }));
    }
    _cameraFSM.ChangeState(_cameraStateCombat);

    _playerStateCombat.ConsumeRoomData(roomManager);
    _playerFSM.ChangeState(_playerStateCombat);

    void reset() { // loving these micro callbacks
      _playerFSM.ChangeState(_playerStateExplore);
      _cameraFSM.ChangeState(_cameraStateMoving);
      roomManager.OnCombatOver -= reset;
    }

    roomManager.OnCombatOver += reset;
  }

  private Tween _lightTween;
  private void HandleStatChange(string name, float newValue) {
    if (name == "Health") {
      _hitSfx.Play();
      _lightTween?.Kill();
      _lightTween = GetTree().CreateTween().SetSC4XStyle();
      var lightEnergy = newValue / _stats.GetStat("MaxHealth") * 2.0f;
      _lightTween.TweenProperty(_playerLight, "light_energy", lightEnergy, 0.5f);
      if (newValue <= 0) {
        ShipManager.LastDeathMessage = "Killed by aliens! Try getting a med kit!";
        SceneTransitions.LoadSceneAsync(_gameOverScene, false, "FF0000");
      }
    }
  }

  private void HandleSlotUpdate(int slot, string item, int quantity) {
    if (item == "") {
      _itemDropSfx.Play();
    }
    else {
      _itemPickupSfx.Play();
    }
    var dmg = 0;
    Item best = null;
    _inventory.DoForSlots((slot, item, qty) => {
      var data = RegistrationManager.GetResource<Item>(item);
      if (data is not null && data.IsWeapon && data.DamageValue > dmg) {
        dmg = data.DamageValue;
        best = data;
      }
    });
    _heldWeapon.Texture = best?.Texture; // null prop so if no weapons, null texture

    EventBus.GUI.TriggerUpdatePlayeInventoryDisplay(slot, item, quantity);
  }

  public override void _Input(InputEvent @event) {
    if (@event is InputEventMouseMotion) {
      _hasLooked = true;
    }
    if (@event.IsAction("forwards") || @event.IsAction("forwards_joystick")) {
      _hasMoved = true;
    }
    if (@event.IsAction("open_inventory")) {
      _hasOpenedInventory = true;
    }
    if (@event.IsAction("interact")) {
      _hasInteracted = true;
    }
  }
}
