using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.FSM;
using Squiggles.Core.Scenes.Utility.Camera;

public partial class PlayerStateExplore : State {

  [Export] private VirtualCamera _vcam;
  [Export] private CharacterBody3D _actor;

  [ExportGroup("Movement", "_movement")]
  [Export] private float _movementSpeed = 4.0f;
  [Export] private float _movementAcceleration = 2.0f;
  [Export] private float _movementDeceleration = 1.0f;

  private Vector2 _inputVector;

  public override void EnterState() => SetPhysicsProcess(true);
  public override void ExitState() => SetPhysicsProcess(false);

  public override void _PhysicsProcess(double delta) {
    var moveIntent = Input.GetVector("left", "right", "backwards", "forwards") // keyboard
        + Input.GetVector("left_joystick", "right_joystick", "backwards_joystick", "forwards_joystick"); // gamepad
    if (moveIntent.LengthSquared() < 0.1f && _actor.Velocity.LengthSquared() < 0.1f) {
      return; // do  no motion.
    }


    var moveDir = new Vector3();
    moveDir += _vcam.GlobalTransform.Forward() * moveIntent.Y;
    moveDir += _vcam.GlobalTransform.Right() * moveIntent.X;
    moveDir.Y = 0;
    if (moveDir.LengthSquared() > 1.0f) {
      moveDir = moveDir.Normalized();
    }

    var similarity = (moveDir.Dot(_actor.Velocity.Normalized()) * 0.5f) + 0.5f;
    var lerpFactor = Mathf.Lerp(_movementDeceleration, _movementAcceleration, similarity);
    _actor.Velocity = _actor.Velocity.Lerp(moveDir * _movementSpeed, lerpFactor * (float)delta);
    _actor.MoveAndSlide();
  }

}
