using Godot;
using System;

public class powerTimer
{
    public float time = 0;
    public float maxTime = 20;
    public string powID;
    public bool shouldIncrement = false;
    public bool first = true;
    public bool done = false;

    public powerTimer(string ID)
    {
        powID = ID;
    }

    public void start()
    {
        time = 0;
        shouldIncrement = true;
        done = true;
    }
    public void stop()
    {
        shouldIncrement = false;
        first = true;
    }

    public static powerTimer agilityTemp = new powerTimer("AgilityPack");
	public static powerTimer speedTemp = new powerTimer("SpeedPack");
	public static powerTimer regenTemp = new powerTimer("RegenPack");
	public static powerTimer fireSpeedTemp = new powerTimer("FireSpeedPack");
	public static powerTimer damAccTemp = new powerTimer("DamAccPack");

    public static powerTimer[] powerTimers = new powerTimer[] { agilityTemp, speedTemp, regenTemp, fireSpeedTemp, damAccTemp };
}