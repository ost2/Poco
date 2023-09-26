using Godot;
using System;

public partial class main : Node
{
	public hud hud;
	main_menu mainMenu;
	pause_menu pauseMenu;
	public options_menu optionsMenu;

	float shakeTimer;
	float maxShakeTime;
	float shakeFreq;

	public float shakeMult = 1;

	public Resolution curResolution = Resolution.r1080p;

	// WORLD
	public bool gameRunning = false;
	public bool doMainMenu = false;

	public enum difficulty
	{
		easy, 
		medium,
		hard,
	}

	public difficulty curDifficulty;

	public FastNoiseLite cloudNoise;

	public enum waveType 
	{
		enemyWave,
		boxWave,
		bossWave,
		nullWave,
	}

	public waveType curWaveType;
	public waveType lastWaveType;

	Timer waveTimer;
	public int waveNumber;
	public int killsThisWave;
	public int killsToProgress;

	public Vector2 lastEnemyKilledPos;

	AudioStreamPlayer2D bossMusic;

	// PLAYER STATS
	public float PlayerHealth { get { return player.health; } }
	public bool PCanAim { get { return player.canAim; } }
	public Vector2 PlayerPos { get { return player.Position; } }
	
	public int playerKills;
	public float PCurXP { get { return player.curXp; } }
	public float PXpToLevel { get { return player.xpToLevel; } }

	public int PAgilityLevel { get { return player.agilityLvl; } }
	public int PSpeedLevel { get { return player.speedLvl; } }
	public int PRegenLevel { get { return player.regenLvl; } }
	public int PFireSpeedLevel { get { return player.fireSpeedLvl; } }
	public int PDamAccLevel { get { return player.damAccLvl; } }
	public int PTotalLevel { get { return player.level; } }
	
	public int PHasLevels { get { return player.hasLevels; } }

	float gameTimer;
	public TimeSpan GameTimer { get { return TimeSpan.FromSeconds(gameTimer); } }
	public bool GameOver { get { return player.isDead; } }

	// MOUSE
	public Vector2 MousePos { get { return mainCam.GetGlobalMousePosition(); } }
	public Vector2 MouseVector { get { return MousePos - PlayerPos; } }

	// CHILD NODES
	public player player;
	public Camera2D mainCam;
	Node2D clouds;
	Node2D background;
	Path2D enemyPath;
	PathFollow2D enemyPoint;

	Path2D boxPath;
	PathFollow2D boxPoint;

	// ENEMIES
	[Export]
	public PackedScene enemyPlaneScene;
	float lvl1EnemySpawnTime = 15.0f;
	float lvl2EnemySpawnTime = 45.0f;
	float lvl3EnemySpawnTime = 120.0f;

	float lvl1EnemyTimer = 0.0f;
	float lvl2EnemyTimer = 0.0f;
	float lvl3EnemyTimer = 0.0f;

	[Export]
	public PackedScene bossPlaneScene;
	
	public float DifficultyMult
	{ 
		get 
		{ 
			switch (curDifficulty)
			{
				case difficulty.easy:
				{
					return 0.65f;
				}
				case difficulty.medium:
				{
					return 0.9f;
				}
				case difficulty.hard:
				{
					return 1.15f;
				}
				default:
				{
					return 1.0f;
				}
			}
		}
	}


	public float enemyTimerMult { get { return (waveNumber * 0.17f) + 1; } }
	public float enemyRange = 700.0f;

	public int EnemyNumber { get { var enemies = GetTree().GetNodesInGroup("Enemies"); return enemies.Count; } }

	// STUFF
	[Export]
	public PackedScene explosionScene;
	[Export]
	public PackedScene boxScene;
	[Export]
	public PackedScene powerUpScene;
	float boxTime = 25.0f;
	float boxTimer = 0.0f;

