using Godot;
using System;

public partial class clock : AnimatedSprite2D
{
	public float maxTime = 10;
	public bool running = false;

	bool oscil = true;
	public float time;
	Vector2 baseScale;

	public float animTime = 1;
	public float animFreq = 4;

	AudioStreamPlayer tickTock;
	float tickVolume;

	public bool showShadow = true;

	AnimatedSprite2D hand;
    public bool willStart = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		tickTock = GetNode<AudioStreamPlayer>("TickSound");
		tickVolume = tickTock.VolumeDb;
		baseScale = Scale;

		Scale = Vector2.Zero;

		hand = GetNode<AnimatedSprite2D>("Hand");
		Hide();

		if (!showShadow)
		{
			GetNode<AnimatedSprite2D>("Shadow").Hide();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!showShadow)
		{
			GetNode<AnimatedSprite2D>("Shadow").Hide();
		}
		time += (float)delta;

		var scaleVal = baseScale.X + (Mathf.Sin(time * 4) * 0.04f);
		if (running)
		{
			if (hand.RotationDegrees >= 360)
			{
				stop();
			}

			hand.RotationDegrees += (float)delta * (360 / maxTime);

			tickTock.PitchScale = 1 + Mathf.InverseLerp(0, maxTime, time);

			if (!tickTock.Playing)
			{
				tickTock.Play();
			}

			if (hand.RotationDegrees > 300 && !IsPlaying())
			{
				Play();
			}

			if (time * 2 <= 1)
			{
				Scale = new Vector2(x: time * 2 * baseScale.X, y: time * 2 * baseScale.Y);
			}
			else
			{
				Scale = new Vector2(scaleVal, scaleVal);
			}
		}
		else
		{
			if (Scale.X > 0)
			{
				Scale = new Vector2(x: baseScale.X - (time * 2 * baseScale.X), y: baseScale.Y - (time * 2 * baseScale.Y));
			}
			else
			{
				Hide();
			}
		}

		Rotation = Mathf.Sin(time * 8) * 0.2f;
	}

	public void start(float t = 0)
	{
		willStart = false;

		tickTock.VolumeDb = tickVolume;
		
		time = 0;

		if (t != 0)
		{
			maxTime = t;
		}

		hand.RotationDegrees = 0;
		running = true;
		Show();
	}
	public void stop()
	{
		time = 0;

		tickTock.VolumeDb = -80;
		tickTock.Stop();

		running = false;
		Stop();
	}

	void surgeAnimation(float delta, float t, float time, float scaleMult, float xMove = 0, float yMove = 0)
	{
		var lerp = Mathf.InverseLerp(0, time, t);
		var scaleVal = baseScale.X + Mathf.Sin(lerp) * scaleMult;

		float xPos = 0;
		float yPos = 0;

		if (lerp <= 2)
		{
			xPos = Position.X + Mathf.Cos(lerp * 2) * xMove * delta;
			yPos = Position.Y + Mathf.Cos(lerp * 2) * yMove * delta;

			var pos = new Vector2(x: xPos, y: yPos);
			var scale = new Vector2(x: scaleVal, y: scaleVal);

			Position = pos;
			Scale = scale;
		}
	}
}
