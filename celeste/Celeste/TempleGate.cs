using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class TempleGate : Solid
	{
		public enum Types
		{
			NearestSwitch,
			CloseBehindPlayer,
			CloseBehindPlayerAlways,
			HoldingTheo,
			TouchSwitches,
			CloseBehindPlayerAndTheo
		}

		private const int OpenHeight = 0;

		private const float HoldingWaitTime = 0.2f;

		private const float HoldingOpenDistSq = 4096f;

		private const float HoldingCloseDistSq = 6400f;

		private const int MinDrawHeight = 4;

		public string LevelID;

		public Types Type;

		public bool ClaimedByASwitch;

		private bool theoGate;

		private int closedHeight;

		private Sprite sprite;

		private Shaker shaker;

		private float drawHeight;

		private float drawHeightMoveSpeed;

		private bool open;

		private float holdingWaitTimer = 0.2f;

		private Vector2 holdingCheckFrom;

		private bool lockState;

		public TempleGate(Vector2 position, int height, Types type, string spriteName, string levelID)
			: base(position, 8f, height, safe: true)
		{
			Type = type;
			closedHeight = height;
			LevelID = levelID;
			Add(sprite = GFX.SpriteBank.Create("templegate_" + spriteName));
			sprite.X = base.Collider.Width / 2f;
			sprite.Play("idle");
			Add(shaker = new Shaker(on: false));
			base.Depth = -9000;
			theoGate = spriteName.Equals("theo", StringComparison.InvariantCultureIgnoreCase);
			holdingCheckFrom = Position + new Vector2(base.Width / 2f, height / 2);
		}

		public TempleGate(EntityData data, Vector2 offset, string levelID)
			: this(data.Position + offset, data.Height, data.Enum("type", Types.NearestSwitch), data.Attr("sprite", "default"), levelID)
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (Type == Types.CloseBehindPlayer)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && player.Left < base.Right && player.Bottom >= base.Top && player.Top <= base.Bottom)
				{
					StartOpen();
					Add(new Coroutine(CloseBehindPlayer()));
				}
			}
			else if (Type == Types.CloseBehindPlayerAlways)
			{
				StartOpen();
				Add(new Coroutine(CloseBehindPlayer()));
			}
			else if (Type == Types.CloseBehindPlayerAndTheo)
			{
				StartOpen();
				Add(new Coroutine(CloseBehindPlayerAndTheo()));
			}
			else if (Type == Types.HoldingTheo)
			{
				if (TheoIsNearby())
				{
					StartOpen();
				}
				base.Hitbox.Width = 16f;
			}
			else if (Type == Types.TouchSwitches)
			{
				Add(new Coroutine(CheckTouchSwitches()));
			}
			drawHeight = Math.Max(4f, base.Height);
		}

		public bool CloseBehindPlayerCheck()
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				return player.X < base.X;
			}
			return false;
		}

		public void SwitchOpen()
		{
			sprite.Play("open");
			Alarm.Set(this, 0.2f, delegate
			{
				shaker.ShakeFor(0.2f, removeOnFinish: false);
				Alarm.Set(this, 0.2f, Open);
			});
		}

		public void Open()
		{
			Audio.Play(theoGate ? "event:/game/05_mirror_temple/gate_theo_open" : "event:/game/05_mirror_temple/gate_main_open", Position);
			holdingWaitTimer = 0.2f;
			drawHeightMoveSpeed = 200f;
			drawHeight = base.Height;
			shaker.ShakeFor(0.2f, removeOnFinish: false);
			SetHeight(0);
			sprite.Play("open");
			open = true;
		}

		public void StartOpen()
		{
			SetHeight(0);
			drawHeight = 4f;
			open = true;
		}

		public void Close()
		{
			Audio.Play(theoGate ? "event:/game/05_mirror_temple/gate_theo_close" : "event:/game/05_mirror_temple/gate_main_close", Position);
			holdingWaitTimer = 0.2f;
			drawHeightMoveSpeed = 300f;
			drawHeight = Math.Max(4f, base.Height);
			shaker.ShakeFor(0.2f, removeOnFinish: false);
			SetHeight(closedHeight);
			sprite.Play("hit");
			open = false;
		}

		private IEnumerator CloseBehindPlayer()
		{
			while (true)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (!lockState && player != null && player.Left > base.Right + 4f)
				{
					break;
				}
				yield return null;
			}
			Close();
		}

		private IEnumerator CloseBehindPlayerAndTheo()
		{
			while (true)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && player.Left > base.Right + 4f)
				{
					TheoCrystal theo = base.Scene.Tracker.GetEntity<TheoCrystal>();
					if (!lockState && theo != null && theo.Left > base.Right + 4f)
					{
						break;
					}
				}
				yield return null;
			}
			Close();
		}

		private IEnumerator CheckTouchSwitches()
		{
			while (!Switch.Check(base.Scene))
			{
				yield return null;
			}
			sprite.Play("open");
			yield return 0.5f;
			shaker.ShakeFor(0.2f, removeOnFinish: false);
			yield return 0.2f;
			while (lockState)
			{
				yield return null;
			}
			Open();
		}

		public bool TheoIsNearby()
		{
			TheoCrystal theo = base.Scene.Tracker.GetEntity<TheoCrystal>();
			if (theo != null && !(theo.X > base.X + 10f))
			{
				return Vector2.DistanceSquared(holdingCheckFrom, theo.Center) < (open ? 6400f : 4096f);
			}
			return true;
		}

		private void SetHeight(int height)
		{
			if ((float)height < base.Collider.Height)
			{
				base.Collider.Height = height;
				return;
			}
			float atY = base.Y;
			int oldHeight = (int)base.Collider.Height;
			if (base.Collider.Height < 64f)
			{
				base.Y -= 64f - base.Collider.Height;
				base.Collider.Height = 64f;
			}
			MoveVExact(height - oldHeight);
			base.Y = atY;
			base.Collider.Height = height;
		}

		public override void Update()
		{
			base.Update();
			if (Type == Types.HoldingTheo)
			{
				if (holdingWaitTimer > 0f)
				{
					holdingWaitTimer -= Engine.DeltaTime;
				}
				else if (!lockState)
				{
					if (open && !TheoIsNearby())
					{
						Close();
						CollideFirst<Player>(Position + new Vector2(8f, 0f))?.Die(Vector2.Zero);
					}
					else if (!open && TheoIsNearby())
					{
						Open();
					}
				}
			}
			float drawTarget = Math.Max(4f, base.Height);
			if (drawHeight != drawTarget)
			{
				lockState = true;
				drawHeight = Calc.Approach(drawHeight, drawTarget, drawHeightMoveSpeed * Engine.DeltaTime);
			}
			else
			{
				lockState = false;
			}
		}

		public override void Render()
		{
			Vector2 shake = new Vector2(Math.Sign(shaker.Value.X), 0f);
			Draw.Rect(base.X - 2f, base.Y - 8f, 14f, 10f, Color.Black);
			sprite.DrawSubrect(Vector2.Zero + shake, new Rectangle(0, (int)(sprite.Height - drawHeight), (int)sprite.Width, (int)drawHeight));
		}
	}
}
