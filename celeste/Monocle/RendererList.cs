using System.Collections.Generic;

namespace Monocle
{
	public class RendererList
	{
		public List<Renderer> Renderers;

		private List<Renderer> adding;

		private List<Renderer> removing;

		private Scene scene;

		internal RendererList(Scene scene)
		{
			this.scene = scene;
			Renderers = new List<Renderer>();
			adding = new List<Renderer>();
			removing = new List<Renderer>();
		}

		internal void UpdateLists()
		{
			if (adding.Count > 0)
			{
				foreach (Renderer renderer2 in adding)
				{
					Renderers.Add(renderer2);
				}
			}
			adding.Clear();
			if (removing.Count > 0)
			{
				foreach (Renderer renderer in removing)
				{
					Renderers.Remove(renderer);
				}
			}
			removing.Clear();
		}

		internal void Update()
		{
			foreach (Renderer renderer in Renderers)
			{
				renderer.Update(scene);
			}
		}

		internal void BeforeRender()
		{
			for (int i = 0; i < Renderers.Count; i++)
			{
				if (Renderers[i].Visible)
				{
					Draw.Renderer = Renderers[i];
					Renderers[i].BeforeRender(scene);
				}
			}
		}

		internal void Render()
		{
			for (int i = 0; i < Renderers.Count; i++)
			{
				if (Renderers[i].Visible)
				{
					Draw.Renderer = Renderers[i];
					Renderers[i].Render(scene);
				}
			}
		}

		internal void AfterRender()
		{
			for (int i = 0; i < Renderers.Count; i++)
			{
				if (Renderers[i].Visible)
				{
					Draw.Renderer = Renderers[i];
					Renderers[i].AfterRender(scene);
				}
			}
		}

		public void MoveToFront(Renderer renderer)
		{
			Renderers.Remove(renderer);
			Renderers.Add(renderer);
		}

		public void Add(Renderer renderer)
		{
			adding.Add(renderer);
		}

		public void Remove(Renderer renderer)
		{
			removing.Add(renderer);
		}
	}
}
