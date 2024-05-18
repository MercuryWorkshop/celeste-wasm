using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CrushBlock : Solid
	{
		public enum Axes
		{
			Both,
			Horizontal,
			Vertical
		}

		private struct MoveState
		{
			public Vector2 From;

			public Vector2 Direction;

			public MoveState(Vector2 from, Vector2 direction)
			{
				From = from;
				Direction = direction;
			}
		}

		public static ParticleType P_Impact;

		public static ParticleType P_Crushing;

		public static ParticleType P_Activate;

		private const float CrushSpeed = 240f;

		private const float CrushAccel = 500f;

		private const float ReturnSpeed = 60f;

		private const float ReturnAccel = 160f;

		private Color fill = Calc.HexToColor("62222b");

		private Level level;

		private bool canActivate;

		private Vector2 crushDir;

		private List<MoveState> returnStack;

		private Coroutine attackCoroutine;

		private bool canMoveVertically;

		private bool canMoveHorizontally;

		private bool chillOut;

		private bool giant;

		private Sprite face;

		private string nextFaceDirection;

		private List<Image> idleImages = new List<Image>();

		private List<Image> activeTopImages = new List<Image>();

		private List<Image> activeRightImages = new List<Image>();

		private List<Image> activeLeftImages = new List<Image>();

		private List<Image> activeBottomImages = new List<Image>();

		private SoundSource currentMoveLoopSfx;

		private SoundSource returnLoopSfx;

		private bool Submerged => base.Scene.CollideCheck<Water>(new Rectangle((int)(base.Center.X - 4f), (int)base.Center.Y, 8, 4));

		public CrushBlock(Vector2 position, float width, float height, Axes axes, bool chillOut = false)
			: base(position, width, height, safe: false)
		{
			OnDashCollide = OnDashed;
			returnStack = new List<MoveState>();
			this.chillOut = chillOut;
			giant = base.Width >= 48f && base.Height >= 48f && chillOut;
			canActivate = true;
			attackCoroutine = new Coroutine();
			attackCoroutine.RemoveOnComplete = false;
			Add(attackCoroutine);
			List<MTexture> idles = GFX.Game.GetAtlasSubtextures("objects/crushblock/block");
			MTexture idle;
			switch (axes)
			{
			default:
				idle = idles[3];
				canMoveHorizontally = (canMoveVertically = true);
				break;
			case Axes.Horizontal:
				idle = idles[1];
				canMoveHorizontally = true;
				canMoveVertically = false;
				break;
			case Axes.Vertical:
				idle = idles[2];
				canMoveHorizontally = false;
				canMoveVertically = true;
				break;
			}
			Add(face = GFX.SpriteBank.Create(giant ? "giant_crushblock_face" : "crushblock_face"));
			face.Position = new Vector2(base.Width, base.Height) / 2f;
			face.Play("idle");
			face.OnLastFrame = delegate(string f)
			{
				if (f == "hit")
				{
					face.Play(nextFaceDirection);
				}
			};
			int c = (int)(base.Width / 8f) - 1;
			int r = (int)(base.Height / 8f) - 1;
			AddImage(idle, 0, 0, 0, 0, -1, -1);
			AddImage(idle, c, 0, 3, 0, 1, -1);
			AddImage(idle, 0, r, 0, 3, -1, 1);
			AddImage(idle, c, r, 3, 3, 1, 1);
			for (int x = 1; x < c; x++)
			{
				AddImage(idle, x, 0, Calc.Random.Choose(1, 2), 0, 0, -1);
				AddImage(idle, x, r, Calc.Random.Choose(1, 2), 3, 0, 1);
			}
			for (int y = 1; y < r; y++)
			{
				AddImage(idle, 0, y, 0, Calc.Random.Choose(1, 2), -1);
				AddImage(idle, c, y, 3, Calc.Random.Choose(1, 2), 1);
			}
			Add(new LightOcclude(0.2f));
			Add(returnLoopSfx = new SoundSource());
			Add(new WaterInteraction(() => crushDir != Vector2.Zero));
		}

		public CrushBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Enum("axes", Axes.Both), data.Bool("chillout"))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
		}

		public override void Update()
		{
			base.Update();
			if (crushDir == Vector2.Zero)
			{
				face.Position = new Vector2(base.Width, base.Height) / 2f;
				if (CollideCheck<Player>(Position + new Vector2(-1f, 0f)))
				{
					face.X -= 1f;
				}
				else if (CollideCheck<Player>(Position + new Vector2(1f, 0f)))
				{
					face.X += 1f;
				}
				else if (CollideCheck<Player>(Position + new Vector2(0f, -1f)))
				{
					face.Y -= 1f;
				}
			}
			if (currentMoveLoopSfx != null)
			{
				currentMoveLoopSfx.Param("submerged", Submerged ? 1 : 0);
			}
			if (returnLoopSfx != null)
			{
				returnLoopSfx.Param("submerged", Submerged ? 1 : 0);
			}
		}

		public override void Render()
		{
			Vector2 was = Position;
			Position += base.Shake;
			Draw.Rect(base.X + 2f, base.Y + 2f, base.Width - 4f, base.Height - 4f, fill);
			base.Render();
			Position = was;
		}

		private void AddImage(MTexture idle, int x, int y, int tx, int ty, int borderX = 0, int borderY = 0)
		{
			MTexture idleSubtex = idle.GetSubtexture(tx * 8, ty * 8, 8, 8);
			Vector2 pos = new Vector2(x * 8, y * 8);
			if (borderX != 0)
			{
				Image b2 = new Image(idleSubtex);
				b2.Color = Color.Black;
				b2.Position = pos + new Vector2(borderX, 0f);
				Add(b2);
			}
			if (borderY != 0)
			{
				Image b = new Image(idleSubtex);
				b.Color = Color.Black;
				b.Position = pos + new Vector2(0f, borderY);
				Add(b);
			}
			Image img = new Image(idleSubtex);
			img.Position = pos;
			Add(img);
			idleImages.Add(img);
			if (borderX != 0 || borderY != 0)
			{
				if (borderX < 0)
				{
					Image active4 = new Image(GFX.Game["objects/crushblock/lit_left"].GetSubtexture(0, ty * 8, 8, 8));
					activeLeftImages.Add(active4);
					active4.Position = pos;
					active4.Visible = false;
					Add(active4);
				}
				else if (borderX > 0)
				{
					Image active3 = new Image(GFX.Game["objects/crushblock/lit_right"].GetSubtexture(0, ty * 8, 8, 8));
					activeRightImages.Add(active3);
					active3.Position = pos;
					active3.Visible = false;
					Add(active3);
				}
				if (borderY < 0)
				{
					Image active2 = new Image(GFX.Game["objects/crushblock/lit_top"].GetSubtexture(tx * 8, 0, 8, 8));
					activeTopImages.Add(active2);
					active2.Position = pos;
					active2.Visible = false;
					Add(active2);
				}
				else if (borderY > 0)
				{
					Image active = new Image(GFX.Game["objects/crushblock/lit_bottom"].GetSubtexture(tx * 8, 0, 8, 8));
					activeBottomImages.Add(active);
					active.Position = pos;
					active.Visible = false;
					Add(active);
				}
			}
		}

		private void TurnOffImages()
		{
			foreach (Image activeLeftImage in activeLeftImages)
			{
				activeLeftImage.Visible = false;
			}
			foreach (Image activeRightImage in activeRightImages)
			{
				activeRightImage.Visible = false;
			}
			foreach (Image activeTopImage in activeTopImages)
			{
				activeTopImage.Visible = false;
			}
			foreach (Image activeBottomImage in activeBottomImages)
			{
				activeBottomImage.Visible = false;
			}
		}

		private DashCollisionResults OnDashed(Player player, Vector2 direction)
		{
			if (CanActivate(-direction))
			{
				Attack(-direction);
				return DashCollisionResults.Rebound;
			}
			return DashCollisionResults.NormalCollision;
		}

		private bool CanActivate(Vector2 direction)
		{
			if (giant && direction.X <= 0f)
			{
				return false;
			}
			if (canActivate && crushDir != direction)
			{
				if (direction.X != 0f && !canMoveHorizontally)
				{
					return false;
				}
				if (direction.Y != 0f && !canMoveVertically)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		private void Attack(Vector2 direction)
		{
			Audio.Play("event:/game/06_reflection/crushblock_activate", base.Center);
			if (currentMoveLoopSfx != null)
			{
				currentMoveLoopSfx.Param("end", 1f);
				SoundSource sfx = currentMoveLoopSfx;
				Alarm.Set(this, 0.5f, delegate
				{
					sfx.RemoveSelf();
				});
			}
			Add(currentMoveLoopSfx = new SoundSource());
			currentMoveLoopSfx.Position = new Vector2(base.Width, base.Height) / 2f;
			if (SaveData.Instance != null && SaveData.Instance.Name != null && SaveData.Instance.Name.StartsWith("FWAHAHA", StringComparison.InvariantCultureIgnoreCase))
			{
				currentMoveLoopSfx.Play("event:/game/06_reflection/crushblock_move_loop_covert");
			}
			else
			{
				currentMoveLoopSfx.Play("event:/game/06_reflection/crushblock_move_loop");
			}
			face.Play("hit");
			crushDir = direction;
			canActivate = false;
			attackCoroutine.Replace(AttackSequence());
			ClearRemainder();
			TurnOffImages();
			ActivateParticles(crushDir);
			if (crushDir.X < 0f)
			{
				foreach (Image activeLeftImage in activeLeftImages)
				{
					activeLeftImage.Visible = true;
				}
				nextFaceDirection = "left";
			}
			else if (crushDir.X > 0f)
			{
				foreach (Image activeRightImage in activeRightImages)
				{
					activeRightImage.Visible = true;
				}
				nextFaceDirection = "right";
			}
			else if (crushDir.Y < 0f)
			{
				foreach (Image activeTopImage in activeTopImages)
				{
					activeTopImage.Visible = true;
				}
				nextFaceDirection = "up";
			}
			else if (crushDir.Y > 0f)
			{
				foreach (Image activeBottomImage in activeBottomImages)
				{
					activeBottomImage.Visible = true;
				}
				nextFaceDirection = "down";
			}
			bool addState = true;
			if (returnStack.Count > 0)
			{
				MoveState last = returnStack[returnStack.Count - 1];
				if (last.Direction == direction || last.Direction == -direction)
				{
					addState = false;
				}
			}
			if (addState)
			{
				returnStack.Add(new MoveState(Position, crushDir));
			}
		}

		private void ActivateParticles(Vector2 dir)
		{
			float angle;
			Vector2 at;
			Vector2 range;
			int amount;
			if (dir == Vector2.UnitX)
			{
				angle = 0f;
				at = base.CenterRight - Vector2.UnitX;
				range = Vector2.UnitY * (base.Height - 2f) * 0.5f;
				amount = (int)(base.Height / 8f) * 4;
			}
			else if (dir == -Vector2.UnitX)
			{
				angle = (float)Math.PI;
				at = base.CenterLeft + Vector2.UnitX;
				range = Vector2.UnitY * (base.Height - 2f) * 0.5f;
				amount = (int)(base.Height / 8f) * 4;
			}
			else if (dir == Vector2.UnitY)
			{
				angle = (float)Math.PI / 2f;
				at = base.BottomCenter - Vector2.UnitY;
				range = Vector2.UnitX * (base.Width - 2f) * 0.5f;
				amount = (int)(base.Width / 8f) * 4;
			}
			else
			{
				angle = -(float)Math.PI / 2f;
				at = base.TopCenter + Vector2.UnitY;
				range = Vector2.UnitX * (base.Width - 2f) * 0.5f;
				amount = (int)(base.Width / 8f) * 4;
			}
			amount += 2;
			level.Particles.Emit(P_Activate, amount, at, range, angle);
		}

		private IEnumerator AttackSequence()
		{
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			StartShaking(0.4f);
			yield return 0.4f;
			if (!chillOut)
			{
				canActivate = true;
			}
			StopPlayerRunIntoAnimation = false;
			bool slowing = false;
			float speed = 0f;
			while (true)
			{
				if (!chillOut)
				{
					speed = Calc.Approach(speed, 240f, 500f * Engine.DeltaTime);
				}
				else if (slowing || CollideCheck<SolidTiles>(Position + crushDir * 256f))
				{
					speed = Calc.Approach(speed, 24f, 500f * Engine.DeltaTime * 0.25f);
					if (!slowing)
					{
						slowing = true;
						Alarm.Set(this, 0.5f, delegate
						{
							face.Play("hurt");
							currentMoveLoopSfx.Stop();
							TurnOffImages();
						});
					}
				}
				else
				{
					speed = Calc.Approach(speed, 240f, 500f * Engine.DeltaTime);
				}
				bool hit = ((crushDir.X == 0f) ? MoveVCheck(speed * crushDir.Y * Engine.DeltaTime) : MoveHCheck(speed * crushDir.X * Engine.DeltaTime));
				if (base.Top >= (float)(level.Bounds.Bottom + 32))
				{
					RemoveSelf();
					yield break;
				}
				if (hit)
				{
					break;
				}
				if (base.Scene.OnInterval(0.02f))
				{
					Vector2 at;
					float dir;
					if (crushDir == Vector2.UnitX)
					{
						at = new Vector2(base.Left + 1f, Calc.Random.Range(base.Top + 3f, base.Bottom - 3f));
						dir = (float)Math.PI;
					}
					else if (crushDir == -Vector2.UnitX)
					{
						at = new Vector2(base.Right - 1f, Calc.Random.Range(base.Top + 3f, base.Bottom - 3f));
						dir = 0f;
					}
					else if (crushDir == Vector2.UnitY)
					{
						at = new Vector2(Calc.Random.Range(base.Left + 3f, base.Right - 3f), base.Top + 1f);
						dir = -(float)Math.PI / 2f;
					}
					else
					{
						at = new Vector2(Calc.Random.Range(base.Left + 3f, base.Right - 3f), base.Bottom - 1f);
						dir = (float)Math.PI / 2f;
					}
					level.Particles.Emit(P_Crushing, at, dir);
				}
				yield return null;
			}
			FallingBlock fallingBlock = CollideFirst<FallingBlock>(Position + crushDir);
			if (fallingBlock != null)
			{
				fallingBlock.Triggered = true;
			}
			if (crushDir == -Vector2.UnitX)
			{
				Vector2 add4 = new Vector2(0f, 2f);
				for (int l = 0; (float)l < base.Height / 8f; l++)
				{
					Vector2 at5 = new Vector2(base.Left - 1f, base.Top + 4f + (float)(l * 8));
					if (!base.Scene.CollideCheck<Water>(at5) && base.Scene.CollideCheck<Solid>(at5))
					{
						SceneAs<Level>().ParticlesFG.Emit(P_Impact, at5 + add4, 0f);
						SceneAs<Level>().ParticlesFG.Emit(P_Impact, at5 - add4, 0f);
					}
				}
			}
			else if (crushDir == Vector2.UnitX)
			{
				Vector2 add3 = new Vector2(0f, 2f);
				for (int k = 0; (float)k < base.Height / 8f; k++)
				{
					Vector2 at4 = new Vector2(base.Right + 1f, base.Top + 4f + (float)(k * 8));
					if (!base.Scene.CollideCheck<Water>(at4) && base.Scene.CollideCheck<Solid>(at4))
					{
						SceneAs<Level>().ParticlesFG.Emit(P_Impact, at4 + add3, (float)Math.PI);
						SceneAs<Level>().ParticlesFG.Emit(P_Impact, at4 - add3, (float)Math.PI);
					}
				}
			}
			else if (crushDir == -Vector2.UnitY)
			{
				Vector2 add2 = new Vector2(2f, 0f);
				for (int j = 0; (float)j < base.Width / 8f; j++)
				{
					Vector2 at3 = new Vector2(base.Left + 4f + (float)(j * 8), base.Top - 1f);
					if (!base.Scene.CollideCheck<Water>(at3) && base.Scene.CollideCheck<Solid>(at3))
					{
						SceneAs<Level>().ParticlesFG.Emit(P_Impact, at3 + add2, (float)Math.PI / 2f);
						SceneAs<Level>().ParticlesFG.Emit(P_Impact, at3 - add2, (float)Math.PI / 2f);
					}
				}
			}
			else if (crushDir == Vector2.UnitY)
			{
				Vector2 add = new Vector2(2f, 0f);
				for (int i = 0; (float)i < base.Width / 8f; i++)
				{
					Vector2 at2 = new Vector2(base.Left + 4f + (float)(i * 8), base.Bottom + 1f);
					if (!base.Scene.CollideCheck<Water>(at2) && base.Scene.CollideCheck<Solid>(at2))
					{
						SceneAs<Level>().ParticlesFG.Emit(P_Impact, at2 + add, -(float)Math.PI / 2f);
						SceneAs<Level>().ParticlesFG.Emit(P_Impact, at2 - add, -(float)Math.PI / 2f);
					}
				}
			}
			Audio.Play("event:/game/06_reflection/crushblock_impact", base.Center);
			level.DirectionalShake(crushDir);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			StartShaking(0.4f);
			StopPlayerRunIntoAnimation = true;
			SoundSource sfx = currentMoveLoopSfx;
			currentMoveLoopSfx.Param("end", 1f);
			currentMoveLoopSfx = null;
			Alarm.Set(this, 0.5f, delegate
			{
				sfx.RemoveSelf();
			});
			crushDir = Vector2.Zero;
			TurnOffImages();
			if (chillOut)
			{
				yield break;
			}
			face.Play("hurt");
			returnLoopSfx.Play("event:/game/06_reflection/crushblock_return_loop");
			yield return 0.4f;
			speed = 0f;
			float waypointSfxDelay = 0f;
			while (returnStack.Count > 0)
			{
				yield return null;
				StopPlayerRunIntoAnimation = false;
				MoveState ret = returnStack[returnStack.Count - 1];
				speed = Calc.Approach(speed, 60f, 160f * Engine.DeltaTime);
				waypointSfxDelay -= Engine.DeltaTime;
				if (ret.Direction.X != 0f)
				{
					MoveTowardsX(ret.From.X, speed * Engine.DeltaTime);
				}
				if (ret.Direction.Y != 0f)
				{
					MoveTowardsY(ret.From.Y, speed * Engine.DeltaTime);
				}
				if ((ret.Direction.X != 0f && base.ExactPosition.X != ret.From.X) || (ret.Direction.Y != 0f && base.ExactPosition.Y != ret.From.Y))
				{
					continue;
				}
				speed = 0f;
				returnStack.RemoveAt(returnStack.Count - 1);
				StopPlayerRunIntoAnimation = true;
				if (returnStack.Count <= 0)
				{
					face.Play("idle");
					returnLoopSfx.Stop();
					if (waypointSfxDelay <= 0f)
					{
						Audio.Play("event:/game/06_reflection/crushblock_rest", base.Center);
					}
				}
				else if (waypointSfxDelay <= 0f)
				{
					Audio.Play("event:/game/06_reflection/crushblock_rest_waypoint", base.Center);
				}
				waypointSfxDelay = 0.1f;
				StartShaking(0.2f);
				yield return 0.2f;
			}
		}

		private bool MoveHCheck(float amount)
		{
			if (MoveHCollideSolidsAndBounds(level, amount, thruDashBlocks: true))
			{
				if (amount < 0f && base.Left <= (float)level.Bounds.Left)
				{
					return true;
				}
				if (amount > 0f && base.Right >= (float)level.Bounds.Right)
				{
					return true;
				}
				for (int i = 1; i <= 4; i++)
				{
					for (int s = 1; s >= -1; s -= 2)
					{
						Vector2 add = new Vector2(Math.Sign(amount), i * s);
						if (!CollideCheck<Solid>(Position + add))
						{
							MoveVExact(i * s);
							MoveHExact(Math.Sign(amount));
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		private bool MoveVCheck(float amount)
		{
			if (MoveVCollideSolidsAndBounds(level, amount, thruDashBlocks: true, null, checkBottom: false))
			{
				if (amount < 0f && base.Top <= (float)level.Bounds.Top)
				{
					return true;
				}
				for (int i = 1; i <= 4; i++)
				{
					for (int s = 1; s >= -1; s -= 2)
					{
						Vector2 add = new Vector2(i * s, Math.Sign(amount));
						if (!CollideCheck<Solid>(Position + add))
						{
							MoveHExact(i * s);
							MoveVExact(Math.Sign(amount));
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}
	}
}
