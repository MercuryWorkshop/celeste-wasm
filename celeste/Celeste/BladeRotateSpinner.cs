using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BladeRotateSpinner : RotateSpinner
	{
		public Sprite Sprite;

		public BladeRotateSpinner(EntityData data, Vector2 offset)
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
			if (base.Scene.OnInterval(0.04f))
			{
				SceneAs<Level>().ParticlesBG.Emit(BladeTrackSpinner.P_Trail, 2, Position, Vector2.One * 3f);
			}
			if (base.Scene.OnInterval(1f))
			{
				Sprite.Play("spin");
			}
		}
	}
}
