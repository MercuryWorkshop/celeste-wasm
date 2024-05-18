using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BirdPath : Entity
	{
		private Vector2 start;

		private Sprite sprite;

		private Vector2[] nodes;

		private Color trailColor = Calc.HexToColor("639bff");

		private Vector2 target;

		private Vector2 speed;

		private float maxspeed;

		private Vector2 lastTrail;

		private float speedMult;

		private EntityID ID;

		private bool onlyOnce;

		private bool onlyIfLeft;

		public BirdPath(EntityID id, EntityData data, Vector2 offset)
			: this(id, data.Position + offset, data.NodesOffset(offset), data.Bool("only_once"), data.Bool("onlyIfLeft"), data.Float("speedMult", 1f))
		{
		}

		public BirdPath(EntityID id, Vector2 position, Vector2[] nodes, bool onlyOnce, bool onlyIfLeft, float speedMult)
		{
			base.Tag = Tags.TransitionUpdate;
			ID = id;
			Position = position;
			start = position;
			this.nodes = nodes;
			this.onlyOnce = onlyOnce;
			this.onlyIfLeft = onlyIfLeft;
			this.speedMult = speedMult;
			maxspeed = 150f * speedMult;
			Add(sprite = GFX.SpriteBank.Create("bird"));
			sprite.Play("flyupRoll");
			sprite.JustifyOrigin(0.5f, 0.75f);
			Add(new SoundSource("event:/new_content/game/10_farewell/bird_flyuproll")
			{
				RemoveOnOneshotEnd = true
			});
			Add(new Coroutine(Routine()));
		}

		public void WaitForTrigger()
		{
			Visible = (Active = false);
		}

		public void Trigger()
		{
			Visible = (Active = true);
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (onlyOnce)
			{
				(base.Scene as Level).Session.DoNotLoad.Add(ID);
			}
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (onlyIfLeft)
			{
				Player p = base.Scene.Tracker.GetEntity<Player>();
				if (p != null && p.X > base.X)
				{
					RemoveSelf();
				}
			}
		}

		private IEnumerator Routine()
		{
			Vector2 last = start;
			for (int i = 0; i <= nodes.Length - 1; i += 2)
			{
				Vector2 anchor = nodes[i];
				Vector2 next = nodes[i + 1];
				SimpleCurve curve = new SimpleCurve(last, next, anchor);
				float dist = curve.GetLengthParametric(32);
				float duration = dist / maxspeed;
				bool playedSfx = false;
				_ = Position;
				for (float p = 0f; p < 1f; p += Engine.DeltaTime * speedMult / duration)
				{
					target = curve.GetPoint(p);
					if (p > 0.9f)
					{
						if (!playedSfx && sprite.CurrentAnimationID != "flyupRoll")
						{
							SoundSource sfx = new SoundSource("event:/new_content/game/10_farewell/bird_flyuproll");
							sfx.RemoveOnOneshotEnd = true;
							Add(sfx);
							playedSfx = true;
						}
						sprite.Play("flyupRoll");
					}
					yield return null;
				}
				last = next;
			}
			RemoveSelf();
		}

		public override void Update()
		{
			if ((base.Scene as Level).Transitioning)
			{
				using (IEnumerator<Component> enumerator = GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current is SoundSource sfx)
						{
							sfx.UpdateSfxPosition();
						}
					}
					return;
				}
			}
			base.Update();
			int num = Math.Sign(base.X - target.X);
			Vector2 direction = (target - Position).SafeNormalize();
			float accel = 800f;
			speed += direction * accel * Engine.DeltaTime;
			if (speed.Length() > maxspeed)
			{
				speed = speed.SafeNormalize(maxspeed);
			}
			Position += speed * Engine.DeltaTime;
			if (num != Math.Sign(base.X - target.X))
			{
				speed.X *= 0.75f;
			}
			float startAngle = speed.Angle();
			float targetRotation = Calc.Angle(Position, target);
			float visibleRotation = Calc.AngleLerp(startAngle, targetRotation, 0.5f);
			sprite.Rotation = (float)Math.PI / 2f + visibleRotation;
			if ((lastTrail - Position).Length() > 32f)
			{
				TrailManager.Add(this, trailColor);
				lastTrail = Position;
			}
		}

		public override void Render()
		{
			base.Render();
		}

		public override void DebugRender(Camera camera)
		{
			Vector2 last = start;
			for (int i = 0; i < nodes.Length - 1; i += 2)
			{
				Vector2 next = nodes[i + 1];
				new SimpleCurve(last, next, nodes[i]).Render(Color.Red * 0.25f, 32);
				last = next;
			}
			Draw.Line(Position, Position + (target - Position).SafeNormalize() * ((target - Position).Length() - 3f), Color.Yellow);
			Draw.Circle(target, 3f, Color.Yellow, 16);
		}
	}
}
