using Godot;
using System;

public partial class death_screen : menu
{
	Label timerLabel;
	Label killsLabel;
	Label lvlLabel;

	menu_button restartButton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		hud = GetParent<hud>();
		main = hud.GetParent<main>();

		timerLabel = GetNode<Label>("TimerLabel");
		killsLabel = GetNode<Label>("KillsDisplay/KillsLabel");
		lvlLabel = GetNode<Label>("LevelDisplay/LevelLabel");

		restartButton = GetNode<menu_button>("RestartButton");
		restartButton.Text = "RESTART";
		restartButton.setButtonID("DeathRestart");

		baseOffset = Offset;
		baseScale = Scale;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		setOffset();
		
		timerLabel.Text = String.Format("{0:mm\\:ss}", main.GameTimer);
		killsLabel.Text = "KILLS: " + main.playerKills;
		lvlLabel.Text = "LEVEL: " + main.PTotalLevel;
	}
}
