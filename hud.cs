using Godot;
using System;

public partial class hud : menu
{
	clock boxClock;
	Label fpsLabel;
	Label noiseValLabel;
	Label tempLabel;

	Label timerLabel;

	death_screen deathScreen;

	wave_text waveText;

	fancy_bar healthBar;
	fancy_bar overHealBar;
	fancy_bar xpBar;

	Node2D killsElement;
	Label killsLabel;
	Sprite2D killsSprite;

	Node2D lvlElement;
	Label lvlLabel;
	Sprite2D lvlSprite;

	Node2D xpElement;

	stat_ui agilityUI;
	stat_ui speedUI;
	stat_ui regenUI;
	stat_ui fireSpeedUI;
	stat_ui damAccUI;


	Node2D waveNumberElement;
	AnimatedSprite2D waveNumberSprite;
	Label waveNumberLabel;

	Label statUpLabel;
	Label levelUpLabel;

	Vector2 statUpLabelStartPos;

	public bool showDeathScreen = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		boxClock = GetNode<clock>("BoxClock");

		main = GetParent<main>();

		fpsLabel = GetNode<Label>("FPSLabel");
		noiseValLabel = GetNode<Label>("CloudValueLabel");
		tempLabel = GetNode<Label>("TempBoostLabel");

		timerLabel = GetNode<Label>("TimerLabel");

		deathScreen = GetNode<death_screen>("DeathScreen");
		waveText = GetNode<wave_text>("WaveText");

		healthBar = GetNode<fancy_bar>("HealthBar");
		healthBar.leftSide = true;
		overHealBar = GetNode<fancy_bar>("OverHealBar");
		overHealBar.leftSide = true;

		xpBar = GetNode<fancy_bar>("XpBar");
		xpBar.showNegative = false;
		xpBar.leftSide = false;

		killsElement = GetNode<Node2D>("KillsElement");
		killsLabel = killsElement.GetNode<Label>("KillsLabel");
		killsSprite = killsElement.GetNode<Sprite2D>("KillsSprite");

		lvlElement = GetNode<Node2D>("LvlElement");
		lvlLabel = lvlElement.GetNode<Label>("LvlLabel");
		lvlSprite = lvlElement.GetNode<Sprite2D>("LvlSprite");

		xpElement = GetNode<Node2D>("XpElement");

		agilityUI = xpElement.GetNode<stat_ui>("AgilityUI");
		speedUI = xpElement.GetNode<stat_ui>("SpeedUI");
		regenUI = xpElement.GetNode<stat_ui>("RegenUI");
		fireSpeedUI = xpElement.GetNode<stat_ui>("FireSpeedUI");
		damAccUI = xpElement.GetNode<stat_ui>("DamAccUI");

		waveNumberElement = GetNode<Node2D>("WaveNumberElement");
		waveNumberSprite = waveNumberElement.GetNode<AnimatedSprite2D>("WaveNumberSprite");
		waveNumberLabel = waveNumberElement.GetNode<Label>("WaveNumberLabel");

		statUpLabel = xpElement.GetNode<Label>("StatUpLabel");
		levelUpLabel = xpElement.GetNode<Label>("LevelUpLabel");
	
		agilityUI.Play();
		speedUI.Play();
		regenUI.Play();
		fireSpeedUI.Play();
		damAccUI.Play();

		waveText.Hide();

		baseOffset = Offset;
		baseScale = Scale;

		statUpLabelStartPos = statUpLabel.Position;

		statUIs = new stat_ui[] { agilityUI, speedUI, regenUI, fireSpeedUI, damAccUI };

		boxClock.showShadow = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	stat_ui[] statUIs;

	public float arrowTime = 0;
	public float arrowFreq = 4;

