using Godot;
using Squiggles.Core.Scenes.Utility;

public partial class GameWonCutscene : Node3D {

  [Export(PropertyHint.File, "*.tscn")] private string _winScreenFile;

  public void GoToWinScreen() => SceneTransitions.LoadSceneAsync(_winScreenFile);
}
