using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class HeartGemDisplay : Component
	{
		public Vector2 Position;

		public Sprite[] Sprites;

		public Vector2 TargetPosition;

		private Image bg;

		private Wiggler rotateWiggler;

		private Coroutine routine;

		private Vector2 bounce;

		private Tween tween;

		private Color SpriteColor
		{
			get
			{
				return Sprites[0].Color;
			}
			set
			{
				for (int i = 0; i < Sprites.Length; i++)
				{
					Sprites[i].Color = value;
				}
			}
		}

		public HeartGemDisplay(int heartgem, bool hasGem)
			: base(active: true, visible: true)
		{
			Sprites = new Sprite[3];
			for (int i = 0; i < Sprites.Length; i++)
			{
				Sprites[i] = GFX.GuiSpriteBank.Create("heartgem" + i);
				Sprites[i].Visible = heartgem == i && hasGem;
				Sprites[i].Play("spin");
			}
			bg = new Image(GFX.Gui["collectables/heartgem/0/spin00"]);
			bg.Color = Color.Black;
			bg.CenterOrigin();
			rotateWiggler = Wiggler.Create(0.4f, 6f);
			rotateWiggler.UseRawDeltaTime = true;
			SimpleCurve curve = new SimpleCurve(Vector2.UnitY * 80f, Vector2.Zero, Vector2.UnitY * -160f);
			tween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.4f);
			tween.OnStart = delegate
			{
				SpriteColor = Color.Transparent;
			};
			tween.OnUpdate = delegate(Tween t)
			{
				bounce = curve.GetPoint(t.Eased);
				SpriteColor = Color.White * Calc.LerpClamp(0f, 1f, t.Percent * 1.5f);
			};
		}

		public void Wiggle()
		{
			rotateWiggler.Start();
			for (int i = 0; i < Sprites.Length; i++)
			{
				if (Sprites[i].Visible)
				{
					Sprites[i].Play("spin", restart: true);
					Sprites[i].SetAnimationFrame(19);
				}
			}
		}

		public void Appear(AreaMode mode)
		{
			tween.Start();
			routine = new Coroutine(AppearSequence(Sprites[(int)mode]));
			routine.UseRawDeltaTime = true;
		}

		public void SetCurrentMode(AreaMode mode, bool has)
		{
			for (int i = 0; i < Sprites.Length; i++)
			{
				Sprites[i].Visible = i == (int)mode && has;
			}
			if (!has)
			{
				routine = null;
			}
		}

		public override void Update()
		{
			base.Update();
			if (routine != null && routine.Active)
			{
				routine.Update();
			}
			if (rotateWiggler.Active)
			{
				rotateWiggler.Update();
			}
			for (int j = 0; j < Sprites.Length; j++)
			{
				if (Sprites[j].Active)
				{
					Sprites[j].Update();
				}
			}
			if (tween != null && tween.Active)
			{
				tween.Update();
			}
			Position = Calc.Approach(Position, TargetPosition, 200f * Engine.DeltaTime);
			for (int i = 0; i < Sprites.Length; i++)
			{
				Sprites[i].Scale.X = Calc.Approach(Sprites[i].Scale.X, 1f, 2f * Engine.DeltaTime);
				Sprites[i].Scale.Y = Calc.Approach(Sprites[i].Scale.Y, 1f, 2f * Engine.DeltaTime);
			}
		}

		public override void Render()
		{
			base.Render();
			bg.Position = base.Entity.Position + Position;
			for (int i = 0; i < Sprites.Length; i++)
			{
				if (Sprites[i].Visible)
				{
					Sprites[i].Rotation = rotateWiggler.Value * 30f * ((float)Math.PI / 180f);
					Sprites[i].Position = base.Entity.Position + Position + bounce;
					Sprites[i].Render();
				}
			}
		}

		private IEnumerator AppearSequence(Sprite sprite)
		{
			sprite.Play("idle");
			sprite.Visible = true;
			sprite.Scale = new Vector2(0.8f, 1.4f);
			yield return tween.Wait();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			sprite.Scale = new Vector2(1.4f, 0.8f);
			yield return 0.4f;
			sprite.CenterOrigin();
			rotateWiggler.Start();
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			sprite.Play("spin");
			routine = null;
		}
	}
}
