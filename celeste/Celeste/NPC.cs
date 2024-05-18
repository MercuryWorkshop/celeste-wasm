using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC : Entity
	{
		public const string MetTheo = "MetTheo";

		public const string TheoKnowsName = "TheoKnowsName";

		public const float TheoMaxSpeed = 48f;

		public Sprite Sprite;

		public TalkComponent Talker;

		public VertexLight Light;

		public Level Level;

		public SoundSource PhoneTapSfx;

		public float Maxspeed = 80f;

		public string MoveAnim = "";

		public string IdleAnim = "";

		public bool MoveY = true;

		public bool UpdateLight = true;

		private List<Entity> temp = new List<Entity>();

		public Session Session => Level.Session;

		public NPC(Vector2 position)
		{
			Position = position;
			base.Depth = 1000;
			base.Collider = new Hitbox(8f, 8f, -4f, -8f);
			Add(new MirrorReflection());
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Level = scene as Level;
		}

		public override void Update()
		{
			base.Update();
			if (UpdateLight && Light != null)
			{
				Rectangle bounds = Level.Bounds;
				bool inview = base.X > (float)(bounds.Left - 16) && base.Y > (float)(bounds.Top - 16) && base.X < (float)(bounds.Right + 16) && base.Y < (float)(bounds.Bottom + 16);
				Light.Alpha = Calc.Approach(Light.Alpha, (inview && !Level.Transitioning) ? 1 : 0, Engine.DeltaTime * 2f);
			}
			if (Sprite != null && Sprite.CurrentAnimationID == "usePhone")
			{
				if (PhoneTapSfx == null)
				{
					Add(PhoneTapSfx = new SoundSource());
				}
				if (!PhoneTapSfx.Playing)
				{
					PhoneTapSfx.Play("event:/char/theo/phone_taps_loop");
				}
			}
			else if (PhoneTapSfx != null && PhoneTapSfx.Playing)
			{
				PhoneTapSfx.Stop();
			}
		}

		public void SetupTheoSpriteSounds()
		{
			Sprite.OnFrameChange = delegate(string anim)
			{
				int currentAnimationFrame = Sprite.CurrentAnimationFrame;
				if ((anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim == "run" && (currentAnimationFrame == 0 || currentAnimationFrame == 4)))
				{
					Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitY, temp));
					if (platformByPriority != null)
					{
						Audio.Play("event:/char/madeline/footstep", base.Center, "surface_index", platformByPriority.GetStepSoundIndex(this));
					}
				}
				else if (anim == "crawl" && currentAnimationFrame == 0)
				{
					if (!Level.Transitioning)
					{
						Audio.Play("event:/char/theo/resort_crawl", Position);
					}
				}
				else if (anim == "pullVent" && currentAnimationFrame == 0)
				{
					Audio.Play("event:/char/theo/resort_vent_tug", Position);
				}
			};
		}

		public void SetupGrannySpriteSounds()
		{
			Sprite.OnFrameChange = delegate(string anim)
			{
				int currentAnimationFrame = Sprite.CurrentAnimationFrame;
				if (anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 4))
				{
					Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitY, temp));
					if (platformByPriority != null)
					{
						Audio.Play("event:/char/madeline/footstep", base.Center, "surface_index", platformByPriority.GetStepSoundIndex(this));
					}
				}
				else if (anim == "walk" && currentAnimationFrame == 2)
				{
					Audio.Play("event:/char/granny/cane_tap", Position);
				}
			};
		}

		public IEnumerator PlayerApproachRightSide(Player player, bool turnToFace = true, float? spacing = null)
		{
			yield return PlayerApproach(player, turnToFace, spacing, 1);
		}

		public IEnumerator PlayerApproachLeftSide(Player player, bool turnToFace = true, float? spacing = null)
		{
			yield return PlayerApproach(player, turnToFace, spacing, -1);
		}

		public IEnumerator PlayerApproach(Player player, bool turnToFace = true, float? spacing = null, int? side = null)
		{
			if (!side.HasValue)
			{
				side = Math.Sign(player.X - base.X);
			}
			if (side == 0)
			{
				side = 1;
			}
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			if (spacing.HasValue)
			{
				yield return player.DummyWalkToExact((int)(base.X + (float?)side * spacing).Value);
			}
			else if (Math.Abs(base.X - player.X) < 12f || Math.Sign(player.X - base.X) != side.Value)
			{
				yield return player.DummyWalkToExact((int)(base.X + (float?)(side * 12)).Value);
			}
			player.Facing = (Facings)(-side.Value);
			if (turnToFace && Sprite != null)
			{
				Sprite.Scale.X = side.Value;
			}
			yield return null;
		}

		public IEnumerator PlayerApproach48px()
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			yield return PlayerApproach(player, turnToFace: true, 48f);
		}

		public IEnumerator PlayerLeave(Player player, float? to = null)
		{
			if (to.HasValue)
			{
				yield return player.DummyWalkToExact((int)to.Value);
			}
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			yield return null;
		}

		public IEnumerator MoveTo(Vector2 target, bool fadeIn = false, int? turnAtEndTo = null, bool removeAtEnd = false)
		{
			if (removeAtEnd)
			{
				base.Tag |= Tags.TransitionUpdate;
			}
			if (Math.Sign(target.X - base.X) != 0 && Sprite != null)
			{
				Sprite.Scale.X = Math.Sign(target.X - base.X);
			}
			(target - Position).SafeNormalize();
			float alpha = (fadeIn ? 0f : 1f);
			if (Sprite != null && Sprite.Has(MoveAnim))
			{
				Sprite.Play(MoveAnim);
			}
			float speed = 0f;
			while ((MoveY && Position != target) || (!MoveY && base.X != target.X))
			{
				speed = Calc.Approach(speed, Maxspeed, 160f * Engine.DeltaTime);
				if (MoveY)
				{
					Position = Calc.Approach(Position, target, speed * Engine.DeltaTime);
				}
				else
				{
					base.X = Calc.Approach(base.X, target.X, speed * Engine.DeltaTime);
				}
				if (Sprite != null)
				{
					Sprite.Color = Color.White * alpha;
				}
				alpha = Calc.Approach(alpha, 1f, Engine.DeltaTime);
				yield return null;
			}
			if (Sprite != null && Sprite.Has(IdleAnim))
			{
				Sprite.Play(IdleAnim);
			}
			while (alpha < 1f)
			{
				if (Sprite != null)
				{
					Sprite.Color = Color.White * alpha;
				}
				alpha = Calc.Approach(alpha, 1f, Engine.DeltaTime);
				yield return null;
			}
			if (turnAtEndTo.HasValue && Sprite != null)
			{
				Sprite.Scale.X = turnAtEndTo.Value;
			}
			if (removeAtEnd)
			{
				base.Scene.Remove(this);
			}
			yield return null;
		}

		public void MoveToAndRemove(Vector2 target)
		{
			Add(new Coroutine(MoveTo(target, fadeIn: false, null, removeAtEnd: true)));
		}
	}
}
