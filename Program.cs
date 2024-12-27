using System;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.Xna.Framework;

Console.WriteLine("Hello, Browser!");

public class FNAGame : Game
{
    public FNAGame()
    {
        GraphicsDeviceManager gdm = new GraphicsDeviceManager(this);

        // Typically you would load a config here...
        gdm.PreferredBackBufferWidth = 512;
        gdm.PreferredBackBufferHeight = 512;
        gdm.IsFullScreen = false;
        gdm.SynchronizeWithVerticalRetrace = true;
    }

	byte r = 0;
	byte g = 0;
	byte b = 0;
	DateTime lastUpdate = DateTime.UnixEpoch;
	int updateCount = 0;

    protected override void Initialize()
    {
        /* This is a nice place to start up the engine, after
		 * loading configuration stuff in the constructor
		 */
        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Load textures, sounds, and so on in here...
        base.LoadContent();
    }

    protected override void UnloadContent()
    {
        // Clean up after yourself!
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        // Run game logic in here. Do NOT render anything here!
        base.Update(gameTime);
		updateCount++;
		DateTime now = DateTime.UtcNow;
		if ((now - lastUpdate).TotalSeconds > 1.0)
		{
			Console.WriteLine($"Main loop still running at: {now}; {Math.Round(updateCount / (now - lastUpdate).TotalSeconds, MidpointRounding.AwayFromZero)} UPS");
			lastUpdate = now;
			updateCount = 0;
		}
		if (r != 255) {
			r++;
			return;
		}
		if (g != 255) {
			g++;
			return;
		}
		if (b != 255) {
			b++;
			return;
		}
		r = 0;
		g = 0;
		b = 0;
    }

    protected override void Draw(GameTime gameTime)
    {
        // Render stuff in here. Do NOT run game logic in here!
        GraphicsDevice.Clear(new Color(r, g, b));
        base.Draw(gameTime);
    }
}

#nullable enable
partial class Main
{
	static Game? game;

	static void InitFmodTest() {
		FMOD.Studio.System system;
		FMOD.RESULT result = FMOD.Studio.System.create(out system);
		Console.WriteLine($"FMOD System: {result} isValid: {system.isValid()}");
	}

	[JSExport]
	internal static void MainLoop() {
		if (game == null) {
			InitFmodTest();
			game = new FNAGame();
		}

		game.RunOneFrame();
	}
}
#nullable disable
