using Godot;
using System;

public partial class HUD : CanvasLayer
{
	private Label _timerLabel;
	private float _elapsedTime = 0f;

	public override void _Ready()
	{
		_timerLabel = GetNode<Label>("TimerLabel");
	}

	public override void _Process(double delta)
	{
		_elapsedTime += (float)delta;

		// Format as MM:SS
		int minutes = (int)(_elapsedTime / 60);
		int seconds = (int)(_elapsedTime % 60);
		_timerLabel.Text = $"{minutes:D2}:{seconds:D2}";
	}

	// Optional: reset timer if player dies
	public void ResetTimer()
	{
		_elapsedTime = 0f;
	}
}
