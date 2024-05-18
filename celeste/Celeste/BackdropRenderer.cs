using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class BackdropRenderer : Renderer
	{
		public Matrix Matrix = Matrix.Identity;

		public List<Backdrop> Backdrops = new List<Backdrop>();

		public float Fade;

		public Color FadeColor = Color.Black;

		private bool usingSpritebatch;

		public override void BeforeRender(Scene scene)
		{
			foreach (Backdrop backdrop in Backdrops)
			{
				backdrop.BeforeRender(scene);
			}
		}

		public override void Update(Scene scene)
		{
			foreach (Backdrop backdrop in Backdrops)
			{
				backdrop.Update(scene);
			}
		}

		public void Ended(Scene scene)
		{
			foreach (Backdrop backdrop in Backdrops)
			{
				backdrop.Ended(scene);
			}
		}

		public T Get<T>() where T : class
		{
			foreach (Backdrop bg in Backdrops)
			{
				if (bg is T)
				{
					return bg as T;
				}
			}
			return null;
		}

		public IEnumerable<T> GetEach<T>() where T : class
		{
			foreach (Backdrop bg in Backdrops)
			{
				if (bg is T)
				{
					yield return bg as T;
				}
			}
		}

		public IEnumerable<T> GetEach<T>(string tag) where T : class
		{
			foreach (Backdrop bg in Backdrops)
			{
				if (bg is T && bg.Tags.Contains(tag))
				{
					yield return bg as T;
				}
			}
		}

		public void StartSpritebatch(BlendState blendState)
		{
			if (!usingSpritebatch)
			{
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix);
			}
			usingSpritebatch = true;
		}

		public void EndSpritebatch()
		{
			if (usingSpritebatch)
			{
				Draw.SpriteBatch.End();
			}
			usingSpritebatch = false;
		}

		public override void Render(Scene scene)
		{
			BlendState blendState = BlendState.AlphaBlend;
			foreach (Backdrop backdrop in Backdrops)
			{
				if (backdrop.Visible)
				{
					if (backdrop is Parallax && (backdrop as Parallax).BlendState != blendState)
					{
						EndSpritebatch();
						blendState = (backdrop as Parallax).BlendState;
					}
					if (backdrop.UseSpritebatch && !usingSpritebatch)
					{
						StartSpritebatch(blendState);
					}
					if (!backdrop.UseSpritebatch && usingSpritebatch)
					{
						EndSpritebatch();
					}
					backdrop.Render(scene);
				}
			}
			if (Fade > 0f)
			{
				Draw.Rect(-10f, -10f, 340f, 200f, FadeColor * Fade);
			}
			EndSpritebatch();
		}

		public void Remove<T>() where T : Backdrop
		{
			for (int i = Backdrops.Count - 1; i >= 0; i--)
			{
				if (Backdrops[i].GetType() == typeof(T))
				{
					Backdrops.RemoveAt(i);
				}
			}
		}
	}
}
