using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class RisingLava : Entity
	{
		private const float Speed = -30f;

		private bool intro;

		private bool iceMode;

		private bool waiting;

		private float lerp;

		public static Color[] Hot = new Color[3]
		{
			Calc.HexToColor("ff8933"),
			Calc.HexToColor("f25e29"),
			Calc.HexToColor("d01c01")
		};

		public static Color[] Cold = new Color[3]
		{
			Calc.HexToColor("33ffe7"),
			Calc.HexToColor("4ca2eb"),
			Calc.HexToColor("0151d0")
		};

		private LavaRect bottomRect;

		private float delay;

		private SoundSource loopSfx;

		public RisingLava(bool intro)
		{
			this.intro = intro;
			base.Depth = -1000000;
			base.Collider = new Hitbox(340f, 120f);
			Visible = false;
			Add(new PlayerCollider(OnPlayer));
			Add(new CoreModeListener(OnChangeMode));
			Add(loopSfx = new SoundSource());
			Add(bottomRect = new LavaRect(400f, 200f, 4));
			bottomRect.Position = new Vector2(-40f, 0f);
			bottomRect.OnlyMode = LavaRect.OnlyModes.OnlyTop;
			bottomRect.SmallWaveAmplitude = 2f;
		}

		public RisingLava(EntityData data, Vector2 offset)
			: this(data.Bool("intro"))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			base.X = SceneAs<Level>().Bounds.Left - 10;
			base.Y = SceneAs<Level>().Bounds.Bottom + 16;
			iceMode = SceneAs<Level>().Session.CoreMode == Session.CoreModes.Cold;
			loopSfx.Play("event:/game/09_core/rising_threat", "room_state", iceMode ? 1 : 0);
			loopSfx.Position = new Vector2(base.Width / 2f, 0f);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (intro)
			{
				waiting = true;
			}
			else
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && player.JustRespawned)
				{
					waiting = true;
				}
			}
			if (intro)
			{
				Visible = true;
			}
		}

		private void OnChangeMode(Session.CoreModes mode)
		{
			iceMode = mode == Session.CoreModes.Cold;
			loopSfx.Param("room_state", iceMode ? 1 : 0);
		}

		private void OnPlayer(Player player)
		{
			if (SaveData.Instance.Assists.Invincible)
			{
				if (delay <= 0f)
				{
					float from = base.Y;
					float to = base.Y + 48f;
					player.Speed.Y = -200f;
					player.RefillDash();
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

		public override void Update()
		{
			delay -= Engine.DeltaTime;
			base.X = SceneAs<Level>().Camera.X;
			Player player = base.Scene.Tracker.GetEntity<Player>();
			base.Update();
			Visible = true;
			float mult = 1f;
			if (waiting)
			{
				loopSfx.Param("rising", 0f);
				if (!intro && player != null && player.JustRespawned)
				{
					base.Y = Calc.Approach(base.Y, player.Y + 32f, 32f * Engine.DeltaTime);
				}
				if ((!iceMode || !intro) && (player == null || !player.JustRespawned))
				{
					waiting = false;
				}
			}
			else
			{
				float center = SceneAs<Level>().Camera.Bottom - 12f;
				if (base.Top > center + 96f)
				{
					base.Top = center + 96f;
				}
				mult = ((!(base.Top > center)) ? Calc.ClampedMap(center - base.Top, 0f, 32f, 1f, 0.5f) : Calc.ClampedMap(base.Top - center, 0f, 96f, 1f, 2f));
				if (delay <= 0f)
				{
					loopSfx.Param("rising", 1f);
					base.Y += -30f * mult * Engine.DeltaTime;
				}
			}
			lerp = Calc.Approach(lerp, iceMode ? 1 : 0, Engine.DeltaTime * 4f);
			bottomRect.SurfaceColor = Color.Lerp(Hot[0], Cold[0], lerp);
			bottomRect.EdgeColor = Color.Lerp(Hot[1], Cold[1], lerp);
			bottomRect.CenterColor = Color.Lerp(Hot[2], Cold[2], lerp);
			bottomRect.Spikey = lerp * 5f;
			bottomRect.UpdateMultiplier = (1f - lerp) * 2f;
			bottomRect.Fade = (iceMode ? 128 : 32);
		}
	}
}
