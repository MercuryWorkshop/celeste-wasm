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

    private static void MainLoop()
    {
        try
        {
            if (_firstRun)
            {
                Console.WriteLine("First run of the main loop");
                _firstRun = false;
                Console.WriteLine($"Assets Test: {File.ReadAllText("/test")}");

                // In original Main() but no longer used
                // game.RunWithLogging();
                // RunThread.WaitAll();
                // game.Dispose();
                // Audio.Unload();
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
}
