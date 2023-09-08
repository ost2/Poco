using Godot;
using System;

public partial class player : Plane
{	
	public float curXp;
	public float xpToLevel = 60;

	public bool cantMove = false;
	public bool isDead = false;

	public int level;

	// BASE STAT VARS
	float pMaxHealth = 100.0f;

	float pFireTime = 0.19f;
	float pBulletSpeed = 900.0f;
	float pDamage = 8.0f;
	float pInaccuracyRad = 0.25f;
	float pBaseSpeed = 400.0f;
	float pBoostAccel = 70.0f;
	float pMaxBoost = 300.0f;
	float pSlowDownAccel = 70.0f;
	float pMaxSlowDown = 300.0f;
	float pTurnAccel = 1.6f;
	float pTurnDecay = 0.5f;
	float pMaxTurn = 3.0f;
	float pHeatUp = 0.0003f;
	float pCoolDown = 0.05f;

	public float overHeal;
	public float maxOverHeal;

	Timer regenTimer;



	public float agilityTempBonus = 1;
	public float speedTempBonus = 1;
	public float regenTempBonus = 1;
	public float fireSpeedTempBonus = 1;
	public float damAccTempBonus = 1;

	powerTimer[] powerTimers;

	// CHILD NODES

	AnimatedSprite2D mainSprite;
	AnimatedSprite2D propeller;
	AudioStreamPlayer2D engineSound;
	AudioStreamPlayer2D levelUpSound;
	public cannon cannon;

	point_to pointer;



	// PARENT NODES
	main main;


	// §READY
	public override void _Ready()
	{
		main = GetParent<main>();

		curSpeed = baseSpeed;

		mainSprite = GetNode<AnimatedSprite2D>("MainSprite");
		engineSound = GetNode<AudioStreamPlayer2D>("EngineSound");
		propeller = GetNode<AnimatedSprite2D>("PropellerSprite");
		propeller.Play();

		regenTimer = GetNode<Timer>("RegenTimer");
		regenTimer.Start();

		cannon = GetNode<cannon>("Cannon");

		levelUpSound = GetNode<AudioStreamPlayer2D>("LevelUpSound");

		mainSprite.Animation = "default";

		assignStats();
		cannon.assignStats();

		pointer = GetNode<point_to>("PointTo");
		pointer.showLine = true;
		pointer.showPreview = false;
		pointer.inversePointScale = true;
	}
	void assignStats()
	{
		canAim = true;
		isPlayer = true;

		maxOverHeal = pMaxHealth;

		fireTime = (pFireTime - fireSpeedBonus) / Mathf.Clamp(fireSpeedTempBonus, 1, 1.5f);
		fireTime = Mathf.Clamp(fireTime, 0.0001f, 1);
		bulletSpeed = (pBulletSpeed + bulletSpeedBonus) * Mathf.Clamp(damAccTempBonus, 1, 1.5f);;
		bulletSpeed = Mathf.Clamp(bulletSpeed, 0, 3000);
		damage = (pDamage + damageBonus) * damAccTempBonus;
		inaccuracyRad = (pInaccuracyRad - accuracyBonus) / Mathf.Clamp(damAccTempBonus, 1, 1.5f);;
		inaccuracyRad = Mathf.Clamp(inaccuracyRad, 0, 2);

		heatUp = (pHeatUp - heatUpBonus) / fireSpeedTempBonus;
		heatUp = Math.Clamp(heatUp, 0, 1);
		coolDown = (pCoolDown + coolDownBonus) * fireSpeedTempBonus;

		baseSpeed = (pBaseSpeed + baseSpeedBonus) * speedTempBonus;
		boostAccel = (pBoostAccel + boostAccelBonus) * speedTempBonus;
		maxBoost = (pMaxBoost + maxBoostBonus) * speedTempBonus;
		slowDownAccel = (pSlowDownAccel + slowDownAccelBonus) * speedTempBonus;
		maxSlowDown = (pMaxSlowDown + maxSlowDownBonus) * speedTempBonus;
		turnAccel = (pTurnAccel + turnAccelBonus) * agilityTempBonus;
		turnDecay = (pTurnDecay + turnDecayBonus) * agilityTempBonus;
		maxTurn = (pMaxTurn + maxTurnBonus) * agilityTempBonus;
	}

