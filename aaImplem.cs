using Godot;
using System;

public class aaImplem
{
    public string name;

    public aaImplem(string n)
    {
        name = n;
    }

    public static aaImplem disabled = new aaImplem("Disabled");
	public static aaImplem msaa2x = new aaImplem("MSAA 2X");
	public static aaImplem msaa4x = new aaImplem("MSAA 4X");
	public static aaImplem msaa8x = new aaImplem("MSAA 8X");
	public static aaImplem fxaa = new aaImplem("FXAA");
	public static aaImplem taa = new aaImplem("TAA");

    public static aaImplem[] aaImplems = new aaImplem[6] { disabled, msaa2x, msaa4x, msaa8x, fxaa, taa };
}