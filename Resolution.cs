using Godot;
using System;

public class Resolution
{
    public Vector2 res;
    public Vector2I resI;

    public string shortName;
    public string longName;
    public Resolution(int x, int y)
    {
        res = new Vector2(x: x, y: y);
        resI = new Vector2I(x, y);

        shortName = y + "p";
        longName = x + "x" + y;
    }

    public static Resolution r360p = new Resolution(640, 360);
    public static Resolution r540p = new Resolution(960, 540);
    public static Resolution r720p = new Resolution(1280, 720);
    public static Resolution r900p = new Resolution(1600, 900);
    public static Resolution r1080p = new Resolution(1920, 1080);
    public static Resolution r1440p = new Resolution(2560, 1440);

	public static Resolution[] resolutions = new Resolution[6]{ r360p, r540p, r720p, r900p, r1080p, r1440p };
}