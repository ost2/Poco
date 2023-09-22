using Godot;
using System;

public partial class stat_ui : AnimatedSprite2D
{
	hud hud;
	public int lvl;
	public bool clockRunning = false;

	AudioStreamPlayer hoverSound;
	float hoverBaseVolume;
	bool playHover = false;


	Vector2 baseScale;
	Vector2 basePos;

	Vector2 arrowBaseScale;
	Vector2 arrowBasePos;

	bool doArrowHover = false;

	public enum stat
	{
		agility,
		speed,
		regen,
		fireSpeed,
		damAcc
	}
	public bool shouldStartClock = false;

	public bool showArrow = false;

	bool doSurge = false;

	[Export]
	public stat curStat = stat.agility;
	public string statID;
	clock clock;
	AnimatedSprite2D arrow;
	Button button;
	Label label;

	float arrowTime = 0;
	float arrowAnimLength = 1;
	float arrowFreq;

	float clockAnimTime = 1;
	float clockAnimFreq = 2;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		baseScale = Scale;
		basePos = Position;

		hud = GetParent<Node2D>().GetParent<hud>();

		hoverSound = GetNode<AudioStreamPlayer>("HoverSound");
		hoverBaseVolume = hoverSound.VolumeDb;

		clock = GetNode<clock>("Clock");
		arrow = GetNode<AnimatedSprite2D>("Arrow");
		button = GetNode<Button>("Button");
		label = GetNode<Label>("Label");

		switch (curStat)
		{
			case stat.agility:
			{
				Animation = "agility";
				statID = "AgilityPack";
				break;
			}
			case stat.speed:
			{
				Animation = "speed";
				statID = "SpeedPack";
				break;
			}
			case stat.regen:
			{
				Animation = "regen";
				statID = "RegenPack";
				break;
			}
			case stat.fireSpeed:
			{
				Animation = "fire_speed";
				statID = "FireSpeedPack";
				break;
			}
			case stat.damAcc:
			{
				Animation = "dam_acc";
				statID = "DamAccPack";
				break;
			}
		}

		arrowBaseScale = arrow.Scale;
		arrowBasePos = arrow.Position;

