using MonoMod.RuntimeDetour;
using System;
using System.Runtime;

public class GCHook
{
    private Hook Hook;

    private void Hooker(GCLatencyMode latencyMode)
    {
        Console.WriteLine($"GCSettings LatencyMode: {latencyMode}");
    }

    public GCHook()
    {
        var getter = typeof(GCSettings).GetProperty("LatencyMode").GetSetMethod();
        Hook = new Hook(getter, Hooker);
    }
}