	void assignNodes()
	{
		hud = GetNode<hud>("HUD");
		mainMenu = GetNode<main_menu>("MainMenu");
		pauseMenu = GetNode<pause_menu>("PauseMenu");
		optionsMenu = GetNode<options_menu>("OptionsMenu");
		player = GetNode<player>("Player");
		mainCam = GetNode<Camera2D>("PlayerCamera");
		clouds = GetNode<Node2D>("Clouds");
		background = GetNode<Node2D>("Background");
		enemyPath = mainCam.GetNode<Path2D>("EnemySpawnPath");
		enemyPoint = enemyPath.GetNode<PathFollow2D>("EnemySpawnPoint");
		boxPath = mainCam.GetNode<Path2D>("BoxSpawnPath");
		boxPoint = boxPath.GetNode<PathFollow2D>("BoxSpawnPoint");
		waveTimer = GetNode<Timer>("WaveTimer");
		bossMusic = GetNode<AudioStreamPlayer2D>("BossMusic");
	}

	void setStartingValues()
	{
		mainCam.Position = player.Position;

		gameRunning = false;
		pauseMenu.Hide();
		optionsMenu.Hide();

		Input.MouseMode = Input.MouseModeEnum.Hidden;

		lvl1EnemySpawnTime = 15.0f;
		lvl2EnemySpawnTime = 45.0f;
		lvl3EnemySpawnTime = 120.0f;

		lvl1EnemyTimer = 0.0f;
		lvl2EnemyTimer = 0.0f;
		lvl3EnemyTimer = 0.0f;

		playerKills = 0;
		killsToProgress = 3;

		waveNumber = 1;
		bossWaveNumber = 0;
		bossNumber = 1;
		bossLevel = 1;

		player.curXp = 0;
		player.xpToLevel = 60;
		player.health = 100;
		player.cantMove = false;
		player.isDead = false;
		player.resetLevels();
		player.Show();

		hud.stopBoxClock();

		player.Velocity = Vector2.Zero;
		player.cannon.realFireTime = player.fireTime;

		gameTimer = 0.0f;

		curWaveType = waveType.nullWave;
		waveTimer.Stop();
	}
	public void resetGame()
	{
		clearBoxes();
		clearEnemies();
		clearPowerUps();

		setStartingValues();

		doEnemyWave();
		//doBossWave();

		hud.showWaveText();
		hud.stopBoxClock();
	}

	// MENUS

	public void startGame()
	{
		hideMainMenu();
		showHud();

		resetGame();
	}
	public void showHud()
	{
		hud.Show();
	}
	public void toggleMainMenu()
	{
		if (doMainMenu)
		{
			hideMainMenu();
		}
		else
		{
			showMainMenu();
		}
	}
	void removeAllTemporary()
	{
		foreach (var timer in powerTimer.powerTimers)
		{
			timer.finish();
			hud.startClock(timer.powID, 0, true);
		}
	}
	
	void showMainMenu()
	{
		curMenu = "MainMenu";

		clearBoxes();
		clearEnemies();
		clearPowerUps();

		gameRunning = false;
		doMainMenu = true;

		mainMenu.Show();
		hud.Hide();
		hud.stopBoxClock();	

		removeAllTemporary();

		Input.MouseMode = Input.MouseModeEnum.Visible;
	}
	void hideMainMenu()
	{
		doMainMenu = false;

		mainMenu.Hide();
		Input.MouseMode = Input.MouseModeEnum.Hidden;
	}
	public string curMenu;
	public void showOptions()
	{
		if (curMenu == "PauseMenu")
		{
			iShit = true;
			GetTree().Paused = false;
			mainCam.Position = player.Position - new Vector2(x: 0, y: -165);
			GetNode<Timer>("IShitTimer").Start();
		}
		optionsMenu.Show();

		hud.Hide();
		mainMenu.Hide();
		pauseMenu.Hide();
	}
	bool iShit = false;
	public void hideOptions()
	{
		optionsMenu.Hide();

		if (curMenu == "MainMenu")
		{
			mainMenu.Show();
		}
		if (curMenu == "PauseMenu")
		{
			hud.Show();
			pauseMenu.Show();
		}
	}

