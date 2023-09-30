namespace Squiggles.Core.Extension;

using System.Collections.Generic;
using System.Linq;
using Godot;
using Squiggles.Core.Interaction;

/// <summary>
/// An Area3D based collision detection system for detecting interactable objects. Useful for a variety of things. Mainly, a distance based interactable detector. So the closest available <see cref="IInteractable"/> that is detected by the Area3D is set to <see cref="CurrentInteraction"/>
/// </summary>
[GlobalClass]
public partial class InteractionSensor : Area3D {
  /// <summary>
  ///  Signal that is emitted when the current interactable object changes
  /// </summary>
  [Signal] public delegate void OnCurrentInteractionChangeEventHandler();

  /// <summary>
  /// A node that this should derive it's position from for distance based calculations
  /// </summary>
  [Export] private Node3D _derivedPosition;
  /// <summary>
  /// Whether or not to select <see cref="ISelectable"/> objects automatically.
  /// </summary>
  [Export] private bool _autoSelectObjects = false;

  /// <summary>
  /// The currently available interaction object, or null if none are present.
  /// </summary>
  public Node3D CurrentInteraction { get; private set; }

  private void OnAreaEnter(Area3D _) => RefreshCurrent();
  private void OnAreaExit(Area3D _) => RefreshCurrent();

  private void OnBodyEnter(Node3D _) => RefreshCurrent();
  private void OnBodyExit(Node3D _) => RefreshCurrent();

  private void RefreshCurrent() {
    // any time something valid enters/exits we check all overlapping. Using Linq to filter out false positives
    var options = new List<Node3D>();
    options.AddRange(GetOverlappingBodies());
    options.AddRange(GetOverlappingAreas());
    options = options.FindAll((Node3D n) => n is IInteractable inter && inter.GetIsActive()).ToList();

    if (options.Count <= 0) {
      if (CurrentInteraction == null) {
        return;
      }
      if (_autoSelectObjects && CurrentInteraction is ISelectable sel) {
        sel.OnDeselect();
      }
      CurrentInteraction = null;
      EmitSignal(nameof(OnCurrentInteractionChange));
    }
    else {
      if (_derivedPosition is null) {
        return;
      }

      var n_current = options[0];
      var dist = float.MaxValue;
      foreach (var n in options) {
        var d = (_derivedPosition.GlobalPosition - n.GlobalPosition).LengthSquared();
        if (d > dist) {
          continue;
        }

        dist = d;
        n_current = n;
      }
      if (CurrentInteraction == n_current) {
        return;
      }

      if (_autoSelectObjects && CurrentInteraction is ISelectable sel1) {
        sel1.OnDeselect();
      }

      CurrentInteraction = n_current;
      if (_autoSelectObjects && CurrentInteraction is ISelectable sel2) {
        sel2.OnSelect();
      }

      EmitSignal(nameof(OnCurrentInteractionChange));
    }

  }
}
