using Godot;
using Squiggles.Core.Data;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.FSM;
using Squiggles.Core.Scenes.Utility.Camera;

public partial class StateCameraControls : State {

  [Export] private CharacterBody3D _actor;
  [Export] private Node3D _spritesRoot;
  [Export] private VirtualCamera _vcam;
  [Export] private Node3D _rotationY;
  [Export] private Node3D _rotationX;
  [Export] private float _extraSensitivityScalar = 1.0f;
  [Export] private float _maxAngleDegrees = 70.0f;
  [Export] private float _minAngleDegrees = -70.0f;
  [Export] private bool _isActiveDuringMovement = false;
  [Export] private float _movementThreshold = 0.2f;

  private const float CAM_SENSITIVITY_SCALAR = 0.0003f;

  public override void EnterState() {
    SetProcess(true);
    var camForward = _vcam.GlobalTransform.Forward();
    camForward.Y = 0;
    _spritesRoot.LookAt(_spritesRoot.GlobalPosition + camForward, Vector3.Up);
  }

  public override void ExitState() => SetProcess(false);

  public override void _Process(double delta) {
    var actorVel = _actor.Velocity.LengthSquared();
    if ((_isActiveDuringMovement && actorVel < _movementThreshold)
        || (!_isActiveDuringMovement && actorVel > _movementThreshold)) {
      EmitSignal(nameof(OnStateFinished));
      return;
    }

    var amount = Input.GetVector("joystick_look_left", "joystick_look_right", "joystick_look_down", "joystick_look_up") * CAM_SENSITIVITY_SCALAR * Controls.Instance.ControllerLookSensitivity * _extraSensitivityScalar;
    if (amount.LengthSquared() < 0.2f) {
      return; // assume joystick noise
    }
    _rotationY.RotateY(amount.X);

    var rot = _rotationX.Rotation;
    rot.X = Mathf.Clamp(amount.Y + rot.X, Mathf.DegToRad(_minAngleDegrees), Mathf.DegToRad(_maxAngleDegrees));
    _rotationX.Rotation = rot;
  }

  public override void _UnhandledInput(InputEvent @event) {
    if (!IsActive) {
      return;
    }
    if (@event is InputEventMouseMotion iemm) {
      DoCameraLook(-iemm.Relative * CAM_SENSITIVITY_SCALAR * Controls.Instance.MouseLookSensivity * _extraSensitivityScalar);
      this.HandleInput();
    }
  }

  private void DoCameraLook(Vector2 amount) {
    _rotationY.RotateY(amount.X);

    var rot = _rotationX.Rotation;
    rot.X = Mathf.Clamp(amount.Y + rot.X, Mathf.DegToRad(_minAngleDegrees), Mathf.DegToRad(_maxAngleDegrees));
    _rotationX.Rotation = rot;
  }


}
