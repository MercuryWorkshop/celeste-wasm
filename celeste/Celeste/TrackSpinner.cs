using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class TrackSpinner : Entity
	{
		public enum Speeds
		{
			Slow,
			Normal,
			Fast
		}

		public static readonly float[] PauseTimes = new float[3] { 0.3f, 0.2f, 0.6f };

		public static readonly float[] MoveTimes = new float[3] { 0.9f, 0.4f, 0.3f };

		public bool Up = true;

		public float PauseTimer;

		public Speeds Speed;

		public bool Moving = true;

		public float Angle;

		public Vector2 Start { get; private set; }

		public Vector2 End { get; private set; }

		public float Percent { get; private set; }

		public TrackSpinner(EntityData data, Vector2 offset)
		{
			base.Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
			Add(new PlayerCollider(OnPlayer));
			Start = data.Position + offset;
			End = data.Nodes[0] + offset;
			Speed = data.Enum("speed", Speeds.Normal);
			Angle = (Start - End).Angle();
			Percent = (data.Bool("startCenter") ? 0.5f : 0f);
			if (Percent == 1f)
			{
				Up = false;
			}
			UpdatePosition();
		}

		public void UpdatePosition()
		{
			Position = Vector2.Lerp(Start, End, Ease.SineInOut(Percent));
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			OnTrackStart();
		}

		public override void Update()
		{
			base.Update();
			if (!Moving)
			{
				return;
			}
			if (PauseTimer > 0f)
			{
				PauseTimer -= Engine.DeltaTime;
				if (PauseTimer <= 0f)
				{
					OnTrackStart();
				}
				return;
			}
			Percent = Calc.Approach(Percent, Up ? 1 : 0, Engine.DeltaTime / MoveTimes[(int)Speed]);
			UpdatePosition();
			if ((Up && Percent == 1f) || (!Up && Percent == 0f))
			{
				Up = !Up;
				PauseTimer = PauseTimes[(int)Speed];
				OnTrackEnd();
			}
		}

		public virtual void OnPlayer(Player player)
		{
			if (player.Die((player.Position - Position).SafeNormalize()) != null)
			{
				Moving = false;
			}
		}

		public virtual void OnTrackStart()
		{
		}

		public virtual void OnTrackEnd()
		{
		}
	}
}