		arrow.Hide();
		dontButton();
	}
	float surgeTime = 10;
	float surgeLength = 2;
	float surgeFreq = 4;

	float arrowHoverTime = 1;
	float arrowHoverFreq = 4;
	float arrowHoverDistance = 8;
	float arrowScaleFac = 0.7f;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		arrowTime = hud.arrowTime;
		arrowFreq = hud.arrowFreq;

		clockRunning = clock.running;

		arrowHoverTime += (float)delta * arrowHoverFreq;

		clock.willStart = shouldStartClock;

		label.Text = "X " + lvl;

		if (playHover && button.Disabled == false)
		{
			if (!hoverSound.Playing)
			{
				hoverSound.Play();
			}
			if (hoverSound.VolumeDb < hoverBaseVolume)
			{
				hoverSound.VolumeDb += 240 * (float)delta;
			}
		}
		else
		{	
			if (hoverSound.VolumeDb < -60)
			{
				hoverSound.Stop();
			}
			else
			{
				hoverSound.VolumeDb -= 30 * (float)delta;
			}
		}

		if (showArrow)
		{
			if (arrowTime <= arrowAnimLength * arrowFreq / Mathf.Pi)
			{
				surgeAnimation(arrow, (float)delta, arrowTime, arrowAnimLength, 1.3f, 0, -10);
				arrow.Visible = true;
			}
			else if (doArrowHover)
			{
				var scaleFac = Mathf.Abs(Mathf.Sin(arrowHoverTime) * arrowScaleFac);
				var posVal = Mathf.Abs(Mathf.Sin(arrowHoverTime)) * arrowHoverDistance;

				arrow.Scale = arrowBaseScale + new Vector2(x: scaleFac, y: scaleFac);
				arrow.Position = arrowBasePos - new Vector2(x: 0, y: posVal);
			}
			else
			{
				var stepValScale = Mathf.Abs(arrow.Scale.X - arrowBaseScale.X) * (float)delta * 6;
				var stepValPos = Mathf.Abs(arrow.Position.Y - arrowBasePos.Y) * (float)delta * 6;

				if (arrow.Scale > arrowBaseScale)
				{
					
					arrow.Scale -= new Vector2(x: stepValScale, y: stepValScale);
				}
				else if (arrow.Scale < arrowBaseScale)
				{
					arrow.Scale += new Vector2(x: stepValScale, y: stepValScale);
				}
				if (arrow.Position.Y < arrowBasePos.Y)
				{
					arrow.Position += new Vector2(x: 0, y: stepValPos);
				}
				else if (arrow.Position.Y > arrowBasePos.Y)
				{
					arrow.Position -= new Vector2(x: 0, y: stepValPos);
				}
			}
		}
		else
		{
			surgeAnimation(arrow, (float)delta, arrowTime, arrowAnimLength, -1, 0, 20);

			if (arrowTime >= arrowAnimLength * arrowFreq / 2 || arrow.Scale.X <= 0)
			{
				arrow.Visible = false;
				arrow.Scale = arrowBaseScale;
				arrow.Position = arrowBasePos;
			}
		}

		if (surgeTime <= surgeLength)
		{
			surgeTime += (float)delta * surgeFreq; 
			var lerp = Mathf.InverseLerp(0, surgeLength / Mathf.Pi, surgeTime);

			var posVal = Mathf.Sin(lerp) * 10;
			var scaleVal = Mathf.Sin(lerp) * 0.4f;

			Position = basePos - new Vector2(x: 0, y: posVal);
			Scale = baseScale + new Vector2(x: scaleVal, y: scaleVal);
		}
		else
		{
			Scale = baseScale;
			Position = basePos;
		}
		
	}

	public void setStatLevel(int level)
	{
		lvl = level;
	}

	void _on_button_mouse_entered()
	{
		if (button.Disabled == false)
		{
			arrowHoverTime = 0;
			doArrowHover = true;
			arrow.Frame = 1;

			playHover = true;
		}
	}
	void _on_button_mouse_exited()
	{
		doArrowHover = false;
		arrow.Frame = 0;

		playHover = false;
	}
	void _on_button_pressed()
	{
		hud.statButtonPressed(statID);
	}

	public void startSurge()
	{
		doSurge = true;
		surgeTime = 0;
	}

	public void doArrow()
	{
		arrowTime = 0;
		showArrow = true;
		arrow.Play();
	}
	public void dontArrow()
	{
		arrowTime = 0;
		showArrow = false;
	}

	public void doButton()
	{
		button.Disabled = false;
	}
	public void dontButton()
	{
		button.Disabled = true;
	}

	public void startClock(float time)
	{
		shouldStartClock = false;
		clock.start(time);
	}
	public void stopClock()
	{
		clock.stop();
	}

	void surgeAnimation(Node2D node, float delta, float t, float time, float scaleMult, float xMove = 0, float yMove = 0, float limit = 2)
	{
		var lerp = Mathf.InverseLerp(0, time, t);
		lerp = Mathf.Clamp(lerp, 0, limit);
		var scaleVal = 1 + Mathf.Sin(lerp) * scaleMult;

		float xPos = 0;
		float yPos = 0;

		if (lerp <= limit)
		{
			xPos = node.Position.X + Mathf.Cos(lerp * 2) * xMove * delta;
			yPos = node.Position.Y + Mathf.Cos(lerp * 2) * yMove * delta;

			var pos = new Vector2(x: xPos, y: yPos);
			var scale = new Vector2(x: scaleVal, y: scaleVal);

			node.Position = pos;
			node.Scale = scale;
		}
	}
}