	public void clearBoxes()
	{
		GetTree().CallGroup("Boxes", "despawnBox");
	}
	public void clearPowerUps()
	{
		GetTree().CallGroup("PowerUps", "despawn");
	}

	public void clearEnemies()
	{
		var enemies = GetTree().GetNodesInGroup("Enemies");
		foreach (var enemy in enemies)
		{
			enemy.QueueFree();
		}
	}

	// §READY
	public override void _Ready()
	{
		curDifficulty = difficulty.easy;

		assignNodes();
		setStartingValues();

		showMainMenu();

		baseZoom = mainCam.Zoom;
	}

	// §PROCESS
	public override void _PhysicsProcess(double delta)
	{
		if (!GameOver && !doMainMenu)
		{
			if (gameTimer > 0.1f)
			{
				gameRunning = true;
			}

			gameTimer += (float)delta;

			//updateCloudThickness();

			if (gameRunning && ((killsThisWave >= killsToProgress && curWaveType == waveType.enemyWave) || (bossDead && curWaveType == waveType.bossWave)))
			{
				doBoxWave();
			}
			else if (curWaveType != waveType.nullWave)
			{
				incrementEnemyTimers((float)delta);
				incrementOtherTimers((float)delta);
			}

			handleBossMusic((float)delta);
			devCommands();

			//updateCloudThickness();
		}

		moveCamera((float)delta);
		moveClouds((float)delta);
	}


	Vector2 baseZoom;

	float shakeStrength;
	public void startScreenShake(float strength, float time)
	{
		var potStrength = strength * 2.5f;
		if (!(shakeTimer < maxShakeTime && shakeStrength > potStrength))
		{
			shakeTimer = 0;
			maxShakeTime = time;
			shakeFreq = 0;
			shakeStrength = potStrength;
		}
	}
	void doScreenShake(float delta)
	{
		var shakeLerp = Mathf.InverseLerp(0, maxShakeTime, shakeTimer);

		shakeFreq = Mathf.Cos(shakeLerp) * 7;
		var shakeVal = Mathf.Cos(shakeLerp * shakeFreq * 5) * (shakeStrength * (1 - shakeLerp)) * shakeMult * 1.5f;

		mainCam.Offset = new Vector2(x: shakeVal, y: shakeVal);

		shakeTimer += delta;
	}

	float camSpeed = 8;
	void moveCamera(float delta)
	{
		mainCam.Zoom = baseZoom * mainMenu.getScaleFactor();

		if (!iShit)
		{
			if (!GameOver && !doMainMenu)
			{
				var dist = 0; //camDistance - player.curSpeed / 3;
				//mainCam.Position 

				var camPoint = player.Position + (player.frontVector * dist) + MouseVector * 0.3f;

				var dirVec = (camPoint - mainCam.Position).Normalized();
				var length = (camPoint - mainCam.Position).Length();

				mainCam.Position += dirVec * length * delta * camSpeed;
			}
			else if (doMainMenu)
			{
				mainCam.Position = player.Position - new Vector2(x: 0, y: -165);
			}
		}

		if (shakeTimer < maxShakeTime)
		{
			doScreenShake(delta);
		}
		else
		{
			mainCam.Offset = Vector2.Zero;
		}

		//var paraLayer = GetNode<ParallaxBackground>("ParallaxBackground").GetNode<ParallaxLayer>("ParallaxLayer");
		//paraLayer.MotionOffset = -(player.frontVector * camDistance);
	}

	// PLAYER

	public void pDie()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;

		player.hasLevels = 0;
	
