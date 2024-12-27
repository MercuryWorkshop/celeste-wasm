using System;
using System.Threading;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Celeste;
using Steamworks;

partial class Program
{
    static Game game;

    private static void Main()
    {
        Thread thread = new Thread(() =>
        {
            Console.WriteLine("calling mount_opfs");
            int ret = mount_opfs();
            Console.WriteLine($"called mount_opfs: {ret}");
        });
        thread.Start();
        thread.Join();
    }

    [DllImport("Emscripten")]
    public extern static int mount_opfs();

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
    internal static void MainLoop()
    {
        if (game == null)
        {
            try
            {
                Init();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error in Init()!");
                Console.Error.WriteLine(e);
                throw;
            }
        }

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
