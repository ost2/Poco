using Godot;
using System;

public partial class fancy_slider : HSlider
{
	bool canGrab;
	bool isGrab;
	AnimatedSprite2D slider;
	AnimatedSprite2D grabber;
	AudioStreamPlayer clickSound;
	AudioStreamPlayer hoverSound;
	AudioStreamPlayer releaseSound;
	Color baseColor;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		slider = GetNode<AnimatedSprite2D>("BarSprite");
		grabber = GetNode<AnimatedSprite2D>("GrabSprite");

		clickSound = GetNode<AudioStreamPlayer>("ClickSound");
		releaseSound = GetNode<AudioStreamPlayer>("ReleaseSound");
		hoverSound = GetNode<AudioStreamPlayer>("HoverSound");

		baseColor = grabber.SelfModulate;
		grabber.Animation = "default";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var centerPos = new Vector2(x: 32, y: 8);
		var posAdd = (-0.5f + (float)Mathf.InverseLerp(MinValue, MaxValue, Value)) * 46;
		var addVec = new Vector2(x: posAdd, y: 0);

		grabber.Position = centerPos + addVec;
	}

	void _on_mouse_entered()
	{
		hoverSound.Play();
		grabber.SelfModulate = new Color(baseColor.R * 1.4f, baseColor.G * 1.4f, baseColor.B * 1.4f, 1);
		canGrab = true;
	}
	void _on_mouse_exited()
	{
		grabber.SelfModulate = baseColor;
		canGrab = false;
	}

	void _on_drag_ended(bool valueChanged)
	{
		releaseSound.Play();
		isGrab = false;

		grabber.Animation = "default";
	}
	void _on_drag_started()
	{
		clickSound.Play();
		grabber.Animation = "grab";
		isGrab = true;
	}
}
