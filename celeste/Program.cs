using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.InteropServices;
using System.Reflection;
using Steamworks;
using MonoMod.RuntimeDetour;
using Microsoft.Xna.Framework;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("browser")]

partial class Program
{
    private static void Main()
    {
        Console.WriteLine("Hi!");
    }

    [DllImport("Emscripten")]
    public extern static int mount_opfs();

    static Game celeste;
    public static bool firstLaunch = true;

	static BloomHooker BloomHook;

    [JSExport]
    internal static Task PreInit()
    {
        typeof(Celeste.Celeste).GetField("_mainThreadId", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, Thread.CurrentThread.ManagedThreadId);
        return Task.Run(() =>
        {
            try
            {
				BloomHook = new(Assembly.GetExecutingAssembly());
                Console.WriteLine("calling mount_opfs");
                int ret = mount_opfs();
                Console.WriteLine($"called mount_opfs: {ret}");
                if (ret != 0)
                {
                    throw new Exception("Failed to mount OPFS");
                }

                Console.WriteLine("initializing settings");
                Celeste.Settings.Initialize();
                if (!Celeste.Settings.Existed)
                {
                    Celeste.Settings.Instance.Language = SteamApps.GetCurrentGameLanguage();
                }
                _ = Celeste.Settings.Existed;
                Console.WriteLine("initialized settings");

				typeof(Celeste.Celeste).GetField("IsGGP", BindingFlags.Static | BindingFlags.Public).SetValue(null, true);
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("Error in PreInit()!");
                Console.Error.WriteLine(error);
                throw error;
            }
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
        Celeste.RunThread.WaitAll();
        celeste.Dispose();
        Celeste.Audio.Unload();
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
