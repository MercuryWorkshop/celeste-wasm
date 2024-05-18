namespace Monocle
{
	public abstract class Renderer
	{
		public bool Visible = true;

		public virtual void Update(Scene scene)
		{
		}

		public virtual void BeforeRender(Scene scene)
		{
		}

		public virtual void Render(Scene scene)
		{
		}

		public virtual void AfterRender(Scene scene)
		{
		}
	}
}
