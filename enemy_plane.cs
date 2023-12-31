using Godot;
using System;

public partial class enemy_plane : Plane
{
	float xpValue = 30;
	float xpWaveBonus = 0.03f;
	public int enemyLevel = 1;
	public bool bigMode = false;

	float randX;
	float randY;

	// MOVEMENT
	float distance;

	// PARENT NODES
	main main;

	// NODES
	player player;

	// CHILD NODES
	AnimatedSprite2D planeSprite;
	AnimatedSprite2D propellerSprite;
	AnimatedSprite2D propeller2;
	AnimatedSprite2D propeller3;
	AudioStreamPlayer2D engineSound;
	cannon cannon;
	cannon cannon2;
	cannon cannon3;
	point_to pointer;

	Node2D wingTips;

	Vector2 baseScale;

	// §READY
	public override void _Ready()
	{
		curSpeed = baseSpeed;
		canAim = false;

		main = GetParent<main>();
		player = main.GetNode<player>("Player");
		planeSprite = GetNode<AnimatedSprite2D>("PlaneSprite");
		planeSprite.Animation = "default";

		cannon = GetNode<cannon>("Cannon");
		cannon2 = GetNode<cannon>("Cannon2");
		cannon3 = GetNode<cannon>("Cannon3");

		propellerSprite = GetNode<AnimatedSprite2D>("PropellerSprite");
		propeller2 = GetNode<AnimatedSprite2D>("PropellerSprite2");
		propeller3 = GetNode<AnimatedSprite2D>("PropellerSprite3");

		engineSound = GetNode<AudioStreamPlayer2D>("EngineSound");

		wingTips = GetNode<Node2D>("WingTips");

		pointer = GetNode<point_to>("PointTo");
		pointer.isActive = false;
		pointer.showLine = false;
		pointer.showPreview = true;
		
		var rand = new Random();
		distance = rand.Next(60, 180);
		Rotation = getPlayerDirVector().Angle();

		GetNode<Area2D>("DamageHitBox").Name = "Enemy";

		var RNG = new RandomNumberGenerator();
		randX = RNG.RandfRange(-100 * main.DifficultyMult, 100 * main.DifficultyMult);
		randY = RNG.RandfRange(-100 * main.DifficultyMult, 100 * main.DifficultyMult);

		if (bigMode)
		{
			goBigMode();
			pointer.previewSprite.Animation = "enemy_plane_big";
			pointer.previewSprite.Scale *= 0.4f;
			pointer.previewSprite.Play();

			cannon.QueueFree();
		}
		else
		{
			pointer.previewSprite.Animation = "enemy_plane";
			pointer.previewSprite.Scale *= 0.5f;
			pointer.previewSprite.Play();

			cannon2.QueueFree();
			cannon3.QueueFree();
		}

		assignStats();

		planeSprite.Animation = "default";
	}

	Color[] lvlColors;
	void assignStats()
	{
		var lvl1Color = new Color(0.8f, 0.7f, 0.6f, 1);
		var lvl2Color = new Color(0.8f, 0.8f, 0, 1);
		var lvl3Color = new Color(0.9f, 0, 0, 1);

		lvlColors =  new Color[]{ lvl1Color, lvl2Color, lvl3Color };

		if (enemyLevel == 2)
		{
			turnAccel = 1.5f;
			baseSpeed = 500.0f;
			maxBoost = 450.0f;
			boostAccel = 150.0f;
			maxSlowDown = 400.0f;
			slowDownAccel = 150.0f;

			fireTime = 0.16f;
			bulletSpeed = 1000.0f;
			damage = 4.0f;
			inaccuracyRad = 0.16f;
			fireRange = 1000.0f;

			health = 80.0f;
			xpValue = 60.0f;

		}
		else if (enemyLevel == 3)
		{
			turnAccel = 1.7f;
			baseSpeed = 600.0f;
			maxBoost = 550.0f;
			boostAccel = 200.0f;
			maxSlowDown = 500.0f;
			slowDownAccel = 175.0f;

			fireTime = 0.135f;
			bulletSpeed = 1150.0f;
			damage = 6.0f;
			inaccuracyRad = 0.14f;
			fireRange = 1100.0f;

			health = 100.0f;
			xpValue = 120.0f;
		}
		else  
		{
			turnAccel = 1.35f;
			baseSpeed = 350.0f;
			maxBoost = 350.0f;
			boostAccel = 100.0f;
			maxSlowDown = 300.0f;
			slowDownAccel = 135.0f;

			fireTime = 0.2f;
			bulletSpeed = 800.0f;
			damage = 3.0f;
			inaccuracyRad = 0.2f;
			fireRange = 900.0f;

			health = 50.0f;
			xpValue = 30.0f;
		}

		if (bigMode)
		{
			goBigStats();
		}

		damage *= main.DifficultyMult;
		health *= main.DifficultyMult;
		aimSkill *= main.DifficultyMult;

		if (planeSprite.Animation != "take_damage")
		{
			planeSprite.SelfModulate = lvlColors[enemyLevel - 1];
		}
		maxHealth = health;

		pointer.previewSprite.SelfModulate = planeSprite.SelfModulate;

		//inaccuracyRad = inaccuracyRad * (1 + getCloudInaccuracy());
	}

