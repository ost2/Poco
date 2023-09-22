using Godot;
using System;

public partial class boss_plane : Plane
{
	float xpValue = 300;
	public int bossLevel = 1;

	// MOVEMENT
	float distance;

	// PARENT NODES
	main main;

	// NODES
	player player;

	// CHILD NODES
	AnimatedSprite2D planeSprite;
	
	AnimatedSprite2D propellerSprite;
	AnimatedSprite2D propellerSprite2;
	AnimatedSprite2D propellerSprite3;

	AudioStreamPlayer2D engineSound;
	cannon cannon;

	point_to pointer;

	Node2D wingTips;

	// §READY
	public override void _Ready()
	{
		curSpeed = baseSpeed;
		canAim = true;

		main = GetParent<main>();
		player = main.GetNode<player>("Player");
		planeSprite = GetNode<AnimatedSprite2D>("PlaneSprite");
		planeSprite.Animation = "default";

		pointer = GetNode<point_to>("PointTo");
		pointer.previewSprite.Animation = "boss_plane";
		pointer.previewSprite.Play();
		pointer.previewSprite.Scale = new Vector2(x: 0.2f, y: 0.2f);
		pointer.isActive = true;
		pointer.showPreview = true;
		pointer.showLine = false;

		wingTips = GetNode<Node2D>("WingTips");

		cannon = GetNode<cannon>("Cannon");

		propellerSprite = GetNode<AnimatedSprite2D>("PropellerSprite");
		propellerSprite2 = GetNode<AnimatedSprite2D>("PropellerSprite2");
		propellerSprite3 = GetNode<AnimatedSprite2D>("PropellerSprite3");

		engineSound = GetNode<AudioStreamPlayer2D>("EngineSound");

		var rand = new Random();
		distance = rand.Next(60, 180);
		Rotation = getPlayerDirVector().Angle();

		GetNode<Area2D>("DamageHitBox").Name = "Enemy";

		assignStats();
	}

	Color[] lvlColors;
	void assignStats()
	{
		var lvl1Color = new Color(0.8f, 0.7f, 0.6f, 1);
		var lvl2Color = new Color(3f, 1f, 0, 1);
		var lvl3Color = new Color(2.8f, 0, 0, 1);

		lvlColors =  new Color[]{ lvl1Color, lvl2Color, lvl3Color };

		if (bossLevel == 2)
		{
			turnAccel = 1.5f;
			baseSpeed = 500.0f;
			maxBoost = 550.0f;
			boostAccel = 150.0f;
			maxSlowDown = 400.0f;
			slowDownAccel = 150.0f;

			fireTime = 0.055f;
			bulletSpeed = 1000.0f;
			damage = 8.0f;
			inaccuracyRad = 0.16f;
			fireRange = 1200.0f;

			health = 1500.0f;
			xpValue = 3200.0f;

			aimSkill = 0.9f;
		}
		else if (bossLevel == 3)
		{
			turnAccel = 1.7f;
			baseSpeed = 600.0f;
			maxBoost = 650.0f;
			boostAccel = 200.0f;
			maxSlowDown = 500.0f;
			slowDownAccel = 175.0f;

			fireTime = 0.035f;
			bulletSpeed = 1150.0f;
			damage = 11.0f;
			inaccuracyRad = 0.14f;
			fireRange = 1300.0f;

			health = 2000.0f;
			xpValue = 1500.0f;

			aimSkill = 1;
		}
		else  
		{
			turnAccel = 1.35f;
			baseSpeed = 350.0f;
			maxBoost = 400.0f;
			boostAccel = 100.0f;
			maxSlowDown = 300.0f;
			slowDownAccel = 135.0f;

			fireTime = 0.075f;
			bulletSpeed = 800.0f;
			damage = 5.0f;
			inaccuracyRad = 0.2f;
			fireRange = 1100.0f;

			health = 1000.0f;
			xpValue = 900.0f;

			aimSkill = 0.8f;
		}

		damage *= main.DifficultyMult;
		health *= main.DifficultyMult;
		aimSkill *= main.DifficultyMult;

		if (planeSprite.Animation != "take_damage")
		{
			planeSprite.SelfModulate = lvlColors[bossLevel - 1];
		}

		maxHealth = health;
	}
	
