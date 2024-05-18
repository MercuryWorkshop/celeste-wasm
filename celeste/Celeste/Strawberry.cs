using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Strawberry : Entity
	{
		public static ParticleType P_Glow;

		public static ParticleType P_GhostGlow;

		public static ParticleType P_GoldGlow;

		public static ParticleType P_MoonGlow;

		public static ParticleType P_WingsBurst;

		public EntityID ID;

		private Sprite sprite;

		public Follower Follower;

		private Wiggler wiggler;

		private Wiggler rotateWiggler;

		private BloomPoint bloom;

		private VertexLight light;

		private Tween lightTween;

		private float wobble;

		private Vector2 start;

		private float collectTimer;

		private bool collected;

		private bool isGhostBerry;

		private bool flyingAway;

		private float flapSpeed;

		public bool ReturnHomeWhenLost = true;

		public bool WaitingOnSeeds;

		public List<StrawberrySeed> Seeds;

		public bool Winged { get; private set; }

		public bool Golden { get; private set; }

		public bool Moon { get; private set; }

		private string gotSeedFlag => "collected_seeds_of_" + ID.ToString();

		private bool IsFirstStrawberry
		{
			get
			{
				for (int i = Follower.FollowIndex - 1; i >= 0; i--)
				{
					if (Follower.Leader.Followers[i].Entity is Strawberry current && !current.Golden)
					{
						return false;
					}
				}
				return true;
			}
		}

		public Strawberry(EntityData data, Vector2 offset, EntityID gid)
		{
			ID = gid;
			Position = (start = data.Position + offset);
			Winged = data.Bool("winged") || data.Name == "memorialTextController";
			Golden = data.Name == "memorialTextController" || data.Name == "goldenBerry";
			Moon = data.Bool("moon");
			isGhostBerry = SaveData.Instance.CheckStrawberry(ID);
			base.Depth = -100;
			base.Collider = new Hitbox(14f, 14f, -7f, -7f);
			Add(new PlayerCollider(OnPlayer));
			Add(new MirrorReflection());
			Add(Follower = new Follower(ID, null, OnLoseLeader));
			Follower.FollowDelay = 0.3f;
			if (Winged)
			{
				Add(new DashListener
				{
					OnDash = OnDash
				});
			}
			if (data.Nodes != null && data.Nodes.Length != 0)
			{
				Seeds = new List<StrawberrySeed>();
				for (int i = 0; i < data.Nodes.Length; i++)
				{
					Seeds.Add(new StrawberrySeed(this, offset + data.Nodes[i], i, isGhostBerry));
				}
			}
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (SaveData.Instance.CheckStrawberry(ID))
			{
				if (Moon)
				{
					sprite = GFX.SpriteBank.Create("moonghostberry");
				}
				else if (Golden)
				{
					sprite = GFX.SpriteBank.Create("goldghostberry");
				}
				else
				{
					sprite = GFX.SpriteBank.Create("ghostberry");
				}
				sprite.Color = Color.White * 0.8f;
			}
			else if (Moon)
			{
				sprite = GFX.SpriteBank.Create("moonberry");
			}
			else if (Golden)
			{
				sprite = GFX.SpriteBank.Create("goldberry");
			}
			else
			{
				sprite = GFX.SpriteBank.Create("strawberry");
			}
			Add(sprite);
			if (Winged)
			{
				sprite.Play("flap");
			}
			sprite.OnFrameChange = OnAnimate;
			Add(wiggler = Wiggler.Create(0.4f, 4f, delegate(float v)
			{
				sprite.Scale = Vector2.One * (1f + v * 0.35f);
			}));
			Add(rotateWiggler = Wiggler.Create(0.5f, 4f, delegate(float v)
			{
				sprite.Rotation = v * 30f * ((float)Math.PI / 180f);
			}));
			Add(bloom = new BloomPoint((Golden || Moon || isGhostBerry) ? 0.5f : 1f, 12f));
			Add(light = new VertexLight(Color.White, 1f, 16, 24));
			Add(lightTween = light.CreatePulseTween());
			if (Seeds != null && Seeds.Count > 0 && !(scene as Level).Session.GetFlag(gotSeedFlag))
			{
				foreach (StrawberrySeed seed in Seeds)
				{
					scene.Add(seed);
				}
				Visible = false;
				Collidable = false;
				WaitingOnSeeds = true;
				bloom.Visible = (light.Visible = false);
			}
			if ((scene as Level).Session.BloomBaseAdd > 0.1f)
			{
				bloom.Alpha *= 0.5f;
			}
		}

		public override void Update()
		{
			if (WaitingOnSeeds)
			{
				return;
			}
			if (!collected)
			{
				if (!Winged)
				{
					wobble += Engine.DeltaTime * 4f;
					Sprite obj = sprite;
					BloomPoint bloomPoint = bloom;
					float num2 = (light.Y = (float)Math.Sin(wobble) * 2f);
					float num5 = (obj.Y = (bloomPoint.Y = num2));
				}
				int followIndex = Follower.FollowIndex;
				if (Follower.Leader != null && Follower.DelayTimer <= 0f && IsFirstStrawberry)
				{
					Player player = Follower.Leader.Entity as Player;
					bool collecting = false;
					if (player != null && player.Scene != null && !player.StrawberriesBlocked)
					{
						if (Golden)
						{
							if (player.CollideCheck<GoldBerryCollectTrigger>() || (base.Scene as Level).Completed)
							{
								collecting = true;
							}
						}
						else if (player.OnSafeGround && (!Moon || player.StateMachine.State != 13))
						{
							collecting = true;
						}
					}
					if (collecting)
					{
						collectTimer += Engine.DeltaTime;
						if (collectTimer > 0.15f)
						{
							OnCollect();
						}
					}
					else
					{
						collectTimer = Math.Min(collectTimer, 0f);
					}
				}
				else
				{
					if (followIndex > 0)
					{
						collectTimer = -0.15f;
					}
					if (Winged)
					{
						base.Y += flapSpeed * Engine.DeltaTime;
						if (flyingAway)
						{
							if (base.Y < (float)(SceneAs<Level>().Bounds.Top - 16))
							{
								RemoveSelf();
							}
						}
						else
						{
							flapSpeed = Calc.Approach(flapSpeed, 20f, 170f * Engine.DeltaTime);
							if (base.Y < start.Y - 5f)
							{
								base.Y = start.Y - 5f;
							}
							else if (base.Y > start.Y + 5f)
							{
								base.Y = start.Y + 5f;
							}
						}
					}
				}
			}
			base.Update();
			if (Follower.Leader != null && base.Scene.OnInterval(0.08f))
			{
				ParticleType type = (isGhostBerry ? P_GhostGlow : (Golden ? P_GoldGlow : ((!Moon) ? P_Glow : P_MoonGlow)));
				SceneAs<Level>().ParticlesFG.Emit(type, Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
			}
		}

		private void OnDash(Vector2 dir)
		{
			if (!flyingAway && Winged && !WaitingOnSeeds)
			{
				base.Depth = -1000000;
				Add(new Coroutine(FlyAwayRoutine()));
				flyingAway = true;
			}
		}

		private void OnAnimate(string id)
		{
			if (!flyingAway && id == "flap" && sprite.CurrentAnimationFrame % 9 == 4)
			{
				Audio.Play("event:/game/general/strawberry_wingflap", Position);
				flapSpeed = -50f;
			}
			int pulseID = ((id == "flap") ? 25 : (Golden ? 30 : ((!Moon) ? 35 : 30)));
			if (sprite.CurrentAnimationFrame == pulseID)
			{
				lightTween.Start();
				if (!collected && (CollideCheck<FakeWall>() || CollideCheck<Solid>()))
				{
					Audio.Play("event:/game/general/strawberry_pulse", Position);
					SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.1f);
				}
				else
				{
					Audio.Play("event:/game/general/strawberry_pulse", Position);
					SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
				}
			}
		}

		public void OnPlayer(Player player)
		{
			if (Follower.Leader != null || collected || WaitingOnSeeds)
			{
				return;
			}
			ReturnHomeWhenLost = true;
			if (Winged)
			{
				Level level = SceneAs<Level>();
				Winged = false;
				sprite.Rate = 0f;
				Alarm.Set(this, Follower.FollowDelay, delegate
				{
					sprite.Rate = 1f;
					sprite.Play("idle");
					level.Particles.Emit(P_WingsBurst, 8, Position + new Vector2(8f, 0f), new Vector2(4f, 2f));
					level.Particles.Emit(P_WingsBurst, 8, Position - new Vector2(8f, 0f), new Vector2(4f, 2f));
				});
			}
			if (Golden)
			{
				(base.Scene as Level).Session.GrabbedGolden = true;
			}
			Audio.Play(isGhostBerry ? "event:/game/general/strawberry_blue_touch" : "event:/game/general/strawberry_touch", Position);
			player.Leader.GainFollower(Follower);
			wiggler.Start();
			base.Depth = -1000000;
		}

		public void OnCollect()
		{
			if (!collected)
			{
				int collectIndex = 0;
				collected = true;
				if (Follower.Leader != null)
				{
					Player obj = Follower.Leader.Entity as Player;
					collectIndex = obj.StrawberryCollectIndex;
					obj.StrawberryCollectIndex++;
					obj.StrawberryCollectResetTimer = 2.5f;
					Follower.Leader.LoseFollower(Follower);
				}
				if (Moon)
				{
					Achievements.Register(Achievement.WOW);
				}
				SaveData.Instance.AddStrawberry(ID, Golden);
				Session session = (base.Scene as Level).Session;
				session.DoNotLoad.Add(ID);
				session.Strawberries.Add(ID);
				session.UpdateLevelStartDashes();
				Add(new Coroutine(CollectRoutine(collectIndex)));
			}
		}

		private IEnumerator FlyAwayRoutine()
		{
			rotateWiggler.Start();
			flapSpeed = -200f;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.25f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				flapSpeed = MathHelper.Lerp(-200f, 0f, t.Eased);
			};
			Add(tween);
			yield return 0.1f;
			Audio.Play("event:/game/general/strawberry_laugh", Position);
			yield return 0.2f;
			if (!Follower.HasLeader)
			{
				Audio.Play("event:/game/general/strawberry_flyaway", Position);
			}
			tween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.5f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				flapSpeed = MathHelper.Lerp(0f, -200f, t.Eased);
			};
			Add(tween);
		}

		private IEnumerator CollectRoutine(int collectIndex)
		{
			_ = base.Scene;
			base.Tag = Tags.TransitionUpdate;
			base.Depth = -2000010;
			int color = 0;
			if (Moon)
			{
				color = 3;
			}
			else if (isGhostBerry)
			{
				color = 1;
			}
			else if (Golden)
			{
				color = 2;
			}
			Audio.Play("event:/game/general/strawberry_get", Position, "colour", color, "count", collectIndex);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			sprite.Play("collect");
			while (sprite.Animating)
			{
				yield return null;
			}
			base.Scene.Add(new StrawberryPoints(Position, isGhostBerry, collectIndex, Moon));
			RemoveSelf();
		}

		private void OnLoseLeader()
		{
			if (collected || !ReturnHomeWhenLost)
			{
				return;
			}
			Alarm.Set(this, 0.15f, delegate
			{
				Vector2 vector = (start - Position).SafeNormalize();
				float num = Vector2.Distance(Position, start);
				float num2 = Calc.ClampedMap(num, 16f, 120f, 16f, 96f);
				Vector2 control = start + vector * 16f + vector.Perpendicular() * num2 * Calc.Random.Choose(1, -1);
				SimpleCurve curve = new SimpleCurve(Position, start, control);
				Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(num / 100f, 0.4f), start: true);
				tween.OnUpdate = delegate(Tween f)
				{
					Position = curve.GetPoint(f.Eased);
				};
				tween.OnComplete = delegate
				{
					base.Depth = 0;
				};
				Add(tween);
			});
		}

		public void CollectedSeeds()
		{
			WaitingOnSeeds = false;
			Visible = true;
			Collidable = true;
			bloom.Visible = (light.Visible = true);
			(base.Scene as Level).Session.SetFlag(gotSeedFlag);
		}
	}
}
