using Godot;
using System;

public partial class PlayerHealthBar : TextureProgressBar
{

  [Export] private Label _numberLabel;
  private Vector2 _startPos;
  public override void _Ready()
  {
    _startPos = Position;
  }


}
