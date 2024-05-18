using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BadelineDummy : Entity
	{
		public PlayerSprite Sprite;

		public PlayerHair Hair;

		public BadelineAutoAnimator AutoAnimator;

		public SineWave Wave;

		public VertexLight Light;

		public float FloatSpeed = 120f;

		public float FloatAccel = 240f;

		public float Floatness = 2f;

		public Vector2 floatNormal = new Vector2(0f, 1f);

		public BadelineDummy(Vector2 position)
			: base(position)
		{
			base.Collider = new Hitbox(6f, 6f, -3f, -7f);
			Sprite = new PlayerSprite(PlayerSpriteMode.Badeline);
			Sprite.Play("fallSlow");
			Sprite.Scale.X = -1f;
			Hair = new PlayerHair(Sprite);
			Hair.Color = BadelineOldsite.HairColor;
			Hair.Border = Color.Black;
			Hair.Facing = Facings.Left;
			Add(Hair);
			Add(Sprite);
			Add(AutoAnimator = new BadelineAutoAnimator());
			Sprite.OnFrameChange = delegate(string anim)
			{
				int currentAnimationFrame = Sprite.CurrentAnimationFrame;
				if ((anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim == "runSlow" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim == "runFast" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)))
				{
					Audio.Play("event:/char/badeline/footstep", Position);
				}
			};
			Add(Wave = new SineWave(0.25f));
			Wave.OnUpdate = delegate(float f)
			{
				Sprite.Position = floatNormal * f * Floatness;
			};
			Add(Light = new VertexLight(new Vector2(0f, -8f), Color.PaleVioletRed, 1f, 20, 60));
		}

		public void Appear(Level level, bool silent = false)
		{
			if (!silent)
			{
				Audio.Play("event:/char/badeline/appear", Position);
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			}
			level.Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
			level.Particles.Emit(BadelineOldsite.P_Vanish, 12, base.Center, Vector2.One * 6f);
		}

		public void Vanish()
		{
			Audio.Play("event:/char/badeline/disappear", Position);
			Shockwave();
			SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, base.Center, Vector2.One * 6f);
			RemoveSelf();
		}

		private void Shockwave()
		{
			SceneAs<Level>().Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
		}

		public IEnumerator FloatTo(Vector2 target, int? turnAtEndTo = null, bool faceDirection = true, bool fadeLight = false, bool quickEnd = false)
		{
			Sprite.Play("fallSlow");
			if (faceDirection && Math.Sign(target.X - base.X) != 0)
			{
				Sprite.Scale.X = Math.Sign(target.X - base.X);
			}
			Vector2 direction = (target - Position).SafeNormalize();
			Vector2 perp = new Vector2(0f - direction.Y, direction.X);
			float speed = 0f;
			while (Position != target)
			{
				speed = Calc.Approach(speed, FloatSpeed, FloatAccel * Engine.DeltaTime);
				Position = Calc.Approach(Position, target, speed * Engine.DeltaTime);
				Floatness = Calc.Approach(Floatness, 4f, 8f * Engine.DeltaTime);
				floatNormal = Calc.Approach(floatNormal, perp, Engine.DeltaTime * 12f);
				if (fadeLight)
				{
					Light.Alpha = Calc.Approach(Light.Alpha, 0f, Engine.DeltaTime * 2f);
				}
				yield return null;
			}
			if (quickEnd)
			{
				Floatness = 2f;
			}
			else
			{
				while (Floatness != 2f)
				{
					Floatness = Calc.Approach(Floatness, 2f, 8f * Engine.DeltaTime);
					yield return null;
				}
			}
			if (turnAtEndTo.HasValue)
			{
				Sprite.Scale.X = turnAtEndTo.Value;
			}
		}

		public IEnumerator WalkTo(float x, float speed = 64f)
		{
			Floatness = 0f;
			Sprite.Play("walk");
			if (Math.Sign(x - base.X) != 0)
			{
				Sprite.Scale.X = Math.Sign(x - base.X);
			}
			while (base.X != x)
			{
				base.X = Calc.Approach(base.X, x, Engine.DeltaTime * speed);
				yield return null;
			}
			Sprite.Play("idle");
		}

		public IEnumerator SmashBlock(Vector2 target)
		{
			SceneAs<Level>().Displacement.AddBurst(Position, 0.5f, 24f, 96f);
			Sprite.Play("dreamDashLoop");
			Vector2 from = Position;
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime * 6f)
			{
				Position = from + (target - from) * Ease.CubeOut(p2);
				yield return null;
			}
			base.Scene.Entities.FindFirst<DashBlock>().Break(Position, new Vector2(0f, -1f), playSound: false);
			Sprite.Play("idle");
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime * 4f)
			{
				Position = target + (from - target) * Ease.CubeOut(p2);
				yield return null;
			}
			Sprite.Play("fallSlow");
		}

		public override void Update()
		{
			if (Sprite.Scale.X != 0f)
			{
				Hair.Facing = (Facings)Math.Sign(Sprite.Scale.X);
			}
			base.Update();
		}

		public override void Render()
		{
			Vector2 was = Sprite.RenderPosition;
			Sprite.RenderPosition = Sprite.RenderPosition.Floor();
			base.Render();
			Sprite.RenderPosition = was;
		}
	}
}
