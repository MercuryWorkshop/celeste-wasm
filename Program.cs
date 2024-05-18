using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.IO;
using Celeste;

public static partial class Program
{
    internal static void Main()
    {
        Console.WriteLine("Setting up main loop");
        Celeste.Celeste._mainThreadId = Thread.CurrentThread.ManagedThreadId;
        Settings.Initialize();
        _ = Settings.Existed;

        game = new Celeste.Celeste();

        SetMainLoop(MainLoop);
    }

    private static bool _firstRun = true;
    private static DateTime _lastLog = DateTime.UnixEpoch;
    private static Celeste.Celeste game;
    public static bool exitGame = false;

    public static void SyncFS() {
        Sync(SyncCallback);
    }

    private static void SyncCallback() {
        Console.WriteLine("Synced!");
    }

    private static void DoNothing() {

    }

    private static void MainLoop()
    {
        if (exitGame) {
            // RunThread.WaitAll();
            SyncFS();
            game.Dispose();
            Audio.Unload();
            SetMainLoop(DoNothing);
        }
        try
        {
            if (_firstRun)
            {
                Console.WriteLine("First run of the main loop");
                _firstRun = false;
                Console.WriteLine($"Assets Test: {File.ReadAllText("/test")}");

                // In original Main() but no longer used
                // game.RunWithLogging();
            }

            var now = DateTime.UtcNow;
            if ((now - _lastLog).TotalSeconds > 1.0)
            {
                _lastLog = now;
                Console.WriteLine($"Main loop still running at: {now}");
            }

            if (game != null)
            {
                game.RunOneFrame();
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }

    [JSImport("setMainLoop", "main.js")]
    internal static partial void SetMainLoop([JSMarshalAs<JSType.Function>] Action cb);

    [JSImport("syncFs", "main.js")]
    internal static partial void Sync([JSMarshalAs<JSType.Function>] Action cb);
}
