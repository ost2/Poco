using Godot;
using System;
using System.Collections.Generic;

public partial class player : Plane
{	
	public float curXp;
	public float xpToLevel = 60;

	public bool cantMove = false;

	public int level;

	// BASE STAT VARS
	float pMaxHealth = 100.0f;

	float pFireTime = 0.23f;
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
	
	float rocketDamage = 100;
	float rocketExploScale = 0.8f;
	int leanLevel;

	public float overHeal;
	public float maxOverHeal;

	float rocketCount;
	Timer regenTimer;

	Node2D rocketCannonNode;
	rocket_cannon[] rocketCannons;
	rocket_cannon rocketC1;
	rocket_cannon rocketC2;

	rocket_target rTarget;

	public float agilityTempBonus = 1;
	public float speedTempBonus = 1;
	public float regenTempBonus = 1;
	public float fireSpeedTempBonus = 1;
	public float damAccTempBonus = 1;

	// CHILD NODES

	AnimatedSprite2D mainSprite;
	AnimatedSprite2D propeller;
	AudioStreamPlayer2D engineSound;
	AudioStreamPlayer2D levelUpSound;
	public cannon cannon;

	point_to pointer;

	player_trail lTrail;
	player_trail rTrail;


	// [Export]
	// public PackedScene trailScene;
	// float trailTimer;
	// float trailReplaceTime = 3;
	// int lTrailNumber;
	// int rTrailNumber;
	// bool trailSet = false;
	// bool delTrail1 = true;

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

		lTrail = GetNode<Node2D>("WingTips").GetNode<Node2D>("LeftWingTip").GetNode<player_trail>("PlayerTrail");
		rTrail = GetNode<Node2D>("WingTips").GetNode<Node2D>("RightWingTip").GetNode<player_trail>("PlayerTrail");

		regenTimer = GetNode<Timer>("RegenTimer");
		regenTimer.Start();

		cannon = GetNode<cannon>("Cannon");

		rocketCannonNode = GetNode<Node2D>("RocketCannons");
		rocketC1 = rocketCannonNode.GetNode<rocket_cannon>("RocketCannon1");
		rocketC1.main = main;
		rocketC2 = rocketCannonNode.GetNode<rocket_cannon>("RocketCannon2");
		rocketC2.main = main;

		rTarget = GetNode<rocket_target>("RocketTarget");

		rocketCannons = new rocket_cannon[] { rocketC1, rocketC2 };

		levelUpSound = GetNode<AudioStreamPlayer2D>("LevelUpSound");

		mainSprite.Animation = "default";

		assignStats();
		cannon.assignStats();

		pointer = GetNode<point_to>("PointTo");
		pointer.showLine = true;
		pointer.showPreview = false;
		pointer.inversePointScale = true;

		// trailTimer = trailReplaceTime;
		// lTrail1 = makeTrail(true);
		// rTrail1 = makeTrail(false);
		// lTrail2 = makeTrail(true);
		// rTrail2 = makeTrail(false);

