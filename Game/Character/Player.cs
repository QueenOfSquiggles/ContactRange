using Godot;
using Squiggles.Core.FSM;

public partial class Player : CharacterBody3D {

  [ExportGroup("Player FSM", "_player")]
  [Export] private FiniteStateMachine _playerFSM;
  [Export] private State _playerStateExplore;
  [Export] private State _playerStateCombat;
  [Export] private State _playerStateCutscene;

  [ExportGroup("Camera FSM", "_camera")]
  [Export] private FiniteStateMachine _cameraFSM;
  [Export] private State _cameraIdle;
  [Export] private State _cameraMoving;
  [Export] private State _cameraCombat;

  public override void _Ready() {

  }
}
