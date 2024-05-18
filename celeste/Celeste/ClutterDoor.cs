using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ClutterDoor : Solid
	{
		public ClutterBlock.Colors Color;

		private Sprite sprite;

		private Wiggler wiggler;

		public ClutterDoor(EntityData data, Vector2 offset, Session session)
			: base(data.Position + offset, data.Width, data.Height, safe: false)
		{
			Color = data.Enum("type", ClutterBlock.Colors.Green);
			SurfaceSoundIndex = 20;
			base.Tag = Tags.TransitionUpdate;
			Add(sprite = GFX.SpriteBank.Create("ghost_door"));
			sprite.Position = new Vector2(base.Width, base.Height) / 2f;
			sprite.Play("idle");
			OnDashCollide = OnDashed;
			Add(wiggler = Wiggler.Create(0.6f, 3f, delegate(float f)
			{
				sprite.Scale = Vector2.One * (1f - f * 0.2f);
			}));
			if (!IsLocked(session))
			{
				InstantUnlock();
			}
		}

		public override void Update()
		{
			Level level = base.Scene as Level;
			if (level.Transitioning && CollideCheck<Player>())
			{
				Visible = false;
				Collidable = false;
			}
			else if (!Collidable && IsLocked(level.Session) && !CollideCheck<Player>())
			{
				Visible = true;
				Collidable = true;
				wiggler.Start();
				Audio.Play("event:/game/03_resort/forcefield_bump", Position);
			}
			base.Update();
		}

		public bool IsLocked(Session session)
		{
			if (session.GetFlag("oshiro_clutter_door_open"))
			{
				return IsComplete(session);
			}
			return true;
		}

		public bool IsComplete(Session session)
		{
			return session.GetFlag("oshiro_clutter_cleared_" + (int)Color);
		}

		public IEnumerator UnlockRoutine()
		{
			Camera camera = SceneAs<Level>().Camera;
			Vector2 from = camera.Position;
			Vector2 to = CameraTarget();
			if ((from - to).Length() > 8f)
			{
				for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime)
				{
					camera.Position = from + (to - from) * Ease.CubeInOut(p2);
					yield return null;
				}
			}
			else
			{
				yield return 0.2f;
			}
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			Audio.Play("event:/game/03_resort/forcefield_vanish", Position);
			sprite.Play("open");
			Collidable = false;
			for (float p2 = 0f; p2 < 0.4f; p2 += Engine.DeltaTime)
			{
				camera.Position = CameraTarget();
				yield return null;
			}
		}

		public void InstantUnlock()
		{
			Visible = (Collidable = false);
		}

		private Vector2 CameraTarget()
		{
			Level level = SceneAs<Level>();
			Vector2 target = Position - new Vector2(320f, 180f) / 2f;
			target.X = MathHelper.Clamp(target.X, level.Bounds.Left, level.Bounds.Right - 320);
			target.Y = MathHelper.Clamp(target.Y, level.Bounds.Top, level.Bounds.Bottom - 180);
			return target;
		}

		private DashCollisionResults OnDashed(Player player, Vector2 direction)
		{
			wiggler.Start();
			Audio.Play("event:/game/03_resort/forcefield_bump", Position);
			return DashCollisionResults.Bounce;
		}
	}
}
