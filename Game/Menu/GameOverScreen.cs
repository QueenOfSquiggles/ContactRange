using Godot;
using Squiggles.Core.Data;
using Squiggles.Core.Error;
using Squiggles.Core.Scenes;
using Squiggles.Core.Scenes.Utility;
using System;

public partial class GameOverScreen : Control {
  [Export(PropertyHint.File, "*.tscn")] private string _mainMenu;
  [Export] private Label _deathMessage;

  public override void _Ready() {
    Input.MouseMode = Input.MouseModeEnum.Visible;
    _deathMessage.Text = ShipManager.LastDeathMessage;
  }

  public void DoTryAgain() {
    var file = ThisIsYourMainScene.Config.PlayScene;
    SceneTransitions.LoadSceneAsync(file, false, "lime");
  }

  public void ReturnMainMenu() {
    SaveData.CurrentSaveSlot.DeleteSaveSlot(); // clear old data for a fresh start
    var file = _mainMenu;
    SceneTransitions.LoadSceneAsync(file, false, "lime");
  }
}
