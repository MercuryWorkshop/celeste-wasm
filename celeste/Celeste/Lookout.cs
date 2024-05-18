using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class Lookout : Entity
	{
		private class Hud : Entity
		{
			public bool TrackMode;

			public float TrackPercent;

			public bool OnlyY;

			public float Easer;

			private float timerUp;

			private float timerDown;

			private float timerLeft;

			private float timerRight;

			private float multUp;

			private float multDown;

			private float multLeft;

			private float multRight;

			private float left;

			private float right;

			private float up;

			private float down;

			private Vector2 aim;

			private MTexture halfDot = GFX.Gui["dot"].GetSubtexture(0, 0, 64, 32);

			public Hud()
			{
				AddTag(Tags.HUD);
			}

			public override void Update()
			{
				Level level = SceneAs<Level>();
				Vector2 cam = level.Camera.Position;
				Rectangle bnd = level.Bounds;
				int w = 320;
				int h = 180;
				bool hitLeft = base.Scene.CollideCheck<LookoutBlocker>(new Rectangle((int)(cam.X - 8f), (int)cam.Y, w, h));
				bool hitRight = base.Scene.CollideCheck<LookoutBlocker>(new Rectangle((int)(cam.X + 8f), (int)cam.Y, w, h));
				bool hitUp = (TrackMode && TrackPercent >= 1f) || base.Scene.CollideCheck<LookoutBlocker>(new Rectangle((int)cam.X, (int)(cam.Y - 8f), w, h));
				bool hitDown = (TrackMode && TrackPercent <= 0f) || base.Scene.CollideCheck<LookoutBlocker>(new Rectangle((int)cam.X, (int)(cam.Y + 8f), w, h));
				left = Calc.Approach(left, (!hitLeft && cam.X > (float)(bnd.Left + 2)) ? 1 : 0, Engine.DeltaTime * 8f);
				right = Calc.Approach(right, (!hitRight && cam.X + (float)w < (float)(bnd.Right - 2)) ? 1 : 0, Engine.DeltaTime * 8f);
				up = Calc.Approach(up, (!hitUp && cam.Y > (float)(bnd.Top + 2)) ? 1 : 0, Engine.DeltaTime * 8f);
				down = Calc.Approach(down, (!hitDown && cam.Y + (float)h < (float)(bnd.Bottom - 2)) ? 1 : 0, Engine.DeltaTime * 8f);
				aim = Input.Aim.Value;
				if (aim.X < 0f)
				{
					multLeft = Calc.Approach(multLeft, 0f, Engine.DeltaTime * 2f);
					timerLeft += Engine.DeltaTime * 12f;
				}
				else
				{
					multLeft = Calc.Approach(multLeft, 1f, Engine.DeltaTime * 2f);
					timerLeft += Engine.DeltaTime * 6f;
				}
				if (aim.X > 0f)
				{
					multRight = Calc.Approach(multRight, 0f, Engine.DeltaTime * 2f);
					timerRight += Engine.DeltaTime * 12f;
				}
				else
				{
					multRight = Calc.Approach(multRight, 1f, Engine.DeltaTime * 2f);
					timerRight += Engine.DeltaTime * 6f;
				}
				if (aim.Y < 0f)
				{
					multUp = Calc.Approach(multUp, 0f, Engine.DeltaTime * 2f);
					timerUp += Engine.DeltaTime * 12f;
				}
				else
				{
					multUp = Calc.Approach(multUp, 1f, Engine.DeltaTime * 2f);
					timerUp += Engine.DeltaTime * 6f;
				}
				if (aim.Y > 0f)
				{
					multDown = Calc.Approach(multDown, 0f, Engine.DeltaTime * 2f);
					timerDown += Engine.DeltaTime * 12f;
				}
				else
				{
					multDown = Calc.Approach(multDown, 1f, Engine.DeltaTime * 2f);
					timerDown += Engine.DeltaTime * 6f;
				}
				base.Update();
			}

			public override void Render()
			{
				Level level = base.Scene as Level;
				float tween = Ease.CubeInOut(Easer);
				Color border = Color.White * tween;
				int sizeX = (int)(80f * tween);
				int sizeY = (int)(80f * tween * 0.5625f);
				int stroke = 8;
				if (level.FrozenOrPaused || level.RetryPlayerCorpse != null)
				{
					border *= 0.25f;
				}
				Draw.Rect(sizeX, sizeY, 1920 - sizeX * 2 - stroke, stroke, border);
				Draw.Rect(sizeX, sizeY + stroke, stroke + 2, 1080 - sizeY * 2 - stroke, border);
				Draw.Rect(1920 - sizeX - stroke - 2, sizeY, stroke + 2, 1080 - sizeY * 2 - stroke, border);
				Draw.Rect(sizeX + stroke, 1080 - sizeY - stroke, 1920 - sizeX * 2 - stroke, stroke, border);
				if (level.FrozenOrPaused || level.RetryPlayerCorpse != null)
				{
					return;
				}
				MTexture arrow = GFX.Gui["towerarrow"];
				float y3 = (float)sizeY * up - (float)(Math.Sin(timerUp) * 18.0 * (double)MathHelper.Lerp(0.5f, 1f, multUp)) - (1f - multUp) * 12f;
				arrow.DrawCentered(new Vector2(960f, y3), border * up, 1f, (float)Math.PI / 2f);
				float y2 = 1080f - (float)sizeY * down + (float)(Math.Sin(timerDown) * 18.0 * (double)MathHelper.Lerp(0.5f, 1f, multDown)) + (1f - multDown) * 12f;
				arrow.DrawCentered(new Vector2(960f, y2), border * down, 1f, 4.712389f);
				if (!TrackMode && !OnlyY)
				{
					float i = left;
					float ml = multLeft;
					float tl = timerLeft;
					float r = right;
					float mr = multRight;
					float tr = timerRight;
					if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
					{
						i = right;
						ml = multRight;
						tl = timerRight;
						r = left;
						mr = multLeft;
						tr = timerLeft;
					}
					float x3 = (float)sizeX * i - (float)(Math.Sin(tl) * 18.0 * (double)MathHelper.Lerp(0.5f, 1f, ml)) - (1f - ml) * 12f;
					arrow.DrawCentered(new Vector2(x3, 540f), border * i);
					float x2 = 1920f - (float)sizeX * r + (float)(Math.Sin(tr) * 18.0 * (double)MathHelper.Lerp(0.5f, 1f, mr)) + (1f - mr) * 12f;
					arrow.DrawCentered(new Vector2(x2, 540f), border * r, 1f, (float)Math.PI);
				}
				else if (TrackMode)
				{
					int h = 1080 - sizeY * 2 - 128 - 64;
					int x = 1920 - sizeX - 64;
					float y = (float)(1080 - h) / 2f + 32f;
					Draw.Rect(x - 7, y + 7f, 14f, h - 14, Color.Black * tween);
					halfDot.DrawJustified(new Vector2(x, y + 7f), new Vector2(0.5f, 1f), Color.Black * tween);
					halfDot.DrawJustified(new Vector2(x, y + (float)h - 7f), new Vector2(0.5f, 1f), Color.Black * tween, new Vector2(1f, -1f));
					GFX.Gui["lookout/cursor"].DrawCentered(new Vector2(x, y + (1f - TrackPercent) * (float)h), Color.White * tween, 1f);
					GFX.Gui["lookout/summit"].DrawCentered(new Vector2(x, y - 64f), Color.White * tween, 0.65f);
				}
			}
		}

		private TalkComponent talk;

		private Hud hud;

		private Sprite sprite;

		private Tween lightTween;

		private bool interacting;

		private bool onlyY;

		private List<Vector2> nodes;

		private int node;

		private float nodePercent;

		private bool summit;

		private string animPrefix = "";

		public Lookout(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Depth = -8500;
			Add(talk = new TalkComponent(new Rectangle(-24, -8, 48, 8), new Vector2(-0.5f, -20f), Interact));
			talk.PlayerMustBeFacing = false;
			summit = data.Bool("summit");
			onlyY = data.Bool("onlyY");
			base.Collider = new Hitbox(4f, 4f, -2f, -4f);
			VertexLight light = new VertexLight(new Vector2(-1f, -11f), Color.White, 0.8f, 16, 24);
			Add(light);
			lightTween = light.CreatePulseTween();
			Add(lightTween);
			Add(sprite = GFX.SpriteBank.Create("lookout"));
			sprite.OnFrameChange = delegate(string s)
			{
				switch (s)
				{
				case "idle":
				case "badeline_idle":
				case "nobackpack_idle":
					if (sprite.CurrentAnimationFrame == sprite.CurrentAnimationTotalFrames - 1)
					{
						lightTween.Start();
					}
					break;
				}
			};
			Vector2[] nodes = data.NodesOffset(offset);
			if (nodes != null && nodes.Length != 0)
			{
				this.nodes = new List<Vector2>(nodes);
			}
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			if (interacting)
			{
				Player player = scene.Tracker.GetEntity<Player>();
				if (player != null)
				{
					player.StateMachine.State = 0;
				}
			}
		}

		private void Interact(Player player)
		{
			if (player.DefaultSpriteMode == PlayerSpriteMode.MadelineAsBadeline || SaveData.Instance.Assists.PlayAsBadeline)
			{
				animPrefix = "badeline_";
			}
			else if (player.DefaultSpriteMode == PlayerSpriteMode.MadelineNoBackpack)
			{
				animPrefix = "nobackpack_";
			}
			else
			{
				animPrefix = "";
			}
			Coroutine routine = new Coroutine(LookRoutine(player));
			routine.RemoveOnComplete = true;
			Add(routine);
			interacting = true;
		}

		public void StopInteracting()
		{
			interacting = false;
			sprite.Play(animPrefix + "idle");
		}

		public override void Update()
		{
			base.Update();
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				sprite.Active = interacting || player.StateMachine.State != 11;
				if (!sprite.Active)
				{
					sprite.SetAnimationFrame(0);
				}
			}
			if (talk != null && CollideCheck<Solid>())
			{
				Remove(talk);
				talk = null;
			}
		}

		private IEnumerator LookRoutine(Player player)
		{
			Level level = SceneAs<Level>();
			SandwichLava lava = base.Scene.Entities.FindFirst<SandwichLava>();
			if (lava != null)
			{
				lava.Waiting = true;
			}
			if (player.Holding != null)
			{
				player.Drop();
			}
			player.StateMachine.State = 11;
			yield return player.DummyWalkToExact((int)base.X, walkBackwards: false, 1f, cancelOnFall: true);
			if (Math.Abs(base.X - player.X) > 4f || player.Dead || !player.OnGround())
			{
				if (!player.Dead)
				{
					player.StateMachine.State = 0;
				}
				yield break;
			}
			Audio.Play("event:/game/general/lookout_use", Position);
			if (player.Facing == Facings.Right)
			{
				sprite.Play(animPrefix + "lookRight");
			}
			else
			{
				sprite.Play(animPrefix + "lookLeft");
			}
			player.Sprite.Visible = (player.Hair.Visible = false);
			yield return 0.2f;
			base.Scene.Add(hud = new Hud());
			hud.TrackMode = nodes != null;
			hud.OnlyY = onlyY;
			nodePercent = 0f;
			node = 0;
			Audio.Play("event:/ui/game/lookout_on");
			while ((hud.Easer = Calc.Approach(hud.Easer, 1f, Engine.DeltaTime * 3f)) < 1f)
			{
				level.ScreenPadding = (int)(Ease.CubeInOut(hud.Easer) * 16f);
				yield return null;
			}
			float accel = 800f;
			float maxspd = 240f;
			Vector2 cam = level.Camera.Position;
			Vector2 speed = Vector2.Zero;
			Vector2 lastDir = Vector2.Zero;
			Vector2 camStart = level.Camera.Position;
			Vector2 camStartCenter = camStart + new Vector2(160f, 90f);
			while (!Input.MenuCancel.Pressed && !Input.MenuConfirm.Pressed && !Input.Dash.Pressed && !Input.Jump.Pressed && interacting)
			{
				Vector2 dir = Input.Aim.Value;
				if (onlyY)
				{
					dir.X = 0f;
				}
				if (Math.Sign(dir.X) != Math.Sign(lastDir.X) || Math.Sign(dir.Y) != Math.Sign(lastDir.Y))
				{
					Audio.Play("event:/game/general/lookout_move", Position);
				}
				lastDir = dir;
				if (sprite.CurrentAnimationID != "lookLeft" && sprite.CurrentAnimationID != "lookRight")
				{
					if (dir.X == 0f)
					{
						if (dir.Y == 0f)
						{
							sprite.Play(animPrefix + "looking");
						}
						else if (dir.Y > 0f)
						{
							sprite.Play(animPrefix + "lookingDown");
						}
						else
						{
							sprite.Play(animPrefix + "lookingUp");
						}
					}
					else if (dir.X > 0f)
					{
						if (dir.Y == 0f)
						{
							sprite.Play(animPrefix + "lookingRight");
						}
						else if (dir.Y > 0f)
						{
							sprite.Play(animPrefix + "lookingDownRight");
						}
						else
						{
							sprite.Play(animPrefix + "lookingUpRight");
						}
					}
					else if (dir.X < 0f)
					{
						if (dir.Y == 0f)
						{
							sprite.Play(animPrefix + "lookingLeft");
						}
						else if (dir.Y > 0f)
						{
							sprite.Play(animPrefix + "lookingDownLeft");
						}
						else
						{
							sprite.Play(animPrefix + "lookingUpLeft");
						}
					}
				}
				if (nodes == null)
				{
					speed += accel * dir * Engine.DeltaTime;
					if (dir.X == 0f)
					{
						speed.X = Calc.Approach(speed.X, 0f, accel * 2f * Engine.DeltaTime);
					}
					if (dir.Y == 0f)
					{
						speed.Y = Calc.Approach(speed.Y, 0f, accel * 2f * Engine.DeltaTime);
					}
					if (speed.Length() > maxspd)
					{
						speed = speed.SafeNormalize(maxspd);
					}
					Vector2 last = cam;
					List<Entity> blockers = base.Scene.Tracker.GetEntities<LookoutBlocker>();
					cam.X += speed.X * Engine.DeltaTime;
					if (cam.X < (float)level.Bounds.Left || cam.X + 320f > (float)level.Bounds.Right)
					{
						speed.X = 0f;
					}
					cam.X = Calc.Clamp(cam.X, level.Bounds.Left, level.Bounds.Right - 320);
					foreach (Entity blocker2 in blockers)
					{
						if (cam.X + 320f > blocker2.Left && cam.Y + 180f > blocker2.Top && cam.X < blocker2.Right && cam.Y < blocker2.Bottom)
						{
							cam.X = last.X;
							speed.X = 0f;
						}
					}
					cam.Y += speed.Y * Engine.DeltaTime;
					if (cam.Y < (float)level.Bounds.Top || cam.Y + 180f > (float)level.Bounds.Bottom)
					{
						speed.Y = 0f;
					}
					cam.Y = Calc.Clamp(cam.Y, level.Bounds.Top, level.Bounds.Bottom - 180);
					foreach (Entity blocker in blockers)
					{
						if (cam.X + 320f > blocker.Left && cam.Y + 180f > blocker.Top && cam.X < blocker.Right && cam.Y < blocker.Bottom)
						{
							cam.Y = last.Y;
							speed.Y = 0f;
						}
					}
					level.Camera.Position = cam;
				}
				else
				{
					Vector2 from = ((node <= 0) ? camStartCenter : nodes[node - 1]);
					Vector2 to = nodes[node];
					float dist = (from - to).Length();
					(to - from).SafeNormalize();
					if (nodePercent < 0.25f && node > 0)
					{
						Vector2 curveStart2 = Vector2.Lerp((node <= 1) ? camStartCenter : nodes[node - 2], from, 0.75f);
						Vector2 curveEnd2 = Vector2.Lerp(from, to, 0.25f);
						SimpleCurve curve2 = new SimpleCurve(curveStart2, curveEnd2, from);
						level.Camera.Position = curve2.GetPoint(0.5f + nodePercent / 0.25f * 0.5f);
					}
					else if (nodePercent > 0.75f && node < nodes.Count - 1)
					{
						Vector2 next = nodes[node + 1];
						Vector2 curveStart = Vector2.Lerp(from, to, 0.75f);
						Vector2 curveEnd = Vector2.Lerp(to, next, 0.25f);
						SimpleCurve curve = new SimpleCurve(curveStart, curveEnd, to);
						level.Camera.Position = curve.GetPoint((nodePercent - 0.75f) / 0.25f * 0.5f);
					}
					else
					{
						level.Camera.Position = Vector2.Lerp(from, to, nodePercent);
					}
					level.Camera.Position += new Vector2(-160f, -90f);
					nodePercent -= dir.Y * (maxspd / dist) * Engine.DeltaTime;
					if (nodePercent < 0f)
					{
						if (node > 0)
						{
							node--;
							nodePercent = 1f;
						}
						else
						{
							nodePercent = 0f;
						}
					}
					else if (nodePercent > 1f)
					{
						if (node < nodes.Count - 1)
						{
							node++;
							nodePercent = 0f;
						}
						else
						{
							nodePercent = 1f;
							if (summit)
							{
								break;
							}
						}
					}
					float currentDist = 0f;
					float totalDist = 0f;
					for (int i = 0; i < nodes.Count; i++)
					{
						float d = (((i == 0) ? camStartCenter : nodes[i - 1]) - nodes[i]).Length();
						totalDist += d;
						if (i < node)
						{
							currentDist += d;
						}
						else if (i == node)
						{
							currentDist += d * nodePercent;
						}
					}
					hud.TrackPercent = currentDist / totalDist;
				}
				yield return null;
			}
			player.Sprite.Visible = (player.Hair.Visible = true);
			sprite.Play(animPrefix + "idle");
			Audio.Play("event:/ui/game/lookout_off");
			while ((hud.Easer = Calc.Approach(hud.Easer, 0f, Engine.DeltaTime * 3f)) > 0f)
			{
				level.ScreenPadding = (int)(Ease.CubeInOut(hud.Easer) * 16f);
				yield return null;
			}
			bool atSummitTop = summit && node >= nodes.Count - 1 && nodePercent >= 0.95f;
			if (atSummitTop)
			{
				yield return 0.5f;
				float duration2 = 3f;
				float approach2 = 0f;
				Coroutine routine = new Coroutine(level.ZoomTo(new Vector2(160f, 90f), 2f, duration2));
				Add(routine);
				while (!Input.MenuCancel.Pressed && !Input.MenuConfirm.Pressed && !Input.Dash.Pressed && !Input.Jump.Pressed && interacting)
				{
					approach2 = Calc.Approach(approach2, 1f, Engine.DeltaTime / duration2);
					Audio.SetMusicParam("escape", approach2);
					yield return null;
				}
			}
			if ((camStart - level.Camera.Position).Length() > 600f)
			{
				Vector2 was = level.Camera.Position;
				Vector2 direction = (was - camStart).SafeNormalize();
				float approach2 = (atSummitTop ? 1f : 0.5f);
				new FadeWipe(base.Scene, wipeIn: false).Duration = approach2;
				for (float duration2 = 0f; duration2 < 1f; duration2 += Engine.DeltaTime / approach2)
				{
					level.Camera.Position = was - direction * MathHelper.Lerp(0f, 64f, Ease.CubeIn(duration2));
					yield return null;
				}
				level.Camera.Position = camStart + direction * 32f;
				new FadeWipe(base.Scene, wipeIn: true);
			}
			Audio.SetMusicParam("escape", 0f);
			level.ScreenPadding = 0f;
			level.ZoomSnap(Vector2.Zero, 1f);
			base.Scene.Remove(hud);
			interacting = false;
			player.StateMachine.State = 0;
			yield return null;
		}
	}
}
