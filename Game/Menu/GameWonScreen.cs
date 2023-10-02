using Godot;
using Squiggles.Core.Scenes.Utility;
using System;

public partial class GameWonScreen : Control {

  public override void _Ready() => Input.MouseMode = Input.MouseModeEnum.Visible;

  public void LoadMainMenu() => SceneTransitions.LoadSceneAsync("res://Core/Scenes/UI/Menus/main_menu.tscn");
}
