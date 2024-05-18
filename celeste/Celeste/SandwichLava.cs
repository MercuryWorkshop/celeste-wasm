using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class SandwichLava : Entity
	{
		private const float TopOffset = -160f;

		private const float Speed = 20f;

		public bool Waiting;

		private bool iceMode;

		private float startX;

		private float lerp;

		private float transitionStartY;

		private bool leaving;

		private float delay;

		private LavaRect bottomRect;

		private LavaRect topRect;

		private bool persistent;

		private SoundSource loopSfx;

		private float centerY => (float)SceneAs<Level>().Bounds.Bottom - 10f;

		public SandwichLava(float startX)
		{
			this.startX = startX;
			base.Depth = -1000000;
			base.Collider = new ColliderList(new Hitbox(340f, 120f), new Hitbox(340f, 120f, 0f, -280f));
			Visible = false;
			Add(loopSfx = new SoundSource());
			Add(new PlayerCollider(OnPlayer));
			Add(new CoreModeListener(OnChangeMode));
			Add(bottomRect = new LavaRect(400f, 200f, 4));
			bottomRect.Position = new Vector2(-40f, 0f);
			bottomRect.OnlyMode = LavaRect.OnlyModes.OnlyTop;
			bottomRect.SmallWaveAmplitude = 2f;
			Add(topRect = new LavaRect(400f, 200f, 4));
			topRect.Position = new Vector2(-40f, -360f);
			topRect.OnlyMode = LavaRect.OnlyModes.OnlyBottom;
			topRect.SmallWaveAmplitude = 2f;
			topRect.BigWaveAmplitude = (bottomRect.BigWaveAmplitude = 2f);
			topRect.CurveAmplitude = (bottomRect.CurveAmplitude = 4f);
			Add(new TransitionListener
			{
				OnOutBegin = delegate
				{
					transitionStartY = base.Y;
					if (persistent && base.Scene != null && base.Scene.Entities.FindAll<SandwichLava>().Count <= 1)
					{
						Leave();
					}
				},
				OnOut = delegate(float f)
				{
					if (base.Scene != null)
					{
						base.X = (base.Scene as Level).Camera.X;
						if (!leaving)
						{
							base.Y = MathHelper.Lerp(transitionStartY, centerY, f);
						}
					}
					if ((f > 0.95f) & leaving)
					{
						RemoveSelf();
					}
				}
			});
		}

		public SandwichLava(EntityData data, Vector2 offset)
			: this(data.Position.X + offset.X)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			base.X = SceneAs<Level>().Bounds.Left - 10;
			base.Y = centerY;
			iceMode = SceneAs<Level>().Session.CoreMode == Session.CoreModes.Cold;
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null && (player.JustRespawned || player.X < startX))
			{
				Waiting = true;
			}
			List<SandwichLava> lavas = base.Scene.Entities.FindAll<SandwichLava>();
			bool removing = false;
			if (!persistent && lavas.Count >= 2)
			{
				SandwichLava other = ((lavas[0] == this) ? lavas[1] : lavas[0]);
				if (!other.leaving)
				{
					other.startX = startX;
					other.Waiting = true;
					RemoveSelf();
					removing = true;
				}
			}
			if (!removing)
			{
				persistent = true;
				base.Tag = Tags.Persistent;
				if ((scene as Level).LastIntroType != Player.IntroTypes.Respawn)
				{
					topRect.Position.Y -= 60f;
					bottomRect.Position.Y += 60f;
				}
				else
				{
					Visible = true;
				}
				loopSfx.Play("event:/game/09_core/rising_threat", "room_state", iceMode ? 1 : 0);
				loopSfx.Position = new Vector2(base.Width / 2f, 0f);
			}
		}

		private void OnChangeMode(Session.CoreModes mode)
		{
			iceMode = mode == Session.CoreModes.Cold;
			loopSfx.Param("room_state", iceMode ? 1 : 0);
		}

		private void OnPlayer(Player player)
		{
			if (Waiting)
			{
				return;
			}
			if (SaveData.Instance.Assists.Invincible)
			{
				if (delay <= 0f)
				{
					int dir = ((player.Y > base.Y + bottomRect.Position.Y - 32f) ? 1 : (-1));
					float from = base.Y;
					float to = base.Y + (float)(dir * 48);
					player.Speed.Y = -dir * 200;
					if (dir > 0)
					{
						player.RefillDash();
					}
					Tween.Set(this, Tween.TweenMode.Oneshot, 0.4f, Ease.CubeOut, delegate(Tween t)
					{
						base.Y = MathHelper.Lerp(from, to, t.Eased);
					});
					delay = 0.5f;
					loopSfx.Param("rising", 0f);
					Audio.Play("event:/game/general/assist_screenbottom", player.Position);
				}
			}
			else
			{
				player.Die(-Vector2.UnitY);
			}
		}

		public void Leave()
		{
			AddTag(Tags.TransitionUpdate);
			leaving = true;
			Collidable = false;
			Alarm.Set(this, 2f, delegate
			{
				RemoveSelf();
			});
		}

		public override void Update()
		{
			Level level = base.Scene as Level;
			base.X = level.Camera.X;
			delay -= Engine.DeltaTime;
			base.Update();
			Visible = true;
			if (Waiting)
			{
				base.Y = Calc.Approach(base.Y, centerY, 128f * Engine.DeltaTime);
				loopSfx.Param("rising", 0f);
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && player.X >= startX && !player.JustRespawned && player.StateMachine.State != 11)
				{
					Waiting = false;
				}
			}
			else if (!leaving && delay <= 0f)
			{
				loopSfx.Param("rising", 1f);
				if (iceMode)
				{
					base.Y += 20f * Engine.DeltaTime;
				}
				else
				{
					base.Y -= 20f * Engine.DeltaTime;
				}
			}
			topRect.Position.Y = Calc.Approach(topRect.Position.Y, -160f - topRect.Height + (float)(leaving ? (-512) : 0), (float)(leaving ? 256 : 64) * Engine.DeltaTime);
			bottomRect.Position.Y = Calc.Approach(bottomRect.Position.Y, leaving ? 512 : 0, (float)(leaving ? 256 : 64) * Engine.DeltaTime);
			lerp = Calc.Approach(lerp, iceMode ? 1 : 0, Engine.DeltaTime * 4f);
			bottomRect.SurfaceColor = Color.Lerp(RisingLava.Hot[0], RisingLava.Cold[0], lerp);
			bottomRect.EdgeColor = Color.Lerp(RisingLava.Hot[1], RisingLava.Cold[1], lerp);
			bottomRect.CenterColor = Color.Lerp(RisingLava.Hot[2], RisingLava.Cold[2], lerp);
			bottomRect.Spikey = lerp * 5f;
			bottomRect.UpdateMultiplier = (1f - lerp) * 2f;
			bottomRect.Fade = (iceMode ? 128 : 32);
			topRect.SurfaceColor = bottomRect.SurfaceColor;
			topRect.EdgeColor = bottomRect.EdgeColor;
			topRect.CenterColor = bottomRect.CenterColor;
			topRect.Spikey = bottomRect.Spikey;
			topRect.UpdateMultiplier = bottomRect.UpdateMultiplier;
			topRect.Fade = bottomRect.Fade;
		}
	}
}
