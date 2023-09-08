using Godot;
using System;

public partial class main_menu : menu
{
	menu_button startButton;
	menu_button optionsButton;
	menu_button quitButton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		main = GetParent<main>();

		startButton = GetNode<menu_button>("StartButton");
		optionsButton = GetNode<menu_button>("OptionsButton");
		quitButton = GetNode<menu_button>("QuitButton");

		startButton.Text = "START";
		startButton.setButtonID("MainStart");

		optionsButton.Text = "OPTIONS";
		optionsButton.setButtonID("MainOptions");

		quitButton.Text = "QUIT";
		quitButton.setButtonID("MainQuit", true);

		baseOffset = Offset;
		baseScale = Scale;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		setOffset();
	}
}
