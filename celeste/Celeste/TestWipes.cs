using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class TestWipes : Scene
	{
		private Coroutine coroutine;

		private Color lastColor = Color.White;

		public TestWipes()
		{
			coroutine = new Coroutine(routine());
		}

		private IEnumerator routine()
		{
			float dur = 1f;
			yield return 1f;
			while (true)
			{
				ScreenWipe.WipeColor = Color.Black;
				new CurtainWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
				lastColor = ScreenWipe.WipeColor;
				ScreenWipe.WipeColor = Calc.HexToColor("ff0034");
				new AngledWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
				lastColor = ScreenWipe.WipeColor;
				ScreenWipe.WipeColor = Calc.HexToColor("0b0960");
				new DreamWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
				lastColor = ScreenWipe.WipeColor;
				ScreenWipe.WipeColor = Calc.HexToColor("39bf00");
				new KeyDoorWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
				lastColor = ScreenWipe.WipeColor;
				ScreenWipe.WipeColor = Calc.HexToColor("4376b3");
				new WindWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
				lastColor = ScreenWipe.WipeColor;
				ScreenWipe.WipeColor = Calc.HexToColor("ffae00");
				new DropWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
				lastColor = ScreenWipe.WipeColor;
				ScreenWipe.WipeColor = Calc.HexToColor("cc54ff");
				new FallWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
				lastColor = ScreenWipe.WipeColor;
				ScreenWipe.WipeColor = Calc.HexToColor("ff007a");
				new MountainWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
				lastColor = ScreenWipe.WipeColor;
				ScreenWipe.WipeColor = Color.White;
				new HeartWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
				lastColor = ScreenWipe.WipeColor;
			}
		}

		public override void Update()
		{
			base.Update();
			coroutine.Update();
		}

		public override void Render()
		{
			Draw.SpriteBatch.Begin();
			Draw.Rect(-1f, -1f, 1920f, 1080f, lastColor);
			Draw.SpriteBatch.End();
			base.Render();
		}
	}
}
