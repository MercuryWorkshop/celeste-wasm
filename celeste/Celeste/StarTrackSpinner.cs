using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class StarTrackSpinner : TrackSpinner
	{
		public static ParticleType[] P_Trail;

		public Sprite Sprite;

		private bool hasStarted;

		private int colorID;

		private bool trail;

		public StarTrackSpinner(EntityData data, Vector2 offset)
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
			if (trail && base.Scene.OnInterval(0.03f))
			{
				SceneAs<Level>().ParticlesBG.Emit(P_Trail[colorID], 1, Position, Vector2.One * 3f);
			}
		}

		public override void OnTrackStart()
		{
			colorID++;
			colorID %= 3;
			Sprite.Play("spin" + colorID);
			if (hasStarted)
			{
				Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
			}
			hasStarted = true;
			trail = true;
		}

		public override void OnTrackEnd()
		{
			trail = false;
		}
	}
}
