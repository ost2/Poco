using Godot;
using System;

public partial class menu : CanvasLayer
{
	public main main;
	public hud hud;

	public Vector2 baseOffset;
	public Vector2 baseScale;

	public string vSync = "Disabled";

	int aaIndex;
	public aaImplem curAA = aaImplem.disabled;

	int resIndex = 4;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		baseOffset = Offset;
		baseScale = Scale;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void setOffset()
	{
		Offset = baseOffset * getScaleFactor();
		Scale = baseScale * getScaleFactor();
	}

	public float getScaleFactor()
	{
		var screenSize = GetViewport().GetVisibleRect().Size;
		var scaleFactor = (Mathf.InverseLerp(0, 1920, screenSize.X) + Mathf.InverseLerp(0, 1080, screenSize.Y)) / 2;

		return scaleFactor;
	}

	void changeResolution()
	{
		if (resIndex < 5)
		{
			resIndex += 1;
		}
		else 
		{
			resIndex = 0;
		}

		main.curResolution = Resolution.resolutions[resIndex];

		DisplayServer.WindowSetSize(main.curResolution.resI);
		
		// ProjectSettings.SetSetting("display/window/size/viewport_width", main.curResolution.res.X);
		// ProjectSettings.SetSetting("display/window/size/viewport_heigth", main.curResolution.res.Y);
	}

	void toggleVsync()
	{
		switch (vSync)
		{
			case "Disabled":
			{
				vSync = "Enabled";
				DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
				break;
			}
			case "Enabled":
			{
				vSync = "Adaptive";
				DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Adaptive);
				break;
			}
			case "Adaptive":
			default:
			{
				vSync = "Disabled";
				DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
				break;
			}
		}
	}

	void changeAA()
	{
		if (aaIndex < 5)
		{
			aaIndex += 1;
		}
		else
		{
			aaIndex = 0;
		}

		curAA = aaImplem.aaImplems[aaIndex];

		switch (curAA.name)
		{
			case "Disabled":
			{
				RenderingServer.ViewportSetMsaa2D(GetViewport().GetViewportRid(), RenderingServer.ViewportMsaa.Disabled);
				RenderingServer.ViewportSetScreenSpaceAA(GetViewport().GetViewportRid(), RenderingServer.ViewportScreenSpaceAA.Disabled);
				RenderingServer.ViewportSetUseTaa(GetViewport().GetViewportRid(), false);
				break;
			}
			case "FXAA":
			{
				RenderingServer.ViewportSetMsaa2D(GetViewport().GetViewportRid(), RenderingServer.ViewportMsaa.Disabled);
				RenderingServer.ViewportSetScreenSpaceAA(GetViewport().GetViewportRid(), RenderingServer.ViewportScreenSpaceAA.Fxaa);
				RenderingServer.ViewportSetUseTaa(GetViewport().GetViewportRid(), false);
				break;
			}
			case "MSAA 2X":
			{
				RenderingServer.ViewportSetMsaa2D(GetViewport().GetViewportRid(), RenderingServer.ViewportMsaa.Msaa2X);
				RenderingServer.ViewportSetScreenSpaceAA(GetViewport().GetViewportRid(), RenderingServer.ViewportScreenSpaceAA.Disabled);
				RenderingServer.ViewportSetUseTaa(GetViewport().GetViewportRid(), false);
				break;
			}
			case "MSAA 4X":
			{
				RenderingServer.ViewportSetMsaa2D(GetViewport().GetViewportRid(), RenderingServer.ViewportMsaa.Msaa4X);
				RenderingServer.ViewportSetScreenSpaceAA(GetViewport().GetViewportRid(), RenderingServer.ViewportScreenSpaceAA.Disabled);
				RenderingServer.ViewportSetUseTaa(GetViewport().GetViewportRid(), false);
				break;
			}
			case "MSAA 8X":
			{
				RenderingServer.ViewportSetMsaa2D(GetViewport().GetViewportRid(), RenderingServer.ViewportMsaa.Disabled);
				RenderingServer.ViewportSetScreenSpaceAA(GetViewport().GetViewportRid(), RenderingServer.ViewportScreenSpaceAA.Disabled);
				RenderingServer.ViewportSetUseTaa(GetViewport().GetViewportRid(), false);
				break;
			}
			case "TAA":
			{
				RenderingServer.ViewportSetMsaa2D(GetViewport().GetViewportRid() , RenderingServer.ViewportMsaa.Disabled);
				RenderingServer.ViewportSetScreenSpaceAA(GetViewport().GetViewportRid(), RenderingServer.ViewportScreenSpaceAA.Disabled);
				RenderingServer.ViewportSetUseTaa(GetViewport().GetViewportRid(), true);
				break;
			}
		}
	}

	public void doButtonAction(string buttonID)
	{
		switch (buttonID)
		{
			case "MainQuit":
			{
				GetTree().Quit();
				break;
			}
			case "MainStart":
			{
				main.startGame();
				break;
			}
			case "MainOptions":
			case "PauseOptions":
			{
				main.showOptions();
				break;
			}
			case "PauseContinue":
			{
				unPause();
				break;
			}
			case "PauseExit":
			{
				unPause();
				main.toggleMainMenu();

				break;
			}
			case "DeathRestart":
			{
				hud.showDeathScreen = false;
				main.resetGame();
				break;
			}
			case "OptionsDifficulty":
			{
				switchDifficulty();
				break;
			}
			case "OptionsResolution":
			{
				changeResolution();
				break;
			}
			case "OptionsVsync":
			{
				toggleVsync();
				break;
			}
			case "OptionsAA":
			{
				changeAA();
				break;
			}
			case "OptionsBack":
			{
				main.hideOptions();
				break;
			}
		}
	}
	public void pause()
	{
		main.curMenu = "PauseMenu";

		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;
		Show();
	}
	public void unPause()
	{	
		Input.MouseMode = Input.MouseModeEnum.Hidden;
		GetTree().Paused = false;

		main.hud.Show();													

		main.optionsMenu.Hide();
		Hide();
	}

	public void togglePause()
	{
		switch (GetTree().Paused)
		{
			case true:
			{
				unPause();
				break;
			}
			case false:
			{
				pause();
				break;
			}
		}
	}

	public void switchDifficulty()
	{
		switch (main.curDifficulty)
		{
			case main.difficulty.easy:
			{
				main.curDifficulty = main.difficulty.medium;
				break;
			}
			case main.difficulty.medium:
			{
				main.curDifficulty = main.difficulty.hard;
				break;
			}
			case main.difficulty.hard:
			{
				main.curDifficulty = main.difficulty.easy;
				break;
			}
		}
	}
}
