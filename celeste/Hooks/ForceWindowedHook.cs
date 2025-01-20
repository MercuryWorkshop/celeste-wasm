using MonoMod.RuntimeDetour;
using System.Reflection;
using System;

public class ForceWindowedHook
{
    private Hook Hook;
    private FieldInfo Fullscreen;

    private void Hooker(Action<object> orig, object self)
    {
        Fullscreen.SetValue(self, false);
        Console.WriteLine("Forced windowed");
        orig(self);
    }

    public ForceWindowedHook(Assembly celeste)
    {
        var Settings = celeste.GetType("Celeste.Settings");
        Fullscreen = Settings.GetField("Fullscreen");

        Hook = new(Settings.GetMethod("ApplyScreen"), Hooker);
    }
}
