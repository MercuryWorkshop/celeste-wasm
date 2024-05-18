using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class BadelineOldsite : Entity
	{
		public static ParticleType P_Vanish;

		public static readonly Color HairColor = Calc.HexToColor("9B3FB5");

		public PlayerSprite Sprite;

		public PlayerHair Hair;

		private LightOcclude occlude;

		private bool ignorePlayerAnim;

		private int index;

		private Player player;

		private bool following;

		private float followBehindTime;

		private float followBehindIndexDelay;

		public bool Hovering;

		private float hoveringTimer;

		private Dictionary<string, SoundSource> loopingSounds = new Dictionary<string, SoundSource>();

		private List<SoundSource> inactiveLoopingSounds = new List<SoundSource>();

		public BadelineOldsite(Vector2 position, int index)
			: base(position)
		{
			this.index = index;
			base.Depth = -1;
			base.Collider = new Hitbox(6f, 6f, -3f, -7f);
			Collidable = false;
			Sprite = new PlayerSprite(PlayerSpriteMode.Badeline);
			Sprite.Play("fallSlow", restart: true);
			Hair = new PlayerHair(Sprite);
			Hair.Color = Color.Lerp(HairColor, Color.White, (float)index / 6f);
			Hair.Border = Color.Black;
			Add(Hair);
			Add(Sprite);
			Visible = false;
			followBehindTime = 1.55f;
			followBehindIndexDelay = 0.4f * (float)index;
			Add(new PlayerCollider(OnPlayer));
		}

		public BadelineOldsite(EntityData data, Vector2 offset, int index)
			: this(data.Position + offset, index)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Session session = SceneAs<Level>().Session;
			if (session.GetLevelFlag("11") && session.Area.Mode == AreaMode.Normal)
			{
				RemoveSelf();
			}
			else if (!session.GetLevelFlag("3") && session.Area.Mode == AreaMode.Normal)
			{
				RemoveSelf();
			}
			else if (!session.GetFlag("evil_maddy_intro") && session.Level == "3" && session.Area.Mode == AreaMode.Normal)
			{
				Hovering = false;
				Visible = true;
				Hair.Visible = false;
				Sprite.Play("pretendDead");
				if (session.Area.Mode == AreaMode.Normal)
				{
					session.Audio.Music.Event = null;
					session.Audio.Apply();
				}
				base.Scene.Add(new CS02_BadelineIntro(this));
			}
			else
			{
				Add(new Coroutine(StartChasingRoutine(base.Scene as Level)));
			}
		}

		public IEnumerator StartChasingRoutine(Level level)
		{
			Hovering = true;
			while ((player = base.Scene.Tracker.GetEntity<Player>()) == null || player.JustRespawned)
			{
				yield return null;
			}
			Vector2 to = player.Position;
			yield return followBehindIndexDelay;
			if (!Visible)
			{
				PopIntoExistance(0.5f);
			}
			Sprite.Play("fallSlow");
			Hair.Visible = true;
			Hovering = false;
			if (level.Session.Area.Mode == AreaMode.Normal)
			{
				level.Session.Audio.Music.Event = "event:/music/lvl2/chase";
				level.Session.Audio.Apply();
			}
			yield return TweenToPlayer(to);
			Collidable = true;
			following = true;
			Add(occlude = new LightOcclude());
			if (level.Session.Level == "2")
			{
				Add(new Coroutine(StopChasing()));
			}
		}

		private IEnumerator TweenToPlayer(Vector2 to)
		{
			Audio.Play("event:/char/badeline/level_entry", Position, "chaser_count", index);
			Vector2 from = Position;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, followBehindTime - 0.1f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				Position = Vector2.Lerp(from, to, t.Eased);
				if (to.X != from.X)
				{
					Sprite.Scale.X = Math.Abs(Sprite.Scale.X) * (float)Math.Sign(to.X - from.X);
				}
				Trail();
			};
			Add(tween);
			yield return tween.Duration;
		}

		private IEnumerator StopChasing()
		{
			Level level = SceneAs<Level>();
			int boundsRight = level.Bounds.X + 148;
			int boundsBottom = level.Bounds.Y + 168 + 184;
			while (base.X != (float)boundsRight || base.Y != (float)boundsBottom)
			{
				yield return null;
				if (base.X > (float)boundsRight)
				{
					base.X = boundsRight;
				}
				if (base.Y > (float)boundsBottom)
				{
					base.Y = boundsBottom;
				}
			}
			following = false;
			ignorePlayerAnim = true;
			Sprite.Play("laugh");
			Sprite.Scale.X = 1f;
			yield return 1f;
			Audio.Play("event:/char/badeline/disappear", Position);
			level.Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
			level.Particles.Emit(P_Vanish, 12, base.Center, Vector2.One * 6f);
			RemoveSelf();
		}

		public override void Update()
		{
			Player.ChaserState state;
			if (player != null && player.Dead)
			{
				Sprite.Play("laugh");
				Sprite.X = (float)(Math.Sin(hoveringTimer) * 4.0);
				Hovering = true;
				hoveringTimer += Engine.DeltaTime * 2f;
				base.Depth = -12500;
				foreach (KeyValuePair<string, SoundSource> loopingSound in loopingSounds)
				{
					loopingSound.Value.Stop();
				}
				Trail();
			}
			else if (following && player.GetChasePosition(base.Scene.TimeActive, followBehindTime + followBehindIndexDelay, out state))
			{
				Position = Calc.Approach(Position, state.Position, 500f * Engine.DeltaTime);
				if (!ignorePlayerAnim && state.Animation != Sprite.CurrentAnimationID && state.Animation != null && Sprite.Has(state.Animation))
				{
					Sprite.Play(state.Animation, restart: true);
				}
				if (!ignorePlayerAnim)
				{
					Sprite.Scale.X = Math.Abs(Sprite.Scale.X) * (float)state.Facing;
				}
				for (int i = 0; i < state.Sounds; i++)
				{
					if (state[i].Action == Player.ChaserStateSound.Actions.Oneshot)
					{
						Audio.Play(state[i].Event, Position, state[i].Parameter, state[i].ParameterValue, "chaser_count", index);
					}
					else if (state[i].Action == Player.ChaserStateSound.Actions.Loop && !loopingSounds.ContainsKey(state[i].Event))
					{
						SoundSource loopSfx2;
						if (inactiveLoopingSounds.Count > 0)
						{
							loopSfx2 = inactiveLoopingSounds[0];
							inactiveLoopingSounds.RemoveAt(0);
						}
						else
						{
							Add(loopSfx2 = new SoundSource());
						}
						loopSfx2.Play(state[i].Event, "chaser_count", index);
						loopingSounds.Add(state[i].Event, loopSfx2);
					}
					else if (state[i].Action == Player.ChaserStateSound.Actions.Stop)
					{
						SoundSource loopSfx = null;
						if (loopingSounds.TryGetValue(state[i].Event, out loopSfx))
						{
							loopSfx.Stop();
							loopingSounds.Remove(state[i].Event);
							inactiveLoopingSounds.Add(loopSfx);
						}
					}
				}
				base.Depth = state.Depth;
				Trail();
			}
			if (Sprite.Scale.X != 0f)
			{
				Hair.Facing = (Facings)Math.Sign(Sprite.Scale.X);
			}
			if (Hovering)
			{
				hoveringTimer += Engine.DeltaTime;
				Sprite.Y = (float)(Math.Sin(hoveringTimer * 2f) * 4.0);
			}
			else
			{
				Sprite.Y = Calc.Approach(Sprite.Y, 0f, Engine.DeltaTime * 4f);
			}
			if (occlude != null)
			{
				occlude.Visible = !CollideCheck<Solid>();
			}
			base.Update();
		}

		private void Trail()
		{
			if (base.Scene.OnInterval(0.1f))
			{
				TrailManager.Add(this, Player.NormalHairColor);
			}
		}

		private void OnPlayer(Player player)
		{
			player.Die((player.Position - Position).SafeNormalize());
		}

		private void Die()
		{
			RemoveSelf();
		}

		private void PopIntoExistance(float duration)
		{
			Visible = true;
			Sprite.Scale = Vector2.Zero;
			Sprite.Color = Color.Transparent;
			Hair.Visible = true;
			Hair.Alpha = 0f;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, duration, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				Sprite.Scale = Vector2.One * t.Eased;
				Sprite.Color = Color.White * t.Eased;
				Hair.Alpha = t.Eased;
			};
			Add(tween);
		}

		private bool OnGround(int dist = 1)
		{
			for (int i = 1; i <= dist; i++)
			{
				if (CollideCheck<Solid>(Position + new Vector2(0f, i)))
				{
					return true;
				}
			}
			return false;
		}
	}
}
