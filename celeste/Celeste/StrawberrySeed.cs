using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class StrawberrySeed : Entity
	{
		public static ParticleType P_Burst;

		private const float LoseDelay = 0.25f;

		private const float LoseGraceTime = 0.15f;

		public Strawberry Strawberry;

		private Sprite sprite;

		private Follower follower;

		private Wiggler wiggler;

		private Platform attached;

		private SineWave sine;

		private Tween lightTween;

		private VertexLight light;

		private BloomPoint bloom;

		private Shaker shaker;

		private int index;

		private Vector2 start;

		private Player player;

		private Level level;

		private float canLoseTimer;

		private float loseTimer;

		private bool finished;

		private bool losing;

		private bool ghost;

		public bool Collected
		{
			get
			{
				if (!follower.HasLeader)
				{
					return finished;
				}
				return true;
			}
		}

		public StrawberrySeed(Strawberry strawberry, Vector2 position, int index, bool ghost)
			: base(position)
		{
			Strawberry = strawberry;
			base.Depth = -100;
			start = Position;
			base.Collider = new Hitbox(12f, 12f, -6f, -6f);
			this.index = index;
			this.ghost = ghost;
			Add(follower = new Follower(OnGainLeader, OnLoseLeader));
			follower.FollowDelay = 0.2f;
			follower.PersistentFollow = false;
			Add(new StaticMover
			{
				SolidChecker = (Solid s) => s.CollideCheck(this),
				OnAttach = delegate(Platform p)
				{
					base.Depth = -1000000;
					base.Collider = new Hitbox(24f, 24f, -12f, -12f);
					attached = p;
					start = Position - p.Position;
				}
			});
			Add(new PlayerCollider(OnPlayer));
			Add(wiggler = Wiggler.Create(0.5f, 4f, delegate(float v)
			{
				sprite.Scale = Vector2.One * (1f + 0.2f * v);
			}));
			Add(sine = new SineWave(0.5f).Randomize());
			Add(shaker = new Shaker(on: false));
			Add(bloom = new BloomPoint(1f, 12f));
			Add(light = new VertexLight(Color.White, 1f, 16, 24));
			Add(lightTween = light.CreatePulseTween());
		}

		public override void Awake(Scene scene)
		{
			level = scene as Level;
			base.Awake(scene);
			sprite = GFX.SpriteBank.Create(ghost ? "ghostberrySeed" : ((level.Session.Area.Mode == AreaMode.CSide) ? "goldberrySeed" : "strawberrySeed"));
			sprite.Position = new Vector2(sine.Value * 2f, sine.ValueOverTwo * 1f);
			Add(sprite);
			if (ghost)
			{
				sprite.Color = Color.White * 0.8f;
			}
			int total = base.Scene.Tracker.CountEntities<StrawberrySeed>();
			float offset = 1f - (float)index / ((float)total + 1f);
			offset = 0.25f + offset * 0.75f;
			sprite.PlayOffset("idle", offset);
			sprite.OnFrameChange = delegate
			{
				if (Visible && sprite.CurrentAnimationID == "idle" && sprite.CurrentAnimationFrame == 19)
				{
					Audio.Play("event:/game/general/seed_pulse", Position, "count", index);
					lightTween.Start();
					level.Displacement.AddBurst(Position, 0.6f, 8f, 20f, 0.2f);
				}
			};
			P_Burst.Color = sprite.Color;
		}

		public override void Update()
		{
			base.Update();
			if (!finished)
			{
				if (canLoseTimer > 0f)
				{
					canLoseTimer -= Engine.DeltaTime;
				}
				else if (follower.HasLeader && player.LoseShards)
				{
					losing = true;
				}
				if (losing)
				{
					if (loseTimer <= 0f || player.Speed.Y < 0f)
					{
						player.Leader.LoseFollower(follower);
						losing = false;
					}
					else if (player.LoseShards)
					{
						loseTimer -= Engine.DeltaTime;
					}
					else
					{
						loseTimer = 0.15f;
						losing = false;
					}
				}
				sprite.Position = new Vector2(sine.Value * 2f, sine.ValueOverTwo * 1f) + shaker.Value;
			}
			else
			{
				light.Alpha = Calc.Approach(light.Alpha, 0f, Engine.DeltaTime * 4f);
			}
		}

		private void OnPlayer(Player player)
		{
			Audio.Play("event:/game/general/seed_touch", Position, "count", index);
			this.player = player;
			player.Leader.GainFollower(follower);
			Collidable = false;
			base.Depth = -1000000;
			bool allCollected = true;
			foreach (StrawberrySeed seed in Strawberry.Seeds)
			{
				if (!seed.follower.HasLeader)
				{
					allCollected = false;
				}
			}
			if (allCollected)
			{
				base.Scene.Add(new CSGEN_StrawberrySeeds(Strawberry));
			}
		}

		private void OnGainLeader()
		{
			wiggler.Start();
			canLoseTimer = 0.25f;
			loseTimer = 0.15f;
		}

		private void OnLoseLeader()
		{
			if (!finished)
			{
				Add(new Coroutine(ReturnRoutine()));
			}
		}

		private IEnumerator ReturnRoutine()
		{
			Audio.Play("event:/game/general/seed_poof", Position);
			Collidable = false;
			sprite.Scale = Vector2.One * 2f;
			yield return 0.05f;
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			for (int i = 0; i < 6; i++)
			{
				float dir = Calc.Random.NextFloat((float)Math.PI * 2f);
				level.ParticlesFG.Emit(P_Burst, 1, Position + Calc.AngleToVector(dir, 4f), Vector2.Zero, dir);
			}
			Visible = false;
			yield return 0.3f + (float)index * 0.1f;
			Audio.Play("event:/game/general/seed_reappear", Position, "count", index);
			Position = start;
			if (attached != null)
			{
				Position += attached.Position;
			}
			shaker.ShakeFor(0.4f, removeOnFinish: false);
			sprite.Scale = Vector2.One;
			Visible = true;
			Collidable = true;
			level.Displacement.AddBurst(Position, 0.2f, 8f, 28f, 0.2f);
		}

		public void OnAllCollected()
		{
			finished = true;
			follower.Leader.LoseFollower(follower);
			base.Depth = -2000002;
			base.Tag = Tags.FrozenUpdate;
			wiggler.Start();
		}

		public void StartSpinAnimation(Vector2 averagePos, Vector2 centerPos, float angleOffset, float time)
		{
			float spinLerp = 0f;
			Vector2 start = Position;
			sprite.Play("noFlash");
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, time / 2f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				spinLerp = t.Eased;
			};
			Add(tween);
			tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, time, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				float angleRadians = (float)Math.PI / 2f + angleOffset - MathHelper.Lerp(0f, 32.201324f, t.Eased);
				Vector2 value = Vector2.Lerp(averagePos, centerPos, spinLerp) + Calc.AngleToVector(angleRadians, 25f);
				Position = Vector2.Lerp(start, value, spinLerp);
			};
			Add(tween);
		}

		public void StartCombineAnimation(Vector2 centerPos, float time, ParticleSystem particleSystem)
		{
			Vector2 from = Position;
			float startAngle = Calc.Angle(centerPos, from);
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BigBackIn, time, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				float angleRadians = MathHelper.Lerp(startAngle, startAngle - (float)Math.PI * 2f, Ease.CubeIn(t.Percent));
				float length = MathHelper.Lerp(25f, 0f, t.Eased);
				Position = centerPos + Calc.AngleToVector(angleRadians, length);
			};
			tween.OnComplete = delegate
			{
				Visible = false;
				for (int i = 0; i < 6; i++)
				{
					float num = Calc.Random.NextFloat((float)Math.PI * 2f);
					particleSystem.Emit(P_Burst, 1, Position + Calc.AngleToVector(num, 4f), Vector2.Zero, num);
				}
				RemoveSelf();
			};
			Add(tween);
		}
	}
}