	void handlePowerTimers(float delta)
	{
		foreach (var timer in powerTimer.powerTimers)
		{
			if (timer.time < timer.maxTime && timer.shouldIncrement)
			{
				timer.done = true;

				if (main.curWaveType != main.waveType.boxWave)
				{
					timer.time += delta;
					doTempBonus(timer.powID, false);
				}
	

				if (timer.first)
				{
					main.hud.startClock(timer.powID, timer.maxTime);
					timer.first = false;
				}
			}
			else
			{
				timer.first = true;
				timer.time = 0;
				timer.shouldIncrement = false;

				if (timer.done)
				{
					doTempBonus(timer.powID, true);
					timer.done = false;
				}
			}
		}
	}

	void doTempBonus(string powID, bool reset)
	{
		float mult = 2;

		if (reset)
		{
			mult = 1;
		}
		switch(powID)
		{
			case "AgilityPack":
			{
				agilityTempBonus = mult;
				break;
			}
			case "SpeedPack":
			{
				speedTempBonus = mult;
				break;
			}
			case "RegenPack":
			{
				regenTempBonus = mult;
				break;
			}
			case "FireSpeedPack":
			{
				fireSpeedTempBonus = mult;
				break;
			}
			case "DamAccPack":
			{
				damAccTempBonus = mult;
				break;
			}
		}
	}

	// §PROCESS
	public override void _PhysicsProcess(double delta)
	{
		assignStats();
		cannon.assignStats();

		pointer.startPos = main.MousePos;

		Vector2 velocity;
		frontVector = Vector2.FromAngle(Rotation);

		if (curXp >= xpToLevel)
		{
			doLevelUp();
		}

		if (!cantMove)
		{
			doInputs(delta);
		}

		if (curSpeed > MaxSpeed)
		{
			curSpeed -= slowDownAccel;
		}

		RotationDegrees += turnVal;

		if (turnVal > 0.0f)
		{
			turnVal -= turnDecay * (float)delta;
		}
		else if (turnVal < 0.0f)
		{
			turnVal += turnDecay * (float)delta;
		}

		//Skew = turnVal * 0.4f;

		velocity = frontVector * curSpeed;

		Velocity = velocity;

		spinPropeller();

		if (mainSprite.Animation != "take_damage")
		{
			handleTiltAnim(mainSprite);
			showStuff(cannon, propeller);
		}
		else
		{
			//hideStuff(cannon, propeller);
		}

		MoveAndSlide();

		if (main.doMainMenu)
		{
			cannon.Hide();
			pointer.isActive = false;
		}
		else 
		{
			cannon.Show();
			pointer.isActive = true;
		}

		handlePowerTimers((float)delta);
	}
	public float TurnSpeedLimit { get { return maxTurn / (1 + Mathf.InverseLerp(0, MaxSpeed, curSpeed)); } }

	void doInputs(double delta)
	{
		if (Input.IsActionPressed("move_forward") && curSpeed <= MaxSpeed)
		{
			curSpeed += boostAccel * (float)delta;
		}
		if (Input.IsActionPressed("move_backward") && curSpeed >= MinSpeed)
		{
			curSpeed -= slowDownAccel * (float)delta;
		}
		if (Input.IsActionPressed("turn_right") && turnVal <= TurnSpeedLimit)
		{
			var turn = turnAccel;
			if (turnVal < 0)
			{
				turn *= 1.3f;
			}
			turnVal += turn * (float)delta;
		}
		if (Input.IsActionPressed("turn_left") && turnVal >= -TurnSpeedLimit)
		{
			var turn = turnAccel;
			if (turnVal > 0)
			{
				turn *= 1.3f;
			}
			turnVal -= turn * (float)delta;
		}

		if (Input.IsActionJustPressed("toggle_aim"))
		{
			canAim = !canAim;
		}
	}


