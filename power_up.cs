using Godot;
using System;

public partial class power_up : Area2D
{
	public string powerID = "null";

	AnimatedSprite2D sprite; 
	AnimatedSprite2D tempSprite;

	main main;

	public bool isCollected = false;
	public bool isTemporary = false;
	public bool moveToPlayer = false;

	public bool hoverCollect = true;

	public int rocketCount;

	point_to pointer;

	AudioStreamPlayer2D pickUpSound;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (hoverCollect)
		{
			moveToPlayer = false;
		}

		Hide();
		pickUpSound = GetNode<AudioStreamPlayer2D>("PickUpSound");
	}
	float time;
	float collectedTime;
	float hoveredTime = 100;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	bool firstCall = true;
	public override void _PhysicsProcess(double delta)
	{
		if (firstCall)
		{
			firstCall = false;

			sprite = GetNode<AnimatedSprite2D>("PowerUpSprite");
			tempSprite = sprite.GetNode<AnimatedSprite2D>("TempSprite");
			
			if (isTemporary)
			{
				tempSprite.Show();
				if (!tempSprite.IsPlaying())
				{
					tempSprite.Play();
				}
			}
			else
			{
				tempSprite.Hide();
			}

			main = GetParent<main>();

			pointer = GetNode<point_to>("PointTo");

			switch (powerID)
			{
				case "HealthPack":
				{
					sprite.Animation = "health_pack";
					pointer.previewSprite.Animation = "health_pack";
					break;
				}
				case "AgilityPack":
				{
					sprite.Animation = "agility_pack";
					pointer.previewSprite.Animation = "agility_pack";
					break;
				}
				case "SpeedPack":
				{
					sprite.Animation = "speed_pack";
					pointer.previewSprite.Animation = "speed_pack";
					break;
				}
				case "FireSpeedPack":
				{
					sprite.Animation = "fire_speed_pack";
					pointer.previewSprite.Animation = "fire_speed_pack";
					break;
				}
				case "DamAccPack":
				{
					sprite.Animation = "dam_acc_pack";
					pointer.previewSprite.Animation = "dam_acc_pack";
					break;
				}
				case "RegenPack":
				{
					sprite.Animation = "regen_pack";
					pointer.previewSprite.Animation = "regen_pack";
					break;
				}
				case "OneRocket":
				{
					sprite.Animation = "rocket_one";
					pointer.previewSprite.Animation = "rocket_one";
					break;
				}
				case "TwoRocket":
				{
					sprite.Animation = "rocket_two";
					pointer.previewSprite.Animation = "rocket_two";
					break;
				}
				case "ThreeRocket":
				{
					sprite.Animation = "rocket_three";
					pointer.previewSprite.Animation = "rocket_three";
					break;
				}
			}
			Show();
			sprite.Play();
			pointer.isActive = true;
			pointer.previewSprite.Play();
		}

		time += (float)delta;

		float scaleVal = 0;

		if (isCollected)
		{
			pointer.isActive = false;

			collectedTime += (float)delta;

			var dirVec = main.PlayerPos - Position;
			Position += dirVec * (float)delta * (collectedTime * 22);

			if (Scale.X > 0)
			{	
				scaleVal = Mathf.Clamp(1 - collectedTime * 0.5f, 0, 3);
			}
			else 
			{
				QueueFree();
			}
		}
		else 
		{
			scaleVal = (Mathf.Sin(time * 2) * 0.1f) + 0.8f;
		}
		if (hoveredTime < 100)
		{
			hoveredTime += (float)delta;
			if (hoveredTime <= 0.5f)
			{
				scaleVal += Mathf.Clamp(Mathf.Sin(hoveredTime * 8), 0, 1);
			}
		}
		sprite.Scale = new Vector2(x: scaleVal, y: scaleVal);

		if (moveToPlayer && !isCollected)
		{
			Position += (main.PlayerPos - Position).Normalized() * hoveredTime * 25;
		}

		if (collectedTime > 5)
		{
			despawn();
		}
	}

	public void despawn()
	{
		QueueFree();
	}

	public void collect()
	{
		isCollected = true;
		pickUpSound.Play();
	}
	bool hovered = false;
	void _on_mouse_entered()
	{
		if (!hovered && !isCollected)
		{
			hovered = true;
			hoveredTime = 0;
			moveToPlayer = true;
			GetNode<AudioStreamPlayer2D>("HoverSound").Play();
		}
	}
}
