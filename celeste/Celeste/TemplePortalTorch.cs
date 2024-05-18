using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class TemplePortalTorch : Entity
	{
		private Sprite sprite;

		private VertexLight light;

		private BloomPoint bloom;

		private SoundSource loopSfx;

		public TemplePortalTorch(Vector2 pos)
			: base(pos)
		{
			Add(sprite = new Sprite(GFX.Game, "objects/temple/portal/portaltorch"));
			sprite.AddLoop("idle", "", 0f, default(int));
			sprite.AddLoop("lit", "", 0.08f, 1, 2, 3, 4, 5, 6);
			sprite.Play("idle");
			sprite.Origin = new Vector2(32f, 64f);
			base.Depth = 8999;
		}

		public void Light(int count = 0)
		{
			sprite.Play("lit");
			Add(bloom = new BloomPoint(1f, 16f));
			Add(light = new VertexLight(Color.LightSeaGreen, 0f, 32, 128));
			Audio.Play((count == 0) ? "event:/game/05_mirror_temple/mainmirror_torch_lit_1" : "event:/game/05_mirror_temple/mainmirror_torch_lit_2", Position);
			Add(loopSfx = new SoundSource());
			loopSfx.Play("event:/game/05_mirror_temple/mainmirror_torch_loop");
		}

		public override void Update()
		{
			base.Update();
			if (bloom != null && bloom.Alpha > 0.5f)
			{
				bloom.Alpha -= Engine.DeltaTime;
			}
			if (light != null && light.Alpha < 1f)
			{
				light.Alpha = Calc.Approach(light.Alpha, 1f, Engine.DeltaTime);
			}
		}
	}
}
