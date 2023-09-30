using Godot;
using Squiggles.Core.Events;
using Squiggles.Core.FSM;

public partial class Player : CharacterBody3D {

  [ExportGroup("Player FSM", "_player")]
  [Export] private FiniteStateMachine _playerFSM;
  [Export] private State _playerStateExplore;
  [Export] private State _playerStateCombat;
  [Export] private State _playerStateCutscene;

  [ExportGroup("Camera FSM", "_camera")]
  [Export] private FiniteStateMachine _cameraFSM;
  [Export] private State _cameraStateIdle;
  [Export] private State _cameraStateMoving;
  [Export] private State _cameraCombat;

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
  }
}
