using Monocle;

namespace Celeste
{
	public static class Tags
	{
		public static BitTag PauseUpdate;

		public static BitTag FrozenUpdate;

		public static BitTag TransitionUpdate;

		public static BitTag HUD;

		public static BitTag Persistent;

		public static BitTag Global;

		public static void Initialize()
		{
			PauseUpdate = new BitTag("pauseUpdate");
			FrozenUpdate = new BitTag("frozenUpdate");
			TransitionUpdate = new BitTag("transitionUpdate");
			HUD = new BitTag("hud");
			Persistent = new BitTag("persistent");
			Global = new BitTag("global");
		}
	}
}
