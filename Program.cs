using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using Celeste;

public static partial class Program
{


    internal static void Main()
    {
        Console.WriteLine("Main()");

        Celeste.Celeste._mainThreadId = Thread.CurrentThread.ManagedThreadId;
        Settings.Initialize();
        _ = Settings.Existed;
    }


    [JSExport()]
    public static void StartGame()
    {
        game = new Celeste.Celeste();

        SetMainLoop(MainLoop);
    }

    private static Celeste.Celeste game;
    public static bool exitGame = false;
    public static bool exited = false;

    public static void SyncFS()
    {
        Sync(SyncCallback);
    }

    private static void SyncCallback()
    {
        Console.WriteLine("Synced!");
    }

    private static void MainLoop()
    {
        if (exited) return;
        if (exitGame)
        {
            // RunThread.WaitAll();
            SyncFS();
            Audio.Unload();
            exitGame = false;
            exited = true;
            SetMainLoop(null);
        }
        try
        {
            game.RunOneFrame();
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
