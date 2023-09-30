using Godot;
using Squiggles.Core.Interaction;

public partial class ShipDoor : Node3D, IInteractable, ISelectable {

  [Export] private AnimationPlayer _anim;
  [Export] private MeshInstance3D _selectionMesh;


  public string GetActiveName() => "Door";
  public bool GetIsActive() => true;
  public bool Interact() { _anim.Play("DoorOpen"); return true; }
  public void OnDeselect() => _selectionMesh.Visible = false;
  public void OnSelect() => _selectionMesh.Visible = true;
}
