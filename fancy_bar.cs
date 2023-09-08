using Godot;
using System;

public partial class fancy_bar : TextureProgressBar
{
	[Export]
	public PackedScene numberScene;

	main main;

	public bool showNegative = true;
	public bool leftSide = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		lastValue = (float)Value;
		main = GetParent<hud>().GetParent<main>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	float lastValue;
	void _on_value_changed(float value)
	{
		var diff = Mathf.Abs(value - lastValue);
		var range = MaxValue - MinValue;

		var scaleVal = ((float)diff / (float)range);
		var maxTime = ((float)diff / (float)range) * 0.5f;

		if (main.gameRunning)
		{
			spawnNumber(scaleVal, maxTime, diff, main.player);
			spawnNumber(scaleVal, maxTime, diff);
		}
		
		lastValue = value;
	}

	void spawnNumber(float scale, float timeLimit, float diff, Node2D follow = null)
	{
		var number = numberScene.Instantiate<value_changed_label>();

		number.timeLimit = 1.5f;
		number.Position = new Vector2(x: -15, y: 38 - ((float)Value / (float)MaxValue) * 40);

		number.maxSize = 0.4f;

		number.left = leftSide;

		number.followNode = follow;

		number.Text = (float)Value - lastValue > 0 ? "+ " + diff.ToString("N0") : "- " + diff.ToString("N0");

		if (!showNegative && (float)Value - lastValue < 0)
		{
			return;
		}
		else 
		{
			AddChild(number);
		}
	}
}
