using Godot;
using System;

public partial class options_menu : menu
{
	menu_button difficultyButton;
	menu_button resolutionButton;
	menu_button aaButton;

	menu_button vSyncButton;
	menu_button backButton;

	fancy_slider shakeSlider;

	string[] difficulties;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		main = GetParent<main>();

		difficultyButton = GetNode<menu_button>("DifficultyButton");
		resolutionButton = GetNode<menu_button>("ResolutionButton");
		aaButton = GetNode<menu_button>("AntiAliasingButton");
		vSyncButton = GetNode<menu_button>("VSyncButton");
		backButton = GetNode<menu_button>("BackButton");
		shakeSlider = GetNode<fancy_slider>("ShakeSlider");

		difficulties = new string[3]{ "EASY", "MEDIUM", "HARD" };

		difficultyButton.setButtonID("OptionsDifficulty");
		resolutionButton.setButtonID("OptionsResolution");
		aaButton.setButtonID("OptionsAA");

		backButton.Text = "BACK";
		backButton.setButtonID("OptionsBack", true);

		vSyncButton.setButtonID("OptionsVsync");

		baseOffset = Offset;
		baseScale = Scale;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		difficultyButton.Text = difficulties[(int)main.curDifficulty];
		resolutionButton.Text = main.curResolution.longName;
		aaButton.Text = curAA.name;

		vSyncButton.Text = vSync;

		main.shakeMult = (float)shakeSlider.Value / 50;

		setOffset();
	}
}
