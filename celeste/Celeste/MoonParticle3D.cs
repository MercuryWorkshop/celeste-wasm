using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class MoonParticle3D : Entity
	{
		public class Particle : Billboard
		{
			public Vector3 Center;

			public Matrix Matrix;

			public float Rotation;

			public float Distance;

			public float YOff;

			public float Spd;

			public Particle(MTexture texture, Color color, Vector3 center, float size, Matrix matrix)
				: base(texture, Vector3.Zero, null, color)
			{
				Center = center;
				Matrix = matrix;
				Size = Vector2.One * Calc.Random.Range(0.05f, 0.15f) * size;
				Distance = Calc.Random.Range(1.8f, 1.9f);
				Rotation = Calc.Random.NextFloat((float)Math.PI * 2f);
				YOff = Calc.Random.Range(-0.1f, 0.1f);
				Spd = Calc.Random.Range(0.8f, 1.2f);
			}

			public override void Update()
			{
				Rotation += Engine.DeltaTime * 0.4f * Spd;
				Vector3 offset = new Vector3((float)Math.Cos(Rotation) * Distance, (float)Math.Sin(Rotation * 3f) * 0.25f + YOff, (float)Math.Sin(Rotation) * Distance);
				Position = Center + Vector3.Transform(offset, Matrix);
			}
		}

		private MountainModel model;

		private List<Particle> particles = new List<Particle>();

		public MoonParticle3D(MountainModel model, Vector3 center)
		{
			this.model = model;
			Visible = false;
			Matrix n = Matrix.CreateRotationZ(0.4f);
			Color[] col2 = new Color[2]
			{
				Calc.HexToColor("53f3dd"),
				Calc.HexToColor("53c9f3")
			};
			for (int l = 0; l < 20; l++)
			{
				Add(new Particle(OVR.Atlas["star"], Calc.Random.Choose(col2), center, 1f, n));
			}
			for (int k = 0; k < 30; k++)
			{
				Add(new Particle(OVR.Atlas["snow"], Calc.Random.Choose(col2), center, 0.3f, n));
			}
			Matrix m = Matrix.CreateRotationZ(0.8f) * Matrix.CreateRotationX(0.4f);
			Color[] col = new Color[2]
			{
				Calc.HexToColor("ab6ffa"),
				Calc.HexToColor("fa70ea")
			};
			for (int j = 0; j < 20; j++)
			{
				Add(new Particle(OVR.Atlas["star"], Calc.Random.Choose(col), center, 1f, m));
			}
			for (int i = 0; i < 30; i++)
			{
				Add(new Particle(OVR.Atlas["snow"], Calc.Random.Choose(col), center, 0.3f, m));
			}
		}

		public override void Update()
		{
			base.Update();
			Visible = model.StarEase > 0f;
		}
	}
}
