using System.Runtime.InteropServices.JavaScript;
using System;
using System.Reflection;

partial class JsSplash
{
    [JSImport("StartSplash", "JsSplash")]
    public static partial void JSStartSplash();
    [JSImport("OnMessage", "JsSplash")]
    public static partial void JSSendMessage(string message);
    [JSImport("EndSplash", "JsSplash")]
    public static partial void JSEndSplash();

	public static void StartSplash() {
		JSStartSplash();
	}

	public static void SendMessage(string message) {
		JSSendMessage(message);
	}

	public static void EndSplash() {
		JSEndSplash();
	}

    public static void Init(Assembly celeste)
    {
        var Splash = celeste.GetType("Celeste.Mod.EverestSplashHandler");
        if (Splash != null)
        {
            var start = Splash.GetField("start", BindingFlags.Public | BindingFlags.Static);
            var send = Splash.GetField("send", BindingFlags.Public | BindingFlags.Static);
            var stop = Splash.GetField("stop", BindingFlags.Public | BindingFlags.Static);

            start.SetValue(null, (Action)StartSplash);
            send.SetValue(null, (Action<string>)SendMessage);
            stop.SetValue(null, (Action)EndSplash);
        }
    }
}
