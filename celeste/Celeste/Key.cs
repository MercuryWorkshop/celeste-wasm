using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Key : Entity
	{
		public static ParticleType P_Shimmer;

		public static ParticleType P_Insert;

		public static ParticleType P_Collect;

		public EntityID ID;

		public bool IsUsed;

		public bool StartedUsing;

		private Follower follower;

		private Sprite sprite;

		private Wiggler wiggler;

		private VertexLight light;

		private ParticleEmitter shimmerParticles;

		private float wobble;

		private bool wobbleActive;

		private Tween tween;

		private Alarm alarm;

		private Vector2[] nodes;

		public bool Turning { get; private set; }

		public Key(Vector2 position, EntityID id, Vector2[] nodes)
			: base(position)
		{
			ID = id;
			base.Collider = new Hitbox(12f, 12f, -6f, -6f);
			this.nodes = nodes;
			Add(follower = new Follower(id));
			Add(new PlayerCollider(OnPlayer));
			Add(new MirrorReflection());
			Add(sprite = GFX.SpriteBank.Create("key"));
			sprite.CenterOrigin();
			sprite.Play("idle");
			Add(new TransitionListener
			{
				OnOut = delegate
				{
					StartedUsing = false;
					if (!IsUsed)
					{
						if (tween != null)
						{
							tween.RemoveSelf();
							tween = null;
						}
						if (alarm != null)
						{
							alarm.RemoveSelf();
							alarm = null;
						}
						Turning = false;
						Visible = true;
						sprite.Visible = true;
						sprite.Rate = 1f;
						sprite.Scale = Vector2.One;
						sprite.Play("idle");
						sprite.Rotation = 0f;
						wiggler.Stop();
						follower.MoveTowardsLeader = true;
					}
				}
			});
			Add(wiggler = Wiggler.Create(0.4f, 4f, delegate(float v)
			{
				sprite.Scale = Vector2.One * (1f + v * 0.35f);
			}));
			Add(light = new VertexLight(Color.White, 1f, 32, 48));
		}

		public Key(EntityData data, Vector2 offset, EntityID id)
			: this(data.Position + offset, id, data.NodesOffset(offset))
		{
		}

		public Key(Player player, EntityID id)
			: this(player.Position + new Vector2(-12 * (int)player.Facing, -8f), id, null)
		{
			player.Leader.GainFollower(follower);
			Collidable = false;
			base.Depth = -1000000;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			ParticleSystem system = (scene as Level).ParticlesFG;
			Add(shimmerParticles = new ParticleEmitter(system, P_Shimmer, Vector2.Zero, new Vector2(6f, 6f), 1, 0.1f));
			shimmerParticles.SimulateCycle();
		}

		public override void Update()
		{
			if (wobbleActive)
			{
				wobble += Engine.DeltaTime * 4f;
				sprite.Y = (float)Math.Sin(wobble);
			}
			base.Update();
		}

		private void OnPlayer(Player player)
		{
			SceneAs<Level>().Particles.Emit(P_Collect, 10, Position, Vector2.One * 3f);
			Audio.Play("event:/game/general/key_get", Position);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			player.Leader.GainFollower(follower);
			Collidable = false;
			Session session = SceneAs<Level>().Session;
			session.DoNotLoad.Add(ID);
			session.Keys.Add(ID);
			session.UpdateLevelStartDashes();
			wiggler.Start();
			base.Depth = -1000000;
			if (nodes != null && nodes.Length >= 2)
			{
				Add(new Coroutine(NodeRoutine(player)));
			}
		}

		private IEnumerator NodeRoutine(Player player)
		{
			yield return 0.3f;
			if (!player.Dead)
			{
				Audio.Play("event:/game/general/cassette_bubblereturn", SceneAs<Level>().Camera.Position + new Vector2(160f, 90f));
				player.StartCassetteFly(nodes[1], nodes[0]);
			}
		}

		public void RegisterUsed()
		{
			IsUsed = true;
			if (follower.Leader != null)
			{
				follower.Leader.LoseFollower(follower);
			}
			SceneAs<Level>().Session.Keys.Remove(ID);
		}

		public IEnumerator UseRoutine(Vector2 target)
		{
			Turning = true;
			follower.MoveTowardsLeader = false;
			wiggler.Start();
			wobbleActive = false;
			sprite.Y = 0f;
			Vector2 from = Position;
			SimpleCurve curve = new SimpleCurve(from, target, (target + from) / 2f + new Vector2(0f, -48f));
			tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 1f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				Position = curve.GetPoint(t.Eased);
				sprite.Rate = 1f + t.Eased * 2f;
			};
			Add(tween);
			yield return tween.Wait();
			tween = null;
			while (sprite.CurrentAnimationFrame != 4)
			{
				yield return null;
			}
			shimmerParticles.Active = false;
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			for (int j = 0; j < 16; j++)
			{
				SceneAs<Level>().ParticlesFG.Emit(P_Insert, base.Center, (float)Math.PI / 8f * (float)j);
			}
			sprite.Play("enter");
			yield return 0.3f;
			tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.3f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				sprite.Rotation = t.Eased * ((float)Math.PI / 2f);
			};
			Add(tween);
			yield return tween.Wait();
			tween = null;
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			alarm = Alarm.Set(this, 1f, delegate
			{
				alarm = null;
				tween = Tween.Create(Tween.TweenMode.Oneshot, null, 1f, start: true);
				tween.OnUpdate = delegate(Tween t)
				{
					light.Alpha = 1f - t.Eased;
				};
				tween.OnComplete = delegate
				{
					RemoveSelf();
				};
				Add(tween);
			});
			yield return 0.2f;
			for (int i = 0; i < 8; i++)
			{
				SceneAs<Level>().ParticlesFG.Emit(P_Insert, base.Center, (float)Math.PI / 4f * (float)i);
			}
			sprite.Visible = false;
			Turning = false;
		}
	}
}
