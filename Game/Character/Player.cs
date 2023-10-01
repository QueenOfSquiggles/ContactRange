using System;
using Godot;
using Squiggles.Core.CharStats;
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
  [Export(PropertyHint.File, "*.tscn")] private string _gameOverScene;

  public override void _Ready() {
    EventBus.Gameplay.RequestPlayerAbleToMove += (can_move) => {
      if (can_move) {
        _playerFSM.ChangeState(_playerStateExplore);
      }
      else {
        _playerFSM.ChangeState(_playerStateCutscene);
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
  }

  public void OnEnterRoom(RoomManager roomManager) {
    _cameraFSM.ChangeState(_cameraStateCombat);

    _playerStateCombat.ConsumeRoomData(roomManager);
    _playerFSM.ChangeState(_playerStateCombat);

    void reset() { // loving these micro callbacks
      _cameraFSM.ChangeState(_cameraStateMoving);
      _playerFSM.ChangeState(_playerStateExplore);
      roomManager.OnCombatOver -= reset;
    }

    roomManager.OnCombatOver += reset;
  }

  private Tween _lightTween;
  private void HandleStatChange(string name, float newValue) {
    if (name == "Health") {
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
    if (item != "") {
      var data = RegistrationManager.GetResource<Item>(item);
      if (data.IsWeapon) {
        _heldWeapon.Texture = data.Texture;
      }
    }
    EventBus.GUI.TriggerUpdatePlayeInventoryDisplay(slot, item, quantity);
  }
}
