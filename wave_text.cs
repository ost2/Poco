using Godot;
using System;

public partial class wave_text : menu
{
	Label waveNumLabel;
	Label subLabel;

	public Timer hideTimer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		hud = GetParent<hud>();
		main = hud.GetParent<main>();

		waveNumLabel = GetNode<Label>("WaveNumberLabel");
		subLabel = GetNode<Label>("SubLabel");

		hideTimer = GetNode<Timer>("Timer");

		baseOffset = Offset;
		baseScale = Scale;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		setOffset();
		
		switch (main.curWaveType)
		{
			case main.waveType.enemyWave:
			{
				waveNumLabel.Text = "WAVE: " + main.waveNumber;
				subLabel.Text = main.killsToProgress + " ENEMIES";
				break;
			}
			case main.waveType.bossWave:
			{
				waveNumLabel.Text = "WAVE: " + main.waveNumber;
				subLabel.Text = "BOSS WAVE";
				break;
			}
			case main.waveType.boxWave:
			{
				waveNumLabel.Text = "WAVE " + main.waveNumber + " COMPLETE";
				subLabel.Text = "GET BOXES";
				break;
			}
		}
		var baseCol = waveNumLabel.SelfModulate;
		var alpha = new Color(baseCol.R, baseCol.G, baseCol.B, Mathf.Sin((float)hideTimer.TimeLeft));
		waveNumLabel.SelfModulate = alpha;
		subLabel.SelfModulate = alpha;
		//waveNumLabel.PivotOffset = new Vector2(x: waveNumLabel.Size.X / 2, y: 0);
	}

	// SIGNAL METHODS
	void _on_timer_timeout()
	{
		Hide();
	}
}
