using Godot;
using System;

public partial class StartEndScript : CanvasLayer
{
	private Button _startButton;
	private Button _quitButton;

	public override void _Ready()
	{
		// get start and quit buttons
		_startButton = GetNode<Button>("Start");
		_quitButton = GetNode<Button>("Quit");

		// connect button pressed signals
		_startButton.Pressed += _on_start_pressed;
		_quitButton.Pressed += _on_quit_pressed;
	}

	private void _on_start_pressed()
	{
		// defer loading game scene to avoid null reference
		CallDeferred(nameof(LoadGameScene));
	}

	private void LoadGameScene()
	{
		// change to game scene
		GetTree().ChangeSceneToFile("res://game.tscn");
	}

	private void _on_quit_pressed()
	{
		// quit game
		GetTree().Quit();
	}
}
