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

	point_to pointer;

	AudioStreamPlayer2D pickUpSound;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
		pickUpSound = GetNode<AudioStreamPlayer2D>("PickUpSound");
	}
	float time;
	float collectedTime;
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
			}
			Show();
			sprite.Play();
			pointer.isActive = true;
			pointer.previewSprite.Play();
		}

		time += (float)delta;

		if (isCollected)
		{
			collectedTime += (float)delta;

			var dirVec = main.PlayerPos - Position;
			Position += dirVec * (float)delta * (collectedTime * 30);

			if (Scale.X > 0)
			{	
				Scale -= new Vector2(x: 0.02f, y: 0.02f);
			}
			else 
			{
				QueueFree();
			}
		}
		else 
		{
			var scaleVal = (Mathf.Sin(time * 2) * 0.1f) + 0.8f;
			sprite.Scale = new Vector2(x: scaleVal, y: scaleVal);
		}

		if (moveToPlayer && !isCollected)
		{
			Position += (main.PlayerPos - Position).Normalized() * time * 250 * (float)delta;
		}
	}

	public void collect()
	{
		isCollected = true;
		pickUpSound.Play();
	}
}
