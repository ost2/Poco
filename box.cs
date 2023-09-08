using Godot;
using System;

public partial class box : Area2D
{
	Vector2 windDir;
	float speed;

	AnimatedSprite2D boxSprite;
	AnimatedSprite2D chuteSprite;

	AudioStreamPlayer2D damageSound;
	AudioStreamPlayer2D breakSound;

	point_to pointer;

	main main;

	float health = 50;

	float time;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		main = GetParent<main>();

		var rand = new RandomNumberGenerator();
		speed = rand.RandfRange(100, 200);
		
		boxSprite = GetNode<AnimatedSprite2D>("BoxSprite");
		boxSprite.Animation = "default";
		chuteSprite = GetNode<AnimatedSprite2D>("ChuteSprite");
		chuteSprite.Play();

		damageSound = GetNode<AudioStreamPlayer2D>("DamageSound");
		breakSound = GetNode<AudioStreamPlayer2D>("BreakSound");

		pointer = GetNode<point_to>("PointTo");
		pointer.showLine = false;

		pointer.previewSprite.Animation = "box";
		pointer.previewSprite.Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	bool isBroken = false;
	public override void _Process(double delta)
	{
		time += (float)delta;
		windDir = new Vector2(x: 20, y: Mathf.Sin(time / 2)).Normalized(); 

		Position += windDir * speed * (float)delta;

		chuteSprite.Rotation = Mathf.Cos(time / 2) / 2;
		boxSprite.Rotation = Mathf.Cos(time / 2) / 4;

		// chuteSprite.Skew = Mathf.Cos(time / 2) / 4;
		// boxSprite.Skew = -Mathf.Cos(time / 2) / 4;
		if (time > 60 && (Position - main.PlayerPos).Length() > 1920)
		{
			QueueFree();
		}

		if (isBroken)
		{
			pointer.isActive = false;
			chuteSprite.Position += new Vector2(x: 150, y: 0) * (float)delta;
		}
	}
	public bool moveToPlayer = false;

	void takeDamage(float damage)
	{
		if (!isBroken)
		{
			health -= damage;
			boxSprite.Animation = "take_damage";
			boxSprite.Play();

			damageSound.Play();

			if (health <= 0)
			{
				moveToPlayer = true;
				breakBox();
			}
		}
	}
	void breakBox()
	{
		isBroken = true;
		boxSprite.Animation = "break";
		boxSprite.Play();

		breakSound.Play();

		main.spawnPowerUp(Position, moveToPlayer);
	}

	public void despawnBox()
	{
		boxSprite.Animation = "despawn";
		boxSprite.Play();
	}

	// SIGNAL METHODS
	void _on_area_entered(Area2D area)
	{
		if (!isBroken)
		{
			if (area is bullet)
			{
				var bullet = area as bullet;

				if (bullet.firedId == "Player")
				{
					takeDamage(bullet.damage);
					bullet.QueueFree();
				}
			}
			else 
			{
				breakBox();
			}
		}
	}
	void _on_box_sprite_animation_finished()
	{
		if (boxSprite.Animation == "break")
		{
			boxSprite.Visible = false;
		}

		if (boxSprite.Animation == "take_damage")
		{
			boxSprite.Animation = "default";
		}

		if (boxSprite.Animation == "despawn")
		{
			QueueFree();
		}
	}
}
