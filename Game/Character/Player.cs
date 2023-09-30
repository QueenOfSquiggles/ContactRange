using System;
using Godot;
using Squiggles.Core.CharStats;
using Squiggles.Core.Events;
using Squiggles.Core.Extension;
using Squiggles.Core.FSM;

public partial class Player : CharacterBody3D {

  [Export] private Light3D _playerLight;
  [Export] private CharStatManager _stats;

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
    _stats.CreateDynamicStat("air", 10.0f, 10.0f, 1f, "AirMax", "AirRegen");
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
    }
  }
}
