using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Celeste;
using Steamworks;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("browser")]

partial class Program
{
    static Game game;

    private static void Main()
    {
        Console.WriteLine("Hi!");
    }

    [DllImport("Emscripten")]
    public extern static int mount_opfs();

	[JSExport]
    internal static void Init()
    {
        Celeste.Celeste._mainThreadId = Thread.CurrentThread.ManagedThreadId;
        Settings.Initialize();
        if (!Settings.Existed)
        {
            Settings.Instance.Language = SteamApps.GetCurrentGameLanguage();
        }
        _ = Settings.Existed;

        game = new Celeste.Celeste();
    }

    [JSExport]
    internal static Task PreInit()
    {
        return Task.Run(() =>
        {
            Console.WriteLine("calling mount_opfs");
            int ret = mount_opfs();
            Console.WriteLine($"called mount_opfs: {ret}");
            if (ret != 0)
            {
                throw new Exception("Failed to mount OPFS");
            }
        });
    }

    [JSExport]
    internal static void MainLoop()
    {
        try
        {
            game.RunOneFrame();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in MainLoop()!");
            Console.Error.WriteLine(e);
            throw;
        }
    }
}
