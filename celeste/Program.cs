using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.InteropServices;
using Celeste;
using Steamworks;
using MonoMod.RuntimeDetour;
using FMOD.Studio;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("browser")]

partial class Program
{
    private static void Main()
    {
        Console.WriteLine("Hi!");
    }

    [DllImport("Emscripten")]
    public extern static int mount_opfs();

    static Celeste.Celeste celeste;
    public static bool firstLaunch = true;

	internal static Bank LoadHook(Func<string, bool, Bank> orig, string name, bool loadStrings) {
		Console.WriteLine("Hook test!!!");
		return orig(name, loadStrings);
	}

	static Hook hook;

    [JSExport]
    internal static Task PreInit()
    {
        Celeste.Celeste._mainThreadId = Thread.CurrentThread.ManagedThreadId;
		try {
			hook = new Hook(typeof(Celeste.Audio.Banks).GetMethod("Load"), LoadHook);
		} catch(Exception err) {
			Console.Error.WriteLine("Failed to create hook");
			Console.Error.WriteLine(err);
		}
        return Task.Run(() =>
        {
            Console.WriteLine("calling mount_opfs");
            int ret = mount_opfs();
            Console.WriteLine($"called mount_opfs: {ret}");
            if (ret != 0)
            {
                throw new Exception("Failed to mount OPFS");
            }

            Console.WriteLine("initializing settings");
            Settings.Initialize();
            if (!Settings.Existed)
            {
                Settings.Instance.Language = SteamApps.GetCurrentGameLanguage();
            }
            _ = Settings.Existed;
            Console.WriteLine("initialized settings");
        });
    }

    [JSExport]
    internal static void Init()
    {
        celeste = new Celeste.Celeste();
    }

    [JSExport]
    internal static void Cleanup()
    {
        firstLaunch = false;
        RunThread.WaitAll();
        celeste.Dispose();
        Audio.Unload();
    }

    [JSExport]
    internal static bool MainLoop()
    {
        try
        {
            celeste.RunOneFrame();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in MainLoop()!");
            Console.Error.WriteLine(e);
            throw;
        }
        return celeste.RunApplication;
    }
}