	void spinPropeller()
	{
		var playSpeed = 1 + Mathf.InverseLerp(MinSpeed, MaxSpeed, curSpeed);
		propeller.SpeedScale = 1 + playSpeed;

		engineSound.PitchScale = 0.75f + Mathf.InverseLerp(MinSpeed, MaxSpeed, curSpeed) * 0.4f;
		engineSound.VolumeDb = -5 + Mathf.InverseLerp(MinSpeed, MaxSpeed, curSpeed) * 4;
		if (!engineSound.Playing)
		{
			engineSound.Play();
		}
	}
	// COMBAT
	public void takeDamage(float damage)
	{
		overHeal -= damage;
		if (overHeal < 0)
		{
			health += overHeal;
			overHeal = 0;
		}

		var damageSound = GetNode<AudioStreamPlayer2D>("TakeDamageSound");
		damageSound.Play();

		if (health <= 0.0f)
		{
			playerDie();
		}
		else 
		{
			mainSprite.Animation = "take_damage";
			mainSprite.Play();
			main.startScreenShake(damage * 0.2f, 0.5f);
		}
	}

	void doHealthRegen()
	{
		health += (regenLvl + (regenTempBonus - 1)) * 0.2f * regenTempBonus;
		//mainSprite.Visible = !mainSprite.Visible;
	}

	public void giveXp(float xp)
	{
		curXp += xp;
		if (curXp >= xpToLevel)
		{
			doLevelUp();
		}
	}
	public int hasLevels = 0;
	void doLevelUp()
	{
		curXp -= xpToLevel;
		xpToLevel *= 1.1f - (level * 0.0005f);
		totalLevelUp();

		hasLevels += 1;
		main.hud.arrowTime = 0;
	}
	void playerDie()
	{
		isDead = true;
		cantMove = true;

		main.spawnExplosion(this);
		Hide();
		main.pDie();
	}

	// SIGNAL METHODS

	void _on_take_damage_hit_box_area_entered(Area2D area)
	{
		var bullet = area as bullet;

		if (bullet.firedId != "Player" && !isDead)
		{
			takeDamage(bullet.damage);
		}
	}

	void _on_main_sprite_animation_finished()
	{
		if (mainSprite.Animation == "take_damage")
		{
			mainSprite.Animation = "default";
		}
	}

	
	void _on_collect_hit_box_area_entered(Area2D area)
	{
		if (area is power_up)
		{
			var powerUp = area as power_up;

			if (!powerUp.isCollected)
			{
				powerUp.collect();

				if (powerUp.powerID != "HealthPack")
				{
					main.hud.doStatUpAnimation(powerUp.powerID, powerUp.isTemporary);

					if (powerUp.isTemporary)
					{
						getTempPower(powerUp.powerID);
					}
					else
					{
						getLevel(powerUp.powerID);
					}
				}
				else
				{
					getHealthPack();
				}
			}
		}
	}
	void _on_regen_timer_timeout()
	{
		doHealthRegen();
		regenTimer.Start();
	}

	void getTempPower(string powID)
	{
		switch (powID)
		{
			case "AgilityPack":
			{
				powerTimer.agilityTemp.time = 0;
				powerTimer.agilityTemp.shouldIncrement = true;
				break;
			}
			case "SpeedPack":
			{
				powerTimer.speedTemp.time = 0;
				powerTimer.speedTemp.shouldIncrement = true;
				break;
			}
			case "RegenPack":
			{
				powerTimer.regenTemp.time = 0;
				powerTimer.regenTemp.shouldIncrement = true;
				break;
			}
			case "FireSpeedPack":
			{
				powerTimer.fireSpeedTemp.time = 0;
				powerTimer.fireSpeedTemp.shouldIncrement = true;
				break;
			}
			case "DamAccPack":
			{
				powerTimer.damAccTemp.time = 0;
				powerTimer.damAccTemp.shouldIncrement = true;
				break;
			}
		}
	}

