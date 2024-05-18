using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public static class ColorGrade
	{
		public static bool Enabled = true;

		private static MTexture from;

		private static MTexture to;

		private static float percent = 0f;

		public static Effect Effect => GFX.FxColorGrading;

		public static float Percent
		{
			get
			{
				return percent;
			}
			set
			{
				Set(from, to, value);
			}
		}

		public static void Set(MTexture grade)
		{
			Set(grade, grade, 0f);
		}

		public static void Set(MTexture fromTex, MTexture toTex, float p)
		{
			if (!Enabled || fromTex == null || toTex == null)
			{
				from = GFX.ColorGrades["none"];
				to = GFX.ColorGrades["none"];
			}
			else
			{
				from = fromTex;
				to = toTex;
			}
			percent = Calc.Clamp(p, 0f, 1f);
			if (from == to || percent <= 0f)
			{
				Effect.CurrentTechnique = Effect.Techniques["ColorGradeSingle"];
				Engine.Graphics.GraphicsDevice.Textures[1] = from.Texture.Texture;
				return;
			}
			if (percent >= 1f)
			{
				Effect.CurrentTechnique = Effect.Techniques["ColorGradeSingle"];
				Engine.Graphics.GraphicsDevice.Textures[1] = to.Texture.Texture;
				return;
			}
			Effect.CurrentTechnique = Effect.Techniques["ColorGrade"];
			Effect.Parameters["percent"].SetValue(percent);
			Engine.Graphics.GraphicsDevice.Textures[1] = from.Texture.Texture;
			Engine.Graphics.GraphicsDevice.Textures[2] = to.Texture.Texture;
		}
	}
}
