using System;
using Monocle;

namespace Celeste
{
	[Serializable]
	public struct Assists
	{
		public enum DashModes
		{
			Normal,
			Two,
			Infinite
		}

		public int GameSpeed;

		public bool Invincible;

		public DashModes DashMode;

		public bool DashAssist;

		public bool InfiniteStamina;

		public bool MirrorMode;

		public bool ThreeSixtyDashing;

		public bool InvisibleMotion;

		public bool NoGrabbing;

		public bool LowFriction;

		public bool SuperDashing;

		public bool Hiccups;

		public bool PlayAsBadeline;

		public static Assists Default
		{
			get
			{
				Assists result = default(Assists);
				result.GameSpeed = 10;
				return result;
			}
		}

		public void EnfornceAssistMode()
		{
			GameSpeed = Calc.Clamp(GameSpeed, 5, 10);
			MirrorMode = false;
			ThreeSixtyDashing = false;
			InvisibleMotion = false;
			NoGrabbing = false;
			LowFriction = false;
			SuperDashing = false;
			Hiccups = false;
			PlayAsBadeline = false;
		}
	}
}