	public void totalLevelUp()
	{
		levelUpSound.Play();
		main.hud.doHudLevelUp();
	}

	public void getLevel(string statUI)
	{
		level += 1;
		
		switch(statUI)
		{
			case "AgilityPack":
			{
				getAgility();
				break;
			}
			case "SpeedPack":
			{
				getSpeed();
				break;
			}
			case "RegenPack":
			{
				getRegen();
				break;
			}
			case "FireSpeedPack":
			{
				getFireSpeed();
				break;
			}
			case "DamAccPack":
			{
				getDamAcc();
				break;
			}
		}
	}

	public float healthPackPower = 35.0f;
	void getHealthPack()
	{
		var h = health;
		var oH = overHeal;
		h += healthPackPower;
		if (h > maxHealth)
		{
			oH += h - maxHealth;
			health = maxHealth;
		}
		else
		{
			health = h;
		}
		oH = Mathf.Clamp(oH, 0, maxHealth);
		overHeal = oH;
	}

	float turnAccelBonus;
	float maxTurnBonus;
	float turnDecayBonus;

	public int agilityLvl;
	void getAgility()
	{
		turnAccelBonus += 0.06f;
		maxTurnBonus += 0.2f;
		turnDecayBonus += 0.01f;

		agilityLvl += 1;
	}

	float boostAccelBonus;
	float slowDownAccelBonus;
	float baseSpeedBonus;
	float maxBoostBonus;
	float maxSlowDownBonus;

	public int speedLvl;
	void getSpeed()
	{
		boostAccelBonus += 15.0f;
		slowDownAccelBonus += 15.0f;
		baseSpeedBonus += 10.0f;
		maxBoostBonus += 30.0f;
		maxSlowDownBonus += 15.0f;

		speedLvl += 1;
	}

	public int fireSpeedLvl;

	float fireSpeedBonus;
	float heatUpBonus;
	float coolDownBonus;
	void getFireSpeed()
	{
		fireSpeedBonus += 0.0009f;
		heatUpBonus += 0.00001f;
		coolDownBonus += 0.003f;

		fireSpeedLvl += 1;
	}

	public int damAccLvl;

	float damageBonus;
	float accuracyBonus;
	float bulletSpeedBonus;
	void getDamAcc()
	{
		damageBonus += 0.4f;
		accuracyBonus += 0.02f;
		bulletSpeedBonus += 25.0f;

		damAccLvl += 1;
	}

	public int regenLvl;
	void getRegen()
	{
		regenLvl += 1;
	}

	public void resetLevels()
	{
		level = 0;
		agilityLvl = 0;
		speedLvl = 0;
		regenLvl = 0;
		fireSpeedLvl = 0;
		damAccLvl = 0;

		turnAccelBonus = 0.0f;
		maxTurnBonus = 0.0f;
		turnDecayBonus = 0.0f;

		boostAccelBonus = 0.0f;
		slowDownAccelBonus = 0.0f;
		baseSpeedBonus = 0.0f;
		maxBoostBonus = 0.0f;
		maxSlowDownBonus = 0.0f;

		fireSpeedBonus = 0.0f;
		heatUpBonus = 0.0f;
		coolDownBonus = 0.0f;

		damageBonus = 0.0f;
		accuracyBonus = 0.0f;
		bulletSpeedBonus = 0.0f;

		hasLevels = 0;
	}

	public void minigun()
	{
		for (int i = 0; i < 100; i++)
		{
			getFireSpeed();
		}
	}
	public void sniper()
	{
		for (int i = 0; i < 100; i++)
		{
			getDamAcc();
		}
	}
	public void acrobat()
	{
		for (int i = 0; i < 100; i++)
		{
			getAgility();
		}
	}
	public void zoomin()
	{
		for (int i = 0; i < 100; i++)
		{
			getSpeed();
		}
	}
}
