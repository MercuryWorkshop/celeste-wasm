using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.InteropServices;
using Celeste;
using Steamworks;

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

    [JSExport]
    internal static Task PreInit()
    {
        Celeste.Celeste._mainThreadId = Thread.CurrentThread.ManagedThreadId;
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