	public override void _Process(double delta)
	{
		setOffset();
		
		fpsLabel.Text = "FPS: " + Engine.GetFramesPerSecond();
		//noiseValLabel.Text = "CLOUD: " + main.cloudNoise.GetNoise2D(main.PlayerPos.X, main.PlayerPos.Y).ToString("N3");

		tempLabel.Text = main.player.agilityTempBonus + ", " + main.player.speedTempBonus + ", " + main.player.regenTempBonus + ", " + main.player.fireSpeedTempBonus + ", " + main.player.damAccTempBonus;

		timerLabel.Text = String.Format("{0:mm\\:ss}", main.GameTimer);

		healthBar.Value = main.PlayerHealth;
		healthBar.MaxValue = main.player.maxHealth;

		overHealBar.Value = main.player.overHeal;
		overHealBar.MaxValue = main.player.maxHealth;

		xpBar.Value = main.PCurXP;
		xpBar.MaxValue = main.PXpToLevel;
		
		switch (main.lastWaveType)
		{
			case main.waveType.enemyWave:
			{
				killsLabel.Text = main.killsThisWave + "/" + main.killsToProgress;
				break;
			}
			case main.waveType.bossWave:
			{
				killsLabel.Text = main.bossDead ? "1 / 1" : "0 / 1";
				break;
			}
		}

		lvlLabel.Text = main.PHasLevels > 0 ? main.PTotalLevel + " + " + main.PHasLevels : "X  " + main.PTotalLevel;

		arrowTime += (float)delta * arrowFreq;

		agilityUI.setStatLevel(main.PAgilityLevel);
		speedUI.setStatLevel(main.PSpeedLevel);
		regenUI.setStatLevel(main.PRegenLevel);
		fireSpeedUI.setStatLevel(main.PFireSpeedLevel);
		damAccUI.setStatLevel(main.PDamAccLevel);

	
		var hpBaseColor = new Color(1, 0, 0, 1);
		if (hpFlashTimer < 1.0f)
		{
			hpFlashTimer += (float)delta * 2;
			
			healthBar.TintProgress = new Color(hpBaseColor.R + Mathf.Sin(hpFlashTimer * 3), hpBaseColor.G + Mathf.Sin(hpFlashTimer * 3), hpBaseColor.B + Mathf.Sin(hpFlashTimer * 3), hpBaseColor.A);
		}
		else 
		{
			healthBar.TintProgress = hpBaseColor;
		}
		
		var overHealBaseColor = new Color(1, 2, 0, 1);
		if (overHealFlashTimer < 1.0f)
		{
			overHealFlashTimer += (float)delta * 2;

			overHealBar.TintProgress = new Color(overHealBaseColor.R + Mathf.Sin(overHealFlashTimer * 3) * 4, overHealBaseColor.G + Mathf.Sin(overHealFlashTimer * 3), overHealBaseColor.B + Mathf.Sin(overHealFlashTimer * 3) * 3);
		}
		else
		{
			overHealBar.TintProgress = overHealBaseColor;
		}

		var xpBaseColor = new Color(0, 1, 0, 1);
		if (xpFlashTimer < 1.0f)
		{
			xpFlashTimer += (float)delta * 2;

			xpBar.TintProgress = new Color(xpBaseColor.R + Mathf.Sin(xpFlashTimer * 3) * 0.5f, xpBaseColor.G + Mathf.Sin(xpFlashTimer * 3), xpBaseColor.B + Mathf.Sin(xpFlashTimer * 3) * 0.5f, xpBaseColor.A);
		}
		else 
		{
			xpBar.TintProgress = xpBaseColor;
		}

		switch (main.curWaveType)
		{
			case main.waveType.boxWave:
			{
				waveNumberSprite.Animation = "box_wave";
				break;
			}
			case main.waveType.enemyWave:
			case main.waveType.bossWave:
			{
				waveNumberSprite.Animation = "enemy_wave";
				break;
			}
		}
		waveNumberLabel.Text = main.waveNumber.ToString();

		if (showDeathScreen)
		{
			doDeathScreen();
		}
		else 
		{
			dontDeathScreen();
			doStatUpText((float)delta);
			doLevelUpText((float)delta);
		}
	}

	public void checkStartClock()
	{
		foreach (var statUI in statUIs)
		{
			if (statUI.shouldStartClock && !statUI.clockRunning)
			{
				statUI.startClock(powTime);
			}
		}
	}
	public void startBoxClock(float t)
	{
		boxClock.start(t);
	}

	void doStatUpText(float delta)
	{
		if (statUpLabelTimer < 1.5f)
		{
			statUpLabel.Show();
			statUpLabelTimer += delta * 1.5f;

			var baseCol = statUpLabel.SelfModulate;
			statUpLabel.SelfModulate = new Color(baseCol.R, baseCol.G, baseCol.B, Mathf.Sin(statUpLabelTimer * 2));

            var mark = new Marker2D { Position = statUpLabel.Position };

            surgeAnimation(mark, delta, statUpLabelTimer * 2, 1.5f, 1.05f, 0, -20);
			
			statUpLabel.Position = mark.Position;
			statUpLabel.Scale = mark.Scale;
		}
		else 
		{
			statUpLabel.Hide();
		}
	}
	float levelUpLabelTimer = 2;
	void doLevelUpText(float delta)
	{
		if (levelUpLabelTimer < 1.5f)
		{
			levelUpLabel.Show();
			levelUpLabelTimer += delta * 1.5f;

			var baseCol = levelUpLabel.SelfModulate;
			levelUpLabel.SelfModulate = new Color(baseCol.R, baseCol.G, baseCol.B, Mathf.Sin(levelUpLabelTimer * 2));
		}
		else 
		{
			levelUpLabel.Hide();
		}
	}

	void doDeathScreen()
	{
		deathScreen.Show();

		waveNumberElement.Hide();
		killsElement.Hide();
		lvlElement.Hide();
		timerLabel.Hide();
		healthBar.Hide();
		overHealBar.Hide();
		xpBar.Hide();

		// foreach (var statUI in statUIs)
		// {
		// 	statUI.Hide();
		// }
	}

