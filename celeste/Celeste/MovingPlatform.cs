using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class MovingPlatform : JumpThru
	{
		private Vector2 start;

		private Vector2 end;

		private float addY;

		private float sinkTimer;

		private MTexture[] textures;

		private string lastSfx;

		private SoundSource sfx;

		public MovingPlatform(Vector2 position, int width, Vector2 node)
			: base(position, width, safe: false)
		{
			start = Position;
			end = node;
			Add(sfx = new SoundSource());
			SurfaceSoundIndex = 5;
			lastSfx = ((Math.Sign(start.X - end.X) > 0 || Math.Sign(start.Y - end.Y) > 0) ? "event:/game/03_resort/platform_horiz_left" : "event:/game/03_resort/platform_horiz_right");
			Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 2f);
			tween.OnUpdate = delegate(Tween t)
			{
				MoveTo(Vector2.Lerp(start, end, t.Eased) + Vector2.UnitY * addY);
			};
			tween.OnStart = delegate
			{
				if (lastSfx == "event:/game/03_resort/platform_horiz_left")
				{
					sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_right");
				}
				else
				{
					sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_left");
				}
			};
			Add(tween);
			tween.Start(reverse: false);
			Add(new LightOcclude(0.2f));
		}

		public MovingPlatform(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Nodes[0] + offset)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Session session = SceneAs<Level>().Session;
			MTexture tex = ((session.Area.ID != 7 || !session.Level.StartsWith("e-")) ? GFX.Game["objects/woodPlatform/" + AreaData.Get(scene).WoodPlatform] : GFX.Game["objects/woodPlatform/" + AreaData.Get(4).WoodPlatform]);
			textures = new MTexture[tex.Width / 8];
			for (int i = 0; i < textures.Length; i++)
			{
				textures[i] = tex.GetSubtexture(i * 8, 0, 8, 8);
			}
			Vector2 halfSize = new Vector2(base.Width, base.Height + 4f) / 2f;
			scene.Add(new MovingPlatformLine(start + halfSize, end + halfSize));
		}

		public override void Render()
		{
			textures[0].Draw(Position);
			for (int i = 8; (float)i < base.Width - 8f; i += 8)
			{
				textures[1].Draw(Position + new Vector2(i, 0f));
			}
			textures[3].Draw(Position + new Vector2(base.Width - 8f, 0f));
			textures[2].Draw(Position + new Vector2(base.Width / 2f - 4f, 0f));
		}

		public override void OnStaticMoverTrigger(StaticMover sm)
		{
			sinkTimer = 0.4f;
		}

		public override void Update()
		{
			base.Update();
			if (HasPlayerRider())
			{
				sinkTimer = 0.2f;
				addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
			}
			else if (sinkTimer > 0f)
			{
				sinkTimer -= Engine.DeltaTime;
				addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
			}
			else
			{
				addY = Calc.Approach(addY, 0f, 20f * Engine.DeltaTime);
			}
		}
	}
}
