using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class SummitCloud : Entity
	{
		private Image image;

		private float diff;

		private Vector2 RenderPosition
		{
			get
			{
				Vector2 camera = (base.Scene as Level).Camera.Position + new Vector2(160f, 90f);
				Vector2 difference = Position + new Vector2(128f, 64f) / 2f - camera;
				return Position + difference * (0.1f + diff);
			}
		}

		public SummitCloud(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Depth = -10550;
			diff = Calc.Random.Range(0.1f, 0.2f);
			List<MTexture> imgs = GFX.Game.GetAtlasSubtextures("scenery/summitclouds/cloud");
			image = new Image(Calc.Random.Choose(imgs));
			image.CenterOrigin();
			image.Scale.X = Calc.Random.Choose(-1, 1);
			Add(image);
			SineWave sine = new SineWave(Calc.Random.Range(0.05f, 0.15f));
			sine.Randomize();
			sine.OnUpdate = delegate(float f)
			{
				image.Y = f * 8f;
			};
			Add(sine);
		}

		public override void Render()
		{
			Vector2 pos = Position;
			Position = RenderPosition;
			base.Render();
			Position = pos;
		}
	}
}