		var deathScreenTimer = GetNode<Timer>("ShowDeathScreenTimer");
		deathScreenTimer.Start();
	}

	// ENEMIES

	int bigChance = 30;
	void incrementEnemyTimers(float delta)
	{
		float waveTypeMult = curWaveType == waveType.enemyWave ? 1 : 0;
		if (curWaveType == waveType.bossWave)
		{
			waveTypeMult = 0.5f;
		}

		lvl1EnemyTimer += delta * enemyTimerMult * waveTypeMult * DifficultyMult;
		lvl2EnemyTimer += delta * (enemyTimerMult * 1.5f) * waveTypeMult * DifficultyMult;
		lvl3EnemyTimer += delta * (enemyTimerMult * 2.0f) * waveTypeMult * DifficultyMult;

		var waveNumVar = 1 - (waveNumber * 0.05f);

		var rand = new Random();

		var bigMode = false;
		if (waveNumber >= 6)
		{
			bigMode = rand.Next(0, (int)(bigChance * Mathf.Clamp(waveNumVar, 0.2f, 1))) == 0 ? true : false;
		}

		var lvl = 1;
		var shouldSpawn = false;

		if (killsThisWave + EnemyNumber < killsToProgress)
		{
			if (lvl1EnemyTimer >= lvl1EnemySpawnTime)
			{
				lvl = 1;
				lvl1EnemyTimer = 0.0f;
				shouldSpawn = true;
			}
			else if (lvl2EnemyTimer >= lvl2EnemySpawnTime)
			{
				if (waveNumber >= 2)
				{
					lvl = 2;
				}
				lvl2EnemyTimer = 0.0f;
				shouldSpawn = true;
			}
			else if (lvl3EnemyTimer >= lvl3EnemySpawnTime)
			{
				if (waveNumber >= 3)
				{
					lvl = 3;
				}
				lvl3EnemyTimer = 0.0f;
				shouldSpawn = true;
			}

			if (shouldSpawn)
			{
				spawnEnemyPlane(lvl, bigMode);
			}
		}
	}
	void spawnEnemyPlane(int lvl = 1, bool big = false)
	{
		enemy_plane enemy = enemyPlaneScene.Instantiate<enemy_plane>();

		enemyPoint.ProgressRatio = GD.Randf();

		enemy.Position = enemyPoint.Position + mainCam.Position;
		enemy.enemyLevel = lvl;

		enemy.bigMode = big;
		
		AddChild(enemy);
	}
	void spawnBossPlane(int lvl = 1)
	{
		boss_plane boss = bossPlaneScene.Instantiate<boss_plane>();

		enemyPoint.ProgressRatio = GD.Randf();

		boss.Position = enemyPoint.Position + mainCam.Position;
		boss.bossLevel = lvl;

		AddChild(boss);
	}
	void spawnBox(bool onMouse = false)
	{
		var rand = new RandomNumberGenerator();
		box box = boxScene.Instantiate<box>();

		// boxPoint.ProgressRatio = GD.Randf();

		var pos = new Vector2(x: rand.RandfRange(-1000, 1000), y: rand.RandfRange(-1000, 1000));
		
		var boxDistance = rand.RandfRange(1500, 2500);

		var boxVec = pos - player.GlobalPosition.Normalized() * boxDistance;
		var boxPos = player.GlobalPosition + boxVec;
		
		box.Position = onMouse ? MousePos : boxPos; //boxPoint.Position + mainCam.Position;

		AddChild(box);
	}

	public void spawnExplosion(CharacterBody2D plane = null, float scale = 1, float volume = -10, bullet bullet = null, float speed = 1, float randomPos = 0, float damage = 0, float pitch = 0.7f)
	{
		explosion explo = explosionScene.Instantiate<explosion>();
		if (plane != null)
		{
			var rand = new RandomNumberGenerator();
			explo.vel = plane.Velocity;
			explo.pos = plane.Position + new Vector2(x: rand.RandfRange(-randomPos, randomPos), y: rand.RandfRange(-randomPos, randomPos));
			explo.size = new Vector2(x: scale, y: scale);
			explo.volume = volume;
			explo.pitch = pitch;
			explo.damage = damage;

			if (bullet != null)
			{
				explo.pos = bullet.Position;
			}
		}
		CallDeferred("add_child", explo);

		startScreenShake(scale * 3, scale);
	}

	public bool maybeSpawnRockets(float P, Vector2 pos, float countP)
	{
		var rnd = new RandomNumberGenerator();

		var move = curDifficulty == difficulty.hard ? true : true;
		if (rnd.RandfRange(0, 100) <= P)
		{
			spawnPowerUp(pos, move, getRandomRocketID(countP));
			return true;
		}
		else return false;
	}

	public bool maybeSpawnPower(float P, Vector2 pos, bool tempOverride)
	{
		var rnd = new RandomNumberGenerator();

		var move = curDifficulty == difficulty.hard ? true : true;
		if (rnd.RandfRange(0, 100) <= P)
		{
			spawnPowerUp(pos, move, getRandomPowerID(true), tempOverride);
			return true;
		}
		else return false;
	}
	// BOXES
	void incrementOtherTimers(float delta)
	{
		float boxMult = curWaveType == waveType.boxWave ? 5 : 1;
		boxTimer += delta * boxMult;

		if (boxTimer >= boxTime)
		{
			spawnBox();
			boxTimer = 0.0f;
		}
	}
	int tempChance = 3;
	public void spawnPowerUp(Vector2 pos, bool moveToPlayer = false, string powID = null, bool tempOverride = false)
	{
		power_up power = powerUpScene.Instantiate<power_up>();
		power.powerID = powID != null ? powID : getRandomPowerID();
		power.Position = pos;
		power.moveToPlayer = moveToPlayer;

		switch (power.powerID)
		{
			case "OneRocket":
				power.rocketCount = 1;
				break;
			case "TwoRocket":
				power.rocketCount = 2;
				break;
			case "ThreeRocket":
				power.rocketCount = 3;
				break;
			default:
				power.rocketCount = 0;
				break;
		}

		var rand = new Random();
		if (power.powerID != "HealthPack" && (rand.Next(0, tempChance) == 0 || tempOverride) && power.rocketCount == 0)
		{
			power.isTemporary = true;
		}

		CallDeferred("add_child", power);
	}
	string getRandomPowerID(bool noHealth = false)
	{
		var rand = new Random();
		if (noHealth)
		{
			return rand.Next(0, 5) switch
			{
				0 => "AgilityPack",
				1 => "SpeedPack",
				2 => "FireSpeedPack",
				3 => "DamAccPack",
				4 => "RegenPack",
				_ => "Null",
			};
		}
        return rand.Next(0, 12) switch
        {
			0 => "AgilityPack",
			1 => "SpeedPack",
			2 => "FireSpeedPack",
			3 => "DamAccPack",
			4 => "RegenPack",
            _ => "HealthPack",
        };
    }
	
	string getRandomRocketID(float P)
	{
		var rnd = new RandomNumberGenerator();

		var roll = rnd.RandfRange(0, 100);
		if (roll < P * 0.3f)
		{
			return "ThreeRocket";
		}
		else if (roll < P * 0.6f)
		{
			return "TwoRocket";
		}
		else
		{
			return "OneRocket";
		}
	}

	// CLOUDS
	void moveClouds(float delta)
	{	
		if (!GameOver)
		{
			var pos = player.GlobalPosition;// + player.frontVector * player.Velocity * delta;

			GetNode<Node2D>("LowerClouds").Position = pos;
			background.Position = player.Position;
			clouds.Position = pos;
			var cloudsSprite = clouds.GetNode<Sprite2D>("CloudsSprite");

			var noiseTexture = cloudsSprite.Texture as NoiseTexture2D;
			// noiseTexture.Width = (int)(480 * mainMenu.getScaleFactor());
			// noiseTexture.Height = (int)(360 * mainMenu.getScaleFactor());
			
			cloudNoise = noiseTexture.Noise as FastNoiseLite;
			cloudNoise.Offset = new Vector3(x: pos.X / 6, y: pos.Y / 6, z: 0);
		}
	}

	void updateCloudThickness()
	{
		var cloudsMap = GetNode<Sprite2D>("CloudsMap");
		var noiseTexture = cloudsMap.Texture as NoiseTexture2D;
		var noise = noiseTexture.Noise as FastNoiseLite;

		var thicknessVal = noise.GetNoise2D(player.Position.X, player.Position.Y) * 0.5f;

		var col = new Color(255, 255, 255, 0.5f + thicknessVal);
		clouds.Modulate = col;
		clouds.SelfModulate = col;
	}

	// WAVES

	void doEnemyWave()
	{
		hud.checkStartClock();

		killsThisWave = 0;
		killsToProgress += 4;

		curWaveType = waveType.enemyWave;
		lastWaveType = waveType.enemyWave;
	}
	void doBoxWave()
	{
		waveTimer.Start();
		hud.startBoxClock((float)waveTimer.TimeLeft);
		hud.showWaveText();
		curWaveType = waveType.boxWave;
	}
	public bool bossDead = false;
	float bossNumber = 1;
	float deadBossNumber = 0;
	int bossLevel = 1;
	
	public int bossWaveNumber = 0;
	void doBossWave()
	{
		hud.checkStartClock();
		
		bossMusic.VolumeDb = 0;
		bossMusic.Play();

		bossWaveNumber++;
		curWaveType = waveType.bossWave;
		lastWaveType = waveType.bossWave;

		bossDead = false;
		
		for (int i = 0; i < bossNumber; i++)
		{
			spawnBossPlane(bossLevel);
		}
		
		if (bossWaveNumber % 3 == 0)
		{
			bossNumber++;
		}
		bossLevel++;
	}
	void handleBossMusic(float delta)
	{
		bossMusic.Position = PlayerPos;
		if (bossDead || GameOver)
		{
			bossMusic.VolumeDb -= delta * 3.5f;
		}
		if (bossMusic.VolumeDb < -50)
		{
			bossMusic.Stop();
		}
	}

	public void bossDie()
	{
		deadBossNumber++;
		if (deadBossNumber >= bossNumber)
		{
			bossDead = true;
		}
	}

	void devCommands()
	{
		var rand = new Random();
		if (Input.IsActionJustPressed("explode"))
		{
			player.takeDamage(player.maxHealth * 2);
		}
		if (Input.IsActionJustPressed("spawn_health_pack"))
		{
			spawnPowerUp(MousePos, false, "OneRocket");
			//spawnPowerUp(MousePos, false, "HealthPack");
			//spawnPowerUp(MousePos, false, "AgilityPack");
		}
		if (Input.IsActionJustPressed("minigun"))
		{
			player.minigun();
		}
		if (Input.IsActionJustPressed("sniper"))
		{
			player.sniper();
		}
		if (Input.IsActionJustPressed("acrobat"))
		{
			player.acrobat();
		}
		if (Input.IsActionJustPressed("zoomin"))
		{
			player.zoomin();
		}
		if (Input.IsActionJustPressed("spawn_explosion"))
		{
			spawnExplosion(player, 1, -10, null, 1, 300);
		}
		if (Input.IsActionJustPressed("spawn_boss"))
		{
			spawnBossPlane(rand.Next(1, 4));
		}
		if (Input.IsActionJustPressed("spawn_enemy"))
		{
			spawnEnemyPlane(rand.Next(1, 4), false);
		}
		if (Input.IsActionJustPressed("spawn_big_enemy"))
		{
			spawnEnemyPlane(rand.Next(1, 4), true);
		}
		if (Input.IsActionJustPressed("spawn_box"))
		{
			spawnBox(true);
		}
	}

	// SIGNAL METHODS

	void _on_show_death_screen_timer_timeout()
	{
		hud.showDeathScreen = true;
	}

	void _on_wave_timer_timeout()
	{
		lvl1EnemyTimer = lvl1EnemySpawnTime * 0.8f;
		lvl2EnemyTimer = lvl2EnemySpawnTime * 0.8f;
		lvl3EnemyTimer = lvl3EnemySpawnTime * 0.8f;

		clearBoxes();
		waveNumber++;
		hud.showWaveText();

		if (waveNumber % 5 == 0)
		{
			doBossWave();
			return;
		}
		else 
		{
			doEnemyWave();
			return;
		}
	}

	void _on_i_shit_timer_timeout()
	{
		GetTree().Paused = true;
		iShit = false;
	}
}