	void goBigMode()
	{
		planeSprite.Hide();
		planeSprite = GetNode<AnimatedSprite2D>("StrongPlaneSprite");
		planeSprite.Show();
		
		wingTips.GetNode<Node2D>("LeftWingTip").Position = new Vector2(x: 4, y: -14);
		wingTips.GetNode<Node2D>("RightWingTip").Position = new Vector2(x: 4, y: 14);

		Scale *= 1.5f;
	}
	void goBigStats()
	{
		turnAccel *= 0.8f;
		maxBoost *= 0.7f;
		damage *= 0.8f;
		
		slowDownAccel *= 1.75f;
		fireTime *= 0.75f;

		xpValue *= 5;
		health *= 5;
	}
	float getCloudInaccuracy()
	{
		var noiseVal = main.cloudNoise.GetNoise2D(GlobalPosition.X, GlobalPosition.Y);
		var pNoiseVal = main.cloudNoise.GetNoise2D(main.player.GlobalPosition.X, main.player.GlobalPosition.Y);

		return noiseVal + pNoiseVal;
	}
	
	float storedRotation;
	float deadTime = 0;
	// §PROCESS
	public override void _PhysicsProcess(double delta)
	{
		if (isDead)
		{
			deadTime += (float)delta;
			if (deadTime > 3)
			{
				QueueFree();
			}
		}
		var dirVector = getPlayerDirVector();

		if (main.killsThisWave == main.killsToProgress - 1)
		{
			pointer.isActive = true;
		}
		else 
		{
			pointer.isActive = false;
		}
		if (!isDead)
		{
			if (bigMode)
			{
				propellerSprite.Hide();
				propeller2.Show();
				propeller3.Show();
				spinPropeller(propeller2, engineSound);
				spinPropeller(propeller3);
			}
			else
			{
				propellerSprite.Show();
				propeller2.Hide();
				propeller3.Hide();
				spinPropeller(propellerSprite, engineSound);
			}

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

			if (planeSprite.Animation != "take_damage" && planeSprite.Animation != "explode")
			{
				handleTiltAnim(planeSprite);
				//showStuff(null, propellerSprite);
			}
			else 
			{
				//hideStuff(cannon, null);
			}
		}
		else 
		{
			Scale *= 1 + 5 * (float)delta;
		}

		MoveAndSlide();

		// if ((main.PlayerPos - Position).Length() > 30000)
		// {
		// 	QueueFree();
		// }
	}
	Vector2 randPos;
	Vector2 getPlayerDirVector()
	{
		randPos = new Vector2(x: player.Position.X + randX, y: player.Position.Y + randY);

		var pVec = randPos - Position;
		var invVec = pVec.Inverse().Normalized();
		var pos = randPos + invVec * distance;

		return (pos - Position).Normalized();
	}


	// COMBAT
	
	void takeDamage(float damage, bullet bull = null)
	{
		if (damage > 0)
		{
			health -= damage;
			var damageSound = GetNode<AudioStreamPlayer2D>("TakeDamageSound");
			damageSound.PitchScale = 1 + Mathf.Clamp((maxHealth - health) / maxHealth, 0.05f, 1);
			damageSound.Play();

			if (health <= 0.0f)
			{
				foreach (var part in hitParticlesArr)
				{
					part.QueueFree();
				}
				enemyDie();
			}
			else 
			{
				if (bull != null)
				{
					var particles = getHitParticles(bull.angle, hitParticlesScene);
					CallDeferred("add_child", particles);
				}
				
				planeSprite.SelfModulate = new Color(255, 255, 255, 1);
				planeSprite.Animation = "take_damage";
				planeSprite.Play();
			}
		}
	}
	void enemyDie()
	{
		pointer.Modulate = new Color(0, 0, 0, 0);

		main.lastEnemyKilledPos = GlobalPosition;
		main.player.curXp += xpValue + (xpValue * (xpWaveBonus * (main.waveNumber - 1)));
		main.playerKills++;
		main.killsThisWave++;
		isDead = true;
		planeSprite.Animation = "explode";
		planeSprite.Play();

		propellerSprite.Hide();
		propeller2.Hide();
		propeller3.Hide();
		wingTips.Hide();

		if (bigMode)
		{
			main.spawnExplosion(this, 1.5f, -7);
			cannon2.Hide();
			cannon3.Hide();
		}
		else
		{
			main.spawnExplosion(this);
			cannon.Hide();
		}
		main.player.playKillSound();

		var spawnRocketChance = xpValue / 5;
		var rocketCountProb = xpValue * 1.5f;
		var spawned = main.maybeSpawnRockets(spawnRocketChance, GlobalPosition, rocketCountProb);

		if (!spawned)
		{
			var spawnTempChance = xpValue / 10;
			spawned = main.maybeSpawnPower(spawnTempChance, GlobalPosition, true);
		}

		if (!spawned)
		{
			var spawnPermChance = xpValue / 50;
			main.maybeSpawnPower(spawnPermChance, GlobalPosition, false);
		}
	}

	// SIGNAL METHODS
	void _on_damage_hit_box_area_entered(Area2D area)
	{
		if (!isDead)
		{
			if (area is bullet)
			{
				var bull = area as bullet;
				if (bull.firedId != "Enemy")
				{
					//main.spawnExplosion(this, 0.15f, false, bullet, 3.5f);
					takeDamage(bull.damage, bull);
					bull.QueueFree();
				}
			}

			else if (area.Name == "Explosion")
			{
				var explo = area.GetParent<explosion>();
				
				takeDamage(explo.damage);
			}
		}
	}

	void _on_plane_sprite_animation_finished()
	{
		switch (planeSprite.Animation)
		{
			case "take_damage":
			{
				planeSprite.SelfModulate = lvlColors[enemyLevel - 1];
				planeSprite.Animation = "default";

				break;
			}
			case "explode":
			{
				QueueFree();
				break;
			}
		}
	}
}
