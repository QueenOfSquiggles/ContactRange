using Godot;
using Squiggles.Core.FSM;
using System;

public partial class AlienStateHitRecover : State {

  [Export] private CharacterBody3D _actor;
  [Export] private float _speed = 0.5f;
  [Export] private float _recoveryTime = 1.0f;
  private Vector3 _direction;
  private float _timer;

  public override void EnterState() {
    SetPhysicsProcess(true);
    var player = GetTree().GetFirstNodeInGroup("player") as Node3D;
    _direction = _actor.GlobalPosition - player.GlobalPosition;
    _direction.Y = 0f;
    _direction = _direction.Normalized();
    _timer = _recoveryTime;

  }
  public override void ExitState() => SetPhysicsProcess(false);


  public override void _PhysicsProcess(double delta) {
    _actor.Velocity = _direction * _speed;
    _actor.MoveAndSlide();

    _timer -= (float)delta;
    if (_timer <= 0) {
      EmitSignal(nameof(OnStateFinished));
    }
  }
}
