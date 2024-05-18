using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BladeTrackSpinner : TrackSpinner
	{
		public static ParticleType P_Trail;

		public Sprite Sprite;

		private bool hasStarted;

		private bool trail;

		public BladeTrackSpinner(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			Add(Sprite = GFX.SpriteBank.Create("templeBlade"));
			Sprite.Play("idle");
			base.Depth = -50;
			Add(new MirrorReflection());
		}

		public override void Update()
		{
			base.Update();
			if (trail && base.Scene.OnInterval(0.04f))
			{
				SceneAs<Level>().ParticlesBG.Emit(P_Trail, 2, Position, Vector2.One * 3f);
			}
		}

		public override void OnTrackStart()
		{
			Sprite.Play("spin");
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
