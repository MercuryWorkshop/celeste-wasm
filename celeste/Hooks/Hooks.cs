using System.Reflection;

public class Hooks
{
    public static GCHook Gc;
    public static BloomHook Bloom;
    public static ForceWindowedHook Windowed;
    public static LoadLaterHook LoadLater;
    public static CreditsHook Credits;

    public static void Initialize(Assembly celeste)
    {
        Gc = new();
        Bloom = new(celeste);
        Windowed = new(celeste);
        LoadLater = new(celeste);
        Credits = new(celeste);
    }
}
