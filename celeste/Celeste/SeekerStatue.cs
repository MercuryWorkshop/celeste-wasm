using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class SeekerStatue : Entity
	{
		private enum Hatch
		{
			Distance,
			PlayerRightOfX
		}

		private Hatch hatch;

		private Sprite sprite;

		public SeekerStatue(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			SeekerStatue seekerStatue = this;
			base.Depth = 8999;
			Add(sprite = GFX.SpriteBank.Create("seeker"));
			sprite.Play("statue");
			sprite.OnLastFrame = delegate(string f)
			{
				if (f == "hatch")
				{
					Seeker entity = new Seeker(data, offset)
					{
						Light = 
						{
							Alpha = 0f
						}
					};
					seekerStatue.Scene.Add(entity);
					seekerStatue.RemoveSelf();
				}
			};
			hatch = data.Enum("hatch", Hatch.Distance);
		}

		public override void Update()
		{
			base.Update();
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null && sprite.CurrentAnimationID == "statue")
			{
				bool ready = false;
				if (hatch == Hatch.Distance && (player.Position - Position).Length() < 220f)
				{
					ready = true;
				}
				else if (hatch == Hatch.PlayerRightOfX && player.X > base.X + 32f)
				{
					ready = true;
				}
				if (ready)
				{
					BreakOutParticles();
					sprite.Play("hatch");
					Audio.Play("event:/game/05_mirror_temple/seeker_statue_break", Position);
					Alarm.Set(this, 0.8f, BreakOutParticles);
				}
			}
		}

		private void BreakOutParticles()
		{
			Level level = SceneAs<Level>();
			for (float i = 0f; i < (float)Math.PI * 2f; i += 0.17453292f)
			{
				Vector2 at = base.Center + Calc.AngleToVector(i + Calc.Random.Range(-(float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(12, 20));
				level.Particles.Emit(Seeker.P_BreakOut, at, i);
			}
		}
	}
}
