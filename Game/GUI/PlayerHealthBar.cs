using Godot;
using Squiggles.Core.CharStats;
using Squiggles.Core.Extension;
using System;

public partial class PlayerHealthBar : TextureProgressBar {

  [Export] private Label _numberLabel;
  [Export] private CharStatManager _stats;
  [Export] private float _responseTime = 0.1f;
  [Export] private float _holdTime = 3.0f;
  [Export] private string _valueKey;
  [Export] private string _valueMaxKey;
  [Export] private bool _pollForDynStat = false;
  private Vector2 _startPos;
  private Vector2 _hiddenPos;

  private float _lastPolledValue;

  private Tween _tween;
  public override void _Ready() {
    _startPos = Position - new Vector2(0, Size.Y);
    _hiddenPos = Position + new Vector2(0, Size.Y);
    Position = _hiddenPos;
    _stats.OnStatChange += HandleStatChange;
    if (_pollForDynStat) {
      SetProcess(true);
      _lastPolledValue = _stats.GetStat(_valueKey);
    }
    else {
      SetProcess(true);
    }
  }

  public override void _Process(double delta) {
    var current = _stats.GetStat(_valueKey);
    if (Mathf.Abs(current - _lastPolledValue) > 0.1f) {
      _lastPolledValue = current;
      HandleStatChange(_valueKey, current);
    }
  }

  public void HandleStatChange(string key, float value) {
    if (key != _valueKey) { return; }
    var maxVal = _stats.GetStat(_valueMaxKey);
    TweenToNewValue(value / maxVal);
  }

  private void TweenToNewValue(float nVal) {
    _tween?.Kill();
    _tween = GetTree().CreateTween().SetSC4XStyle();
    _tween.TweenProperty(this, "position", _startPos, _responseTime / 2.0f);
    _tween.Parallel().TweenMethod(Callable.From<float>(SetValue), Value, nVal, _responseTime);
    _tween.TweenInterval(_holdTime);
    _tween.TweenProperty(this, "position", _hiddenPos, _responseTime / 2.0f);
  }

  private void SetValue(float valuePercent) {
    _numberLabel.Text = $"{valuePercent * 100f:N0}%";
    Value = valuePercent;
  }


}
