namespace Squiggles.Core.Scenes.UI.Menus;

using System;
using System.Threading.Tasks;
using Godot;
using Squiggles.Core.Events;
using Squiggles.Core.Extension;
using Squiggles.Core.Scenes.Utility;

/// <summary>
/// The pause menu, which is used during gameplay is a "pause_menu_controller" is used (res://Core/Scenes/UI/Menus/pause_menu_controller.tscn). It pauses the game using the scene tree pause feature. It contains options for saving and loading as well as the same options menu as in the main menu. It also blurs the background using a weak box blur effect.
/// </summary>
public partial class PauseMenu : Control {

  /// <summary>
  /// The path to the main menu file
  /// </summary>
  [Export(PropertyHint.File, "*.tscn")] private string _mainMenuFile = "";
  /// <summary>
  /// The path to the options menu file
  /// </summary>
  [Export(PropertyHint.File, "*.tscn")] private string _optionsMenuFile = "";

  /// <summary>
  /// The menu panel
  /// </summary>
  [Export] private Control _menuPanel;
  /// <summary>
  /// The current panel that has popped up
  /// </summary>
  private Control _currentPopup;
  /// <summary>
  /// The controls which are related to games with multiple save slots. (Removed if `<see cref="SaveSlotSettings.SlotOptions"/> != MULTI_SLOT_SAVE_DATA`)
  /// </summary>
  [Export] private Control[] _saveSlotRelatedElements = Array.Empty<Control>();
  /// <summary>
  /// The control related to loading the last save. Removed if the configration disables loading the last save while playing.
  /// </summary>
  [Export] private Control _loadLastSaveControl;

  public override void _Ready() {
    GetTree().Paused = true;
    Input.MouseMode = Input.MouseModeEnum.Visible;
    if (ThisIsYourMainScene.Config?.SaveSlotHandlingSettings?.SlotOptions == Meta.SaveSlotSettings.SaveSlotOptions.NO_SAVE_DATA) {
      // if no save slot chosen, remove the save data related buttons
      foreach (var c in _saveSlotRelatedElements) {
        c?.QueueFree();
      }
    }
    if (ThisIsYourMainScene.Config?.SaveSlotHandlingSettings?.AllowPlayersToReloadLastSave is false) {
      _loadLastSaveControl?.QueueFree();
    }

  }

  public override void _UnhandledInput(InputEvent @event) {
    if (@event.IsActionPressed("ui_cancel")) {

      ReturnToPlay();
      this.HandleInput();
    }
  }

  private async void ReturnToPlay() {
    EventBus.Data.TriggerSerializeAll();
    await Task.Delay(10);
    Input.MouseMode = ThisIsYourMainScene.Config?.GameplayConfig?.GameplayMouseMode ?? Input.MouseModeEnum.Captured;
    GetTree().Paused = false;
    QueueFree();
  }

  private async void ExitToMainMenu() {
    EventBus.Data.TriggerSerializeAll();
    await Task.Delay(10);
    GetTree().Paused = false;
    SceneTransitions.LoadSceneAsync(_mainMenuFile);
  }

  private void OnBtnOptions() {
    if (_currentPopup is null || !IsInstanceValid(_currentPopup)) {
      CreateNewSlidingScene(_optionsMenuFile);
    }
    else {
      _currentPopup?.GetComponent<SlidingPanelComponent>()?.RemoveScene();
    }

  }
  private void OnBtnSave() {
    EventBus.Data.TriggerSerializeAll();
    var _ = Name; // force access instance data
  }

  private void OnBtnReloadLastSave() {
    EventBus.Data.TriggerReload();
    var _ = Name; // force access instance data
  }

  private void CreateNewSlidingScene(string scene_file) {
    var packed = GD.Load<PackedScene>(scene_file);
    var scene = packed?.Instantiate<Control>();
    if (scene is null) {
      return;
    }
    scene.GlobalPosition = new Vector2(_menuPanel?.Size.X ?? 0, 0);
    AddChild(scene);
    _currentPopup = scene;
  }
}
