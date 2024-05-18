using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Gondola : Solid
	{
		private class Rope : Entity
		{
			public Gondola Gondola;

			public Rope()
			{
				base.Depth = 8999;
			}

			public override void Render()
			{
				Vector2 ropeA = (Gondola.LeftCliffside.Position + new Vector2(40f, -12f)).Floor();
				Vector2 ropeB = (Gondola.RightCliffside.Position + new Vector2(-40f, -4f)).Floor();
				Vector2 normal = (ropeB - ropeA).SafeNormalize();
				Vector2 gondolaA = Gondola.Position + new Vector2(0f, -55f) - normal * 6f;
				Vector2 gondolaB = Gondola.Position + new Vector2(0f, -55f) + normal * 6f;
				for (int i = 0; i < 2; i++)
				{
					Vector2 push = Vector2.UnitY * i;
					Draw.Line(ropeA + push, gondolaA + push, Color.Black);
					Draw.Line(gondolaB + push, ropeB + push, Color.Black);
				}
			}
		}

		public float Rotation;

		public float RotationSpeed;

		public Entity LeftCliffside;

		public Entity RightCliffside;

		private Entity back;

		private Image backImg;

		private Sprite front;

		public Sprite Lever;

		private Image top;

		private bool brokenLever;

		private bool inCliffside;

		public Vector2 Start { get; private set; }

		public Vector2 Destination { get; private set; }

		public Vector2 Halfway { get; private set; }

		public Gondola(EntityData data, Vector2 offset)
			: base(data.Position + offset, 64f, 8f, safe: true)
		{
			EnableAssistModeChecks = false;
			Add(front = GFX.SpriteBank.Create("gondola"));
			front.Play("idle");
			front.Origin = new Vector2(front.Width / 2f, 12f);
			front.Y = -52f;
			Add(top = new Image(GFX.Game["objects/gondola/top"]));
			top.Origin = new Vector2(top.Width / 2f, 12f);
			top.Y = -52f;
			Add(Lever = new Sprite(GFX.Game, "objects/gondola/lever"));
			Lever.Add("idle", "", 0f, default(int));
			Lever.Add("pulled", "", 0.5f, "idle", 1, 1);
			Lever.Origin = new Vector2(front.Width / 2f, 12f);
			Lever.Y = -52f;
			Lever.Play("idle");
			(base.Collider as Hitbox).Position.X = (0f - base.Collider.Width) / 2f;
			Start = Position;
			Destination = offset + data.Nodes[0];
			Halfway = (Position + Destination) / 2f;
			base.Depth = -10500;
			inCliffside = data.Bool("active", defaultValue: true);
			SurfaceSoundIndex = 28;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Add(back = new Entity(Position));
			back.Depth = 9000;
			backImg = new Image(GFX.Game["objects/gondola/back"]);
			backImg.Origin = new Vector2(backImg.Width / 2f, 12f);
			backImg.Y = -52f;
			back.Add(backImg);
			scene.Add(LeftCliffside = new Entity(Position + new Vector2(-124f, 0f)));
			Image img2 = new Image(GFX.Game["objects/gondola/cliffsideLeft"]);
			img2.JustifyOrigin(0f, 1f);
			LeftCliffside.Add(img2);
			LeftCliffside.Depth = 8998;
			scene.Add(RightCliffside = new Entity(Destination + new Vector2(144f, -104f)));
			Image img = new Image(GFX.Game["objects/gondola/cliffsideRight"]);
			img.JustifyOrigin(0f, 0.5f);
			img.Scale.X = -1f;
			RightCliffside.Add(img);
			RightCliffside.Depth = 8998;
			scene.Add(new Rope
			{
				Gondola = this
			});
			if (!inCliffside)
			{
				Position = Destination;
				Lever.Visible = false;
				UpdatePositions();
				JumpThru platform = new JumpThru(Position + new Vector2((0f - base.Width) / 2f, -36f), (int)base.Width, safe: true);
				platform.SurfaceSoundIndex = 28;
				base.Scene.Add(platform);
			}
			top.Rotation = Calc.Angle(Start, Destination);
		}

		public override void Update()
		{
			if (inCliffside)
			{
				float str = ((Math.Sign(Rotation) == Math.Sign(RotationSpeed)) ? 8f : 6f);
				if (Math.Abs(Rotation) < 0.5f)
				{
					str *= 0.5f;
				}
				if (Math.Abs(Rotation) < 0.25f)
				{
					str *= 0.5f;
				}
				RotationSpeed += (float)(-Math.Sign(Rotation)) * str * Engine.DeltaTime;
				Rotation += RotationSpeed * Engine.DeltaTime;
				Rotation = Calc.Clamp(Rotation, -0.4f, 0.4f);
				if (Math.Abs(Rotation) < 0.02f && Math.Abs(RotationSpeed) < 0.2f)
				{
					Rotation = (RotationSpeed = 0f);
				}
			}
			UpdatePositions();
			base.Update();
		}

		private void UpdatePositions()
		{
			back.Position = Position;
			backImg.Rotation = Rotation;
			front.Rotation = Rotation;
			if (!brokenLever)
			{
				Lever.Rotation = Rotation;
			}
			top.Rotation = Calc.Angle(Start, Destination);
		}

		public Vector2 GetRotatedFloorPositionAt(float x, float y = 52f)
		{
			Vector2 dir = Calc.AngleToVector(Rotation + (float)Math.PI / 2f, 1f);
			Vector2 perp = new Vector2(0f - dir.Y, dir.X);
			return Position + new Vector2(0f, -52f) + dir * y - perp * x;
		}

		public void BreakLever()
		{
			Add(new Coroutine(BreakLeverRoutine()));
		}

		private IEnumerator BreakLeverRoutine()
		{
			brokenLever = true;
			Vector2 speed = new Vector2(240f, -130f);
			while (true)
			{
				Lever.Position += speed * Engine.DeltaTime;
				Lever.Rotation += 2f * Engine.DeltaTime;
				speed.Y += 400f * Engine.DeltaTime;
				yield return null;
			}
		}

		public void PullSides()
		{
			front.Play("pull");
		}

		public void CancelPullSides()
		{
			front.Play("idle");
		}
	}
}