	float storedRotation;
	// §PROCESS
	public override void _PhysicsProcess(double delta)
	{
		var dirVector = getPlayerDirVector();
		if (!isDead)
		{
			if (!main.GameOver)
			{
				if ((player.Position - Position).Length() > 500 && curSpeed < MaxSpeed)
				{
					curSpeed += boostAccel * (float)delta;
				}
				else if ((player.Position - Position).Length() < 300 && curSpeed > MinSpeed)
				{
					curSpeed -= slowDownAccel * (float)delta;
				}

				Rotation = Vector2.FromAngle(Rotation).MoveToward(dirVector, turnAccel * (float)delta).Angle();

				turnVal =  (Rotation - storedRotation) * 50;

				storedRotation = Rotation;

				frontVector = Vector2.FromAngle(Rotation);

				Velocity = frontVector * curSpeed;
			}

			spinPropeller(propellerSprite, engineSound);
			spinPropeller(propellerSprite2);
			spinPropeller(propellerSprite3);

			if (planeSprite.Animation != "take_damage")
			{
				handleTiltAnim(planeSprite);
				//showStuff(null, propellerSprite, propellerSprite2, propellerSprite3);
			}
			else 
			{
				//hideStuff(null, propellerSprite, propellerSprite2, propellerSprite3);
			}
		}
		else 
		{
			var rand = new Random();
			deadTime += (float)delta;

			if (deadTime % 0.04f == 0 && deadTime < 0.25f)
			{
				main.spawnExplosion(this, 1 + (rand.Next(0, 3) * 0.1f), -5, null, 1, 100.0f);
			}

			planeSprite.Scale *= 1 + (1.5f * (float)delta);
			if (rand.Next(0, (int)Engine.GetFramesPerSecond()) == 0)
			{
				main.spawnExplosion(this, 1 + (rand.Next(0, 3) * 0.1f), -5, null, 1, 100.0f);
			}
		}
		MoveAndSlide();
	}
	float deadTime = 0;

	Vector2 getPlayerDirVector()
	{
		// var backVec = player.frontVector.Inverse();
		// var backPoint = player.Position + backVec * distance / 5;

		var pVec = player.Position - Position;
		var invVec = pVec.Inverse().Normalized();
		var pos = player.Position + invVec * distance;

		return (pos - Position).Normalized();
	}

	// COMBAT
	void takeDamage(float damage, bullet bull = null)
	{
		health -= damage;
		var damageSound = GetNode<AudioStreamPlayer2D>("TakeDamageSound");
		damageSound.PitchScale = 1 + (maxHealth - health) / maxHealth;
		damageSound.Play();

		if (health <= 0.0f)
		{
			bossDie();
		}
		else 
		{
			if (bull != null)
			{
				var particles = getHitParticles(bull.angle, hitParticlesScene);
				CallDeferred("add_child", particles);
			}
			planeSprite.Animation = "take_damage";
			planeSprite.SelfModulate = new Color(1, 1, 1, 1);
			planeSprite.Play();
		}
	}
	void bossDie()
	{
		main.lastEnemyKilledPos = GlobalPosition;
		hideStuff(cannon, propellerSprite, propellerSprite2, propellerSprite3, wingTips);
		main.player.curXp += xpValue;
		main.playerKills++;
		main.killsThisWave++;
		isDead = true;
		planeSprite.Animation = "explode";
		planeSprite.Play();
		main.spawnExplosion(this, 3, 0);

		main.bossDie();
	}

	// SIGNAL METHODS
	void _on_damage_hit_box_area_entered(Area2D area)
	{
		if (!isDead)
		{
			if (area is bullet)
			{
				var bullet = area as bullet;
				if (bullet.firedId != "Enemy")
				{
					//main.spawnExplosion(this, 0.15f, false, bullet, 3.5f);
					takeDamage(bullet.damage, bullet);
					bullet.QueueFree();
				}
			}

			else if (area.Name == "Explosion")
			{
				var explo = area.GetParent<explosion>();
				
				takeDamage(explo.damage);
			}
		}
	}

	void _on_sprite_2d_animation_finished()
	{
		if (planeSprite.Animation == "take_damage")
		{
			planeSprite.SelfModulate = lvlColors[bossLevel - 1];
			planeSprite.Animation = "default";
		}
		if (planeSprite.Animation == "explode")
		{
			QueueFree();
		}
	}
}