		canAim = true;
	}
	void assignStats()
	{
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

				if (main.curWaveType != main.waveType.boxWave || timer.time > 0)
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

		handleRocketCannons();

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

		velocity = frontVector * curSpeed -GlobalPosition * 0.001f;

		Velocity = velocity;

		spinPropeller();

		if (mainSprite.Animation != "take_damage")
		{
			leanLevel = handleTiltAnim(mainSprite);
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

		//handleTrail((float)delta);
		handlePowerTimers((float)delta);
	}

	player_trail lTrail1 = null;
	player_trail lTrail2 = null;
	player_trail rTrail1 = null;
	player_trail rTrail2 = null;
	// void handleTrail(float delta)
	// {
	// 	trailTimer += delta;

	// 	if (trailTimer >= trailReplaceTime)
	// 	{
	// 		trailTimer = 0;

	// 		if (delTrail1)
	// 		{
	// 			lTrail1.QueueFree();
	// 			rTrail1.QueueFree();
	// 		}
	// 		else
	// 		{
	// 			lTrail2.QueueFree();
	// 			rTrail2.QueueFree();
	// 		}

	// 		trailSet = true;
	// 		if (delTrail1)
	// 		{
	// 			delTrail1 = false;
	// 			lTrail1 = makeTrail(true);
	// 			rTrail1 = makeTrail(false);
	// 		}
	// 		else
	// 		{
	// 			delTrail1 = true;
	// 			lTrail2 = makeTrail(true);
	// 			rTrail2 = makeTrail(false);
	// 		}
	// 	}
	// }
	// player_trail makeTrail(bool left)
	// {
	// 	var trail = trailScene.Instantiate<player_trail>();
	// 	var tips = GetNode<Node2D>("WingTips");
	// 	var rTip = tips.GetNode<Node2D>("RightWingTip");
	// 	var lTip = tips.GetNode<Node2D>("LeftWingTip");

	// 	if (left)
	// 	{
	// 		lTip.AddChild(trail);
	// 	}
	// 	else
	// 	{
	// 		rTip.AddChild(trail);
	// 	}
	// 	return trail;
	// }

	float rocketSpeedMult = 0.7f;
	Plane curRocketTarget;
	Plane prevRocketTarget;
	void handleRocketCannons()
	{	
		// if (rocketTargets.Count > 0)
		// {
		// 	curRocketTarget = getRocketTarget();
		// 	rTarget.target = curRocketTarget;
		// }
		
		rocketC1.spriteScale = rocketC1.baseScale + leanLevel * 0.1f * rocketC1.baseScale;
		rocketC2.spriteScale = rocketC2.baseScale - leanLevel * 0.1f * rocketC2.baseScale;

		if (rocketCount > 0)
		{
			rocketC1.loaded = true;
		}
		else
		{
			rocketC1.loaded = false;
		}
		if (rocketCount > 1)
		{
			rocketC2.loaded = true;
		}
		else
		{
			rocketC2.loaded = false;
		}

		curRocketTarget = getRocketTarget();

		if (curRocketTarget != prevRocketTarget)
		{
			if (curRocketTarget == null)
			{
				rTarget.loseTarget();
			}
			else
			{
				rTarget.getNewTarget();
			}
		}
		prevRocketTarget = curRocketTarget;

		rTarget.target = curRocketTarget;
		rTarget.rocketCount = rocketCount;

		foreach (var rc in rocketCannons)
		{
			rc.startSpeed = curSpeed;
			rc.speedMult = rocketSpeedMult;
			rc.damage = rocketDamage;
			rc.explosionScale = rocketExploScale;
			rc.TargetPosition = frontVector; //cannon.CrosshairPos - rc.GlobalPosition;
		}
	}

	float rocketLockRad = 1000;
	Plane getRocketTarget()
	{
		// float furthest = 0;
		// var target = rocketTargets[0];

		// foreach (var plane in rocketTargets)
		// {
		// 	var dist = (plane.GlobalPosition - GlobalPosition).Length();
		// 	if (dist > furthest)
		// 	{
		// 		furthest = dist;
		// 		target = plane;
		// 	}
		// }
		
		float closest = 300;
		Plane target = null;

		var list = GetTree().GetNodesInGroup("Enemies");
		foreach (var thing in list)
		{	
			if (thing.IsInsideTree())
			{
				var enemy = thing as Plane;

				var dist = Mathf.Abs((enemy.GlobalPosition - cannon.CrosshairPos).Length());
				if (dist < closest)
				{
					closest = dist;
					target = enemy;
				}
			}
		}

		return target;
	}

	void doInputs(double delta)
	{
		if (Input.IsActionPressed("move_forward") && curSpeed <= MaxSpeed)
		{
			var increase = Mathf.Clamp(boostAccel * (float)delta, 0, MaxSpeed - curSpeed);
			curSpeed += increase;
		}
		if (Input.IsActionPressed("move_backward") && curSpeed >= MinSpeed)
		{
			var decrease = Mathf.Clamp(slowDownAccel * (float)delta, 0, MinSpeed + curSpeed);
			curSpeed -= decrease;
		}
		if (Input.IsActionPressed("turn_right") && turnVal < TurnSpeedLimit)
		{
			var turn = turnAccel * (float)delta;
			if (turnVal < 0)
			{
				turn *= 1.3f;
			}
			turn = Mathf.Clamp(turn, 0, TurnSpeedLimit - turnVal);
			turnVal += turn;
		}
		if (Input.IsActionPressed("turn_left") && turnVal > -TurnSpeedLimit)
		{
			var turn = turnAccel * (float)delta;
			if (turnVal > 0)
			{
				turn *= 1.3f;
			}
			turn = Mathf.Clamp(turn, 0, TurnSpeedLimit + turnVal);
			turnVal -= turn;
		}

		if (Input.IsActionJustPressed("toggle_aim"))
		{
			canAim = !canAim;
		}

		if (Input.IsActionJustPressed("fire_rocket"))
		{
			fireRocket(getRocketCannon());
		}
	}

	public void getRockets(int count)
	{
		rocketCount += count;
	}
	int lastRocketIndex = 0;
	rocket_cannon getRocketCannon()
	{
		if (rocketCount == 1)
		{
			return rocketC1;
		}
		else
		{
			if (lastRocketIndex == 0)
			{
				lastRocketIndex = 1;
				return rocketC1;
			}
			else if (lastRocketIndex == 1)
			{
				lastRocketIndex = 0;
				return rocketC2;
			}
		}
		return null;
	}

	void fireRocket(rocket_cannon rc)
	{
		if (rocketCount > 0)
		{
			rocketCount -= 1;
			rc.shootRocket(curRocketTarget);
		}
	}

	void spinPropeller()
	{
		var playSpeed = 1 + Mathf.Clamp(Mathf.InverseLerp(MinSpeed, MaxSpeed, curSpeed), 0.05f, 1);
		propeller.SpeedScale = 1 + playSpeed;

		engineSound.PitchScale = 0.75f + Mathf.Clamp(Mathf.InverseLerp(MinSpeed, MaxSpeed, curSpeed), 0.05f, 1) * 0.4f;
		engineSound.VolumeDb = -5 + Mathf.Clamp(Mathf.InverseLerp(MinSpeed, MaxSpeed, curSpeed), 0.05f, 1) * 4;
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
	public void stopTrails()
	{
		lTrail.stopDrawing();
		rTrail.stopDrawing();
	}
	public void startTrails()
	{
		lTrail.startDrawing();
		rTrail.startDrawing();
	}

	public void playKillSound()
	{
		GetNode<AudioStreamPlayer>("KillSound").Play();
	}

	List<Plane> rocketTargets = new List<Plane>();

	// SIGNAL METHODS

	void _on_target_area_body_entered(Node2D body)
	{
		if (body is Plane)
		{
			var target = body as Plane;
			rocketTargets.Add(target);
		}
	}
	void _on_target_area_body_exited(Node2D body)
	{
		if (body is Plane)
		{
			var target = body as Plane;
			if (rocketTargets.Contains(target))
			{
				rocketTargets.Remove(target);
			}
		}
	}

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

					if (powerUp.rocketCount > 0)
					{
						getRockets(powerUp.rocketCount);
						return;
					}

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
				powerTimer.agilityTemp.first = true;
				powerTimer.agilityTemp.time = 0;
				powerTimer.agilityTemp.shouldIncrement = true;
				break;
			}
			case "SpeedPack":
			{
				powerTimer.speedTemp.first = true;
				powerTimer.speedTemp.time = 0;
				powerTimer.speedTemp.shouldIncrement = true;
				break;
			}
			case "RegenPack":
			{
				powerTimer.regenTemp.first = true;
				powerTimer.regenTemp.time = 0;
				powerTimer.regenTemp.shouldIncrement = true;
				break;
			}
			case "FireSpeedPack":
			{
				powerTimer.fireSpeedTemp.first = true;
				powerTimer.fireSpeedTemp.time = 0;
				powerTimer.fireSpeedTemp.shouldIncrement = true;
				break;
			}
			case "DamAccPack":
			{
				powerTimer.damAccTemp.first = true;
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
		fireSpeedBonus += 0.00125f;
		heatUpBonus += 0.0000075f;
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
	public void clearRockets()
	{
		rocketCount = 0;
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