	void dontDeathScreen()
	{
		deathScreen.Hide();

		waveNumberElement.Show();
		killsElement.Show();
		lvlElement.Show();
		//timerLabel.Show();
		healthBar.Show();
		overHealBar.Show();
		xpBar.Show();
	}
	public void showWaveText()
	{
		waveText.hideTimer.Start();
		waveText.Show();
	}

	void doArrowFlash()
	{
		foreach (var statUI in statUIs)
		{
			if (!statUI.showArrow)
			{
				statUI.doArrow();
			}
		}
	}
	public void doHudLevelUp()
	{
		doArrowFlash();
		foreach (var statUI in statUIs)
		{
			statUI.doButton();
		}
		levelUpLabelTimer = 0.0f;
	}

	public void statButtonPressed(string statID)
	{
		doStatUp();
		main.player.getLevel(statID);

		arrowTime = 0;
		doStatUpAnimation(statID);

		if (main.PHasLevels <= 0)
		{
			foreach (var statUI in statUIs)
			{
				statUI.dontArrow();
				statUI.dontButton();
			}
		}
	}

	void doStatUp()
	{
		main.player.hasLevels -= 1;
		var statUpSound = GetNode<AudioStreamPlayer2D>("StatUpSound");
		statUpSound.GlobalPosition = main.PlayerPos;
		statUpSound.Play();
	}

	void surgeAnimation(Node2D node, float delta, float t, float time, float scaleMult, float xMove = 0, float yMove = 0)
	{
		var lerp = Mathf.InverseLerp(0, time, t);
		var scaleVal = 1 + Mathf.Sin(lerp) * scaleMult;

		float xPos = 0;
		float yPos = 0;

		if (lerp <= 2)
		{
			xPos = node.Position.X + Mathf.Cos(lerp * 2) * xMove * delta;
			yPos = node.Position.Y + Mathf.Cos(lerp * 2) * yMove * delta;

			var pos = new Vector2(x: xPos, y: yPos);
			var scale = new Vector2(x: scaleVal, y: scaleVal);

			node.Position = pos;
			node.Scale = scale;
		}
	}

	float hpFlashTimer = 0.0f;
	void _on_health_bar_value_changed(float value)
	{
		hpFlashTimer = 0.0f;
	}
	float overHealFlashTimer = 0.0f;
		void _on_over_heal_bar_value_changed(float value)
	{
		overHealFlashTimer = 0.0f;
	}
	float xpFlashTimer = 0.0f;
	void _on_xp_bar_value_changed(float value)
	{
		xpFlashTimer = 0.0f;
	}

	float statUpLabelTimer = 3.0f;

	public void startClock(string powID, float time)
	{
		var stat = new stat_ui();

		switch (powID)
		{
			case "AgilityPack":
			{
				stat = agilityUI;
				break;
			}
			case "SpeedPack":
			{
				stat = speedUI;
				break;
			}
			case "RegenPack":
			{
				stat = regenUI;
				break;
			}
			case "FireSpeedPack":
			{
				stat = fireSpeedUI;
				break;
			}
			case "DamAccPack":
			{
				stat = damAccUI;
				break;
			}
		}

		if (main.curWaveType == main.waveType.boxWave)
		{
			stat.shouldStartClock = true;
		}
		else
		{
			stat.startClock(powTime);
		}
	}

	float powTime = 20;

	public void doStatUpAnimation(string statID, bool temp = false)
	{
		statUpLabel.Position = statUpLabelStartPos;
		var tempStr = "UP";
		if (temp)
		{
			tempStr = "BOOST";
			statUpLabel.SelfModulate = new Color(1, 1, 0, 1);
		}
		else
		{
			statUpLabel.SelfModulate = new Color(1, 1, 1, 1);
		}

		switch (statID)
		{
			case "AgilityPack":
			{
				statUpLabel.Text = "+ AGILITY " + tempStr + " +";
				agilityUI.Play();
				agilityUI.startSurge();
				break;
			}
			case "SpeedPack":
			{
				statUpLabel.Text = "+ SPEED " + tempStr + " +";
				speedUI.Play();
				speedUI.startSurge();
				break;
			}
			case "RegenPack":
			{
				statUpLabel.Text = "+ HP REGEN " + tempStr + " +";
				regenUI.Play();
				regenUI.startSurge();
				break;
			}
			case "FireSpeedPack":
			{
				statUpLabel.Text = "+ FIRE SPEED " + tempStr + " +";
				fireSpeedUI.Play();
				fireSpeedUI.startSurge();
				break;
			}
			case "DamAccPack":
			{
				statUpLabel.Text = "+ DAMAGE & ACCURACY " + tempStr + " +";
				damAccUI.Play();
				damAccUI.startSurge();
				break;
			}
		}
		statUpLabelTimer = 0.0f;
	}
}
