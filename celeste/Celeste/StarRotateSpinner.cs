using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class StarRotateSpinner : RotateSpinner
	{
		public Sprite Sprite;

		private int colorID;

		public StarRotateSpinner(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			Add(Sprite = GFX.SpriteBank.Create("moonBlade"));
			colorID = Calc.Random.Choose(0, 1, 2);
			Sprite.Play("idle" + colorID);
			base.Depth = -50;
			Add(new MirrorReflection());
		}

		public override void Update()
		{
			base.Update();
			if (Moving && base.Scene.OnInterval(0.03f))
			{
				SceneAs<Level>().ParticlesBG.Emit(StarTrackSpinner.P_Trail[colorID], 1, Position, Vector2.One * 3f);
			}
			if (base.Scene.OnInterval(0.8f))
			{
				colorID++;
				colorID %= 3;
				Sprite.Play("spin" + colorID);
			}
		}
	}
}
