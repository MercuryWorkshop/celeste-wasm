using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class MoonCreature : Entity
	{
		private struct TrailNode
		{
			public Vector2 Position;

			public Color Color;
		}

		private TrailNode[] trail;

		private Vector2 start;

		private Vector2 target;

		private float targetTimer;

		private Vector2 speed;

		private Vector2 bump;

		private Player following;

		private Vector2 followingOffset;

		private float followingTime;

		private Color OrbColor;

		private Color CenterColor;

		private Sprite Sprite;

		private const float Acceleration = 90f;

		private const float FollowAcceleration = 120f;

		private const float MaxSpeed = 40f;

		private const float MaxFollowSpeed = 70f;

		private const float MaxFollowDistance = 200f;

		private readonly int spawn;

		private Rectangle originLevelBounds;

		public MoonCreature(Vector2 position)
		{
			base.Tag = Tags.TransitionUpdate;
			base.Depth = -13010;
			base.Collider = new Hitbox(20f, 20f, -10f, -10f);
			start = position;
			targetTimer = 0f;
			GetRandomTarget();
			Position = target;
			Add(new PlayerCollider(OnPlayer));
			OrbColor = Calc.HexToColor("b0e6ff");
			CenterColor = Calc.Random.Choose(Calc.HexToColor("c34fc7"), Calc.HexToColor("4f95c7"), Calc.HexToColor("53c74f"));
			Color trailStartColor = Color.Lerp(CenterColor, Calc.HexToColor("bde4ee"), 0.5f);
			Color trailEndColor = Color.Lerp(CenterColor, Calc.HexToColor("2f2941"), 0.5f);
			trail = new TrailNode[10];
			for (int i = 0; i < 10; i++)
			{
				trail[i] = new TrailNode
				{
					Position = Position,
					Color = Color.Lerp(trailStartColor, trailEndColor, (float)i / 9f)
				};
			}
			Add(Sprite = GFX.SpriteBank.Create("moonCreatureTiny"));
		}

		public MoonCreature(EntityData data, Vector2 offset)
			: this(data.Position + offset)
		{
			spawn = data.Int("number", 1) - 1;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			for (int i = 0; i < spawn; i++)
			{
				scene.Add(new MoonCreature(Position + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4))));
			}
			originLevelBounds = (scene as Level).Bounds;
		}

		private void OnPlayer(Player player)
		{
			Vector2 spd = (Position - player.Center).SafeNormalize(player.Speed.Length() * 0.3f);
			if (spd.LengthSquared() > bump.LengthSquared())
			{
				bump = spd;
				if ((player.Center - start).Length() < 200f)
				{
					following = player;
					followingTime = Calc.Random.Range(6f, 12f);
					GetFollowOffset();
				}
			}
		}

		private void GetFollowOffset()
		{
			followingOffset = new Vector2(Calc.Random.Choose(-1, 1) * Calc.Random.Range(8, 16), Calc.Random.Range(-20f, 0f));
		}

		private void GetRandomTarget()
		{
			Vector2 lastTarget = target;
			do
			{
				float dist = Calc.Random.NextFloat(32f);
				float angle = Calc.Random.NextFloat((float)Math.PI * 2f);
				target = start + Calc.AngleToVector(angle, dist);
			}
			while ((lastTarget - target).Length() < 8f);
		}

		public override void Update()
		{
			base.Update();
			if (following == null)
			{
				targetTimer -= Engine.DeltaTime;
				if (targetTimer <= 0f)
				{
					targetTimer = Calc.Random.Range(0.8f, 4f);
					GetRandomTarget();
				}
			}
			else
			{
				followingTime -= Engine.DeltaTime;
				targetTimer -= Engine.DeltaTime;
				if (targetTimer <= 0f)
				{
					targetTimer = Calc.Random.Range(0.8f, 2f);
					GetFollowOffset();
				}
				target = following.Center + followingOffset;
				if ((Position - start).Length() > 200f || followingTime <= 0f)
				{
					following = null;
					targetTimer = 0f;
				}
			}
			Vector2 dir = (target - Position).SafeNormalize();
			speed += dir * ((following == null) ? 90f : 120f) * Engine.DeltaTime;
			speed = speed.SafeNormalize() * Math.Min(speed.Length(), (following == null) ? 40f : 70f);
			bump = bump.SafeNormalize() * Calc.Approach(bump.Length(), 0f, Engine.DeltaTime * 80f);
			Position += (speed + bump) * Engine.DeltaTime;
			Vector2 lastTrailPosition = Position;
			for (int i = 0; i < trail.Length; i++)
			{
				Vector2 normal = (trail[i].Position - lastTrailPosition).SafeNormalize();
				if (normal == Vector2.Zero)
				{
					normal = new Vector2(0f, 1f);
				}
				normal.Y += 0.05f;
				Vector2 trailTarget = lastTrailPosition + normal * 2f;
				trail[i].Position = Calc.Approach(trail[i].Position, trailTarget, 128f * Engine.DeltaTime);
				lastTrailPosition = trail[i].Position;
			}
			base.X = Calc.Clamp(base.X, originLevelBounds.Left + 4, originLevelBounds.Right - 4);
			base.Y = Calc.Clamp(base.Y, originLevelBounds.Top + 4, originLevelBounds.Bottom - 4);
		}

		public override void Render()
		{
			Vector2 was = Position;
			Position = Position.Floor();
			for (int i = trail.Length - 1; i >= 0; i--)
			{
				Vector2 pos = trail[i].Position;
				float size = Calc.ClampedMap(i, 0f, trail.Length - 1, 3f);
				Draw.Rect(pos.X - size / 2f, pos.Y - size / 2f, size, size, trail[i].Color);
			}
			base.Render();
			Position = was;
		}
	}
}
