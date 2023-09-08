using Godot;
using System;

public partial class pause_menu : menu
{
	menu_button continueButton;
	menu_button optionsButton;
	menu_button mainMenuButton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		main = GetParent<main>();

		continueButton = GetNode<menu_button>("ContinueButton");
		optionsButton = GetNode<menu_button>("OptionsButton");
		mainMenuButton = GetNode<menu_button>("MainMenuButton");

		continueButton.Text = "CONTINUE";
		continueButton.setButtonID("PauseContinue");

		optionsButton.Text = "OPTIONS";
		optionsButton.setButtonID("PauseOptions");

		mainMenuButton.Text = "EXIT";
		mainMenuButton.setButtonID("PauseExit", true);

		baseOffset = Offset;
		baseScale = Scale;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		setOffset();

		if (Input.IsActionJustPressed("pause"))
		{
			if (main.gameRunning && !main.GameOver)
			{
				togglePause();
			}
		}
	}
}
